using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TheNerdCollective.Integrations.Dar.Configuration;
using TheNerdCollective.Integrations.Dar.Mapping;
using TheNerdCollective.Integrations.Dar.Models;

namespace TheNerdCollective.Integrations.Dar.Services.Dar.Internal;

/// <summary>WFS-fallback til kommune-liste og spatial opslag når GraphQL er tom (samme API-nøgle).</summary>
internal sealed class DagiWfsKommuneClient
{
    private const string FeatureType = "dagi_v001:kommuneinddeling_current";
    private const int PageSize = 200;

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly DarDagiOptions _options;

    public DagiWfsKommuneClient(HttpClient httpClient, string apiKey, DarDagiOptions options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    internal async Task<IReadOnlyList<KommuneDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = new List<KommuneDto>();
        var startIndex = 0;

        while (true)
        {
            var url =
                $"{(_options.WfsUrl ?? DarDagiOptions.DefaultWfsUrl).TrimEnd('?')}" +
                $"?service=WFS&version=2.0.0&request=GetFeature" +
                $"&typeNames={Uri.EscapeDataString(FeatureType)}" +
                $"&outputFormat=application/json" +
                $"&count={PageSize}" +
                $"&startIndex={startIndex}" +
                $"&apiKey={Uri.EscapeDataString(_apiKey)}";

            using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"DAGI WFS returnerede HTTP {(int)response.StatusCode}: {body}");
            }

            using var document = JsonDocument.Parse(body);
            var page = DagiKommuneJsonParser.ParseKommuneList(document.RootElement);
            if (page.Count == 0)
            {
                break;
            }

            items.AddRange(page);

            if (page.Count < PageSize)
            {
                break;
            }

            startIndex += PageSize;
        }

        return items
            .GroupBy(k => k.Kommunekode ?? k.IdLokalId ?? k.Navn ?? string.Empty, StringComparer.Ordinal)
            .Select(g => g.First())
            .OrderBy(k => k.Navn, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    internal async Task<KommuneDto?> FindByPointAsync(
        double easting,
        double northing,
        CancellationToken cancellationToken = default)
    {
        var wkt = FormatPointWkt(easting, northing);
        var features = await FetchByCqlAsync(
            $"INTERSECTS(geometri, {wkt})",
            100,
            cancellationToken).ConfigureAwait(false);

        var (queryLatitude, queryLongitude) = Etrs89Utm32NConverter.ToWgs84(easting, northing);
        return SelectBestPointMatch(features, queryLatitude, queryLongitude, easting, northing);
    }

    internal async Task<IReadOnlyList<KommuneDto>> FindByGeometryAsync(
        string polygonWkt,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(polygonWkt))
        {
            return Array.Empty<KommuneDto>();
        }

        var features = await FetchByCqlAsync(
            $"INTERSECTS(geometri, {polygonWkt})",
            PageSize,
            cancellationToken).ConfigureAwait(false);

        return features
            .Select(ApplyRepresentativePointFromFeature)
            .Where(k => k is not null)
            .Select(k => k!)
            .GroupBy(k => k.Kommunekode ?? k.IdLokalId ?? k.Navn ?? string.Empty, StringComparer.Ordinal)
            .Select(g => g.First())
            .OrderBy(k => k.Navn, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private async Task<IReadOnlyList<JsonElement>> FetchByCqlAsync(
        string cqlFilter,
        int count,
        CancellationToken cancellationToken)
    {
        var url =
            $"{(_options.WfsUrl ?? DarDagiOptions.DefaultWfsUrl).TrimEnd('?')}" +
            $"?service=WFS&version=2.0.0&request=GetFeature" +
            $"&typeNames={Uri.EscapeDataString(FeatureType)}" +
            $"&outputFormat=application/json" +
            $"&count={count}" +
            $"&CQL_FILTER={Uri.EscapeDataString(cqlFilter)}" +
            $"&apiKey={Uri.EscapeDataString(_apiKey)}";

        using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<JsonElement>();
        }

        using var document = JsonDocument.Parse(body);
        if (!document.RootElement.TryGetProperty("features", out var features)
            || features.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<JsonElement>();
        }

        var items = new List<JsonElement>();
        foreach (var feature in features.EnumerateArray())
        {
            items.Add(feature.Clone());
        }

        return items;
    }

    private static KommuneDto? SelectBestPointMatch(
        IReadOnlyList<JsonElement> features,
        double queryLatitude,
        double queryLongitude,
        double easting,
        double northing)
    {
        if (features.Count == 0)
        {
            return null;
        }

        var containing = new List<(KommuneDto Kommune, JsonElement Feature)>();
        foreach (var feature in features)
        {
            if (!WktPointInPolygonHelper.GeoJsonFeatureContainsEtrs89(feature, easting, northing))
            {
                continue;
            }

            var mapped = ApplyRepresentativePointFromFeature(feature);
            if (mapped is null)
            {
                continue;
            }

            containing.Add((mapped, feature));
        }

        if (containing.Count == 0)
        {
            return null;
        }

        if (containing.Count == 1)
        {
            return containing[0].Kommune;
        }

        return containing
            .OrderBy(pair => TryGetGeoJsonFeatureAreaSquareMeters(pair.Feature) ?? double.MaxValue)
            .ThenBy(pair => pair.Kommune.Kommunekode ?? string.Empty, StringComparer.Ordinal)
            .First()
            .Kommune;
    }

    private static double? TryGetGeoJsonFeatureAreaSquareMeters(JsonElement feature)
    {
        if (!feature.TryGetProperty("geometry", out var geometry)
            || !geometry.TryGetProperty("coordinates", out var coordinates))
        {
            return null;
        }

        if (!geometry.TryGetProperty("type", out var typeElement))
        {
            return null;
        }

        var type = typeElement.GetString();
        if (string.Equals(type, "Polygon", StringComparison.OrdinalIgnoreCase))
        {
            return AbsoluteAreaFromGeoJsonRing(coordinates);
        }

        if (string.Equals(type, "MultiPolygon", StringComparison.OrdinalIgnoreCase)
            && coordinates.ValueKind == JsonValueKind.Array)
        {
            double? maxArea = null;
            foreach (var polygon in coordinates.EnumerateArray())
            {
                var area = AbsoluteAreaFromGeoJsonRing(polygon);
                if (area is not null)
                {
                    maxArea = maxArea is null ? area : Math.Max(maxArea.Value, area.Value);
                }
            }

            return maxArea;
        }

        return null;
    }

    private static double? AbsoluteAreaFromGeoJsonRing(JsonElement polygonCoordinates)
    {
        if (polygonCoordinates.ValueKind != JsonValueKind.Array || polygonCoordinates.GetArrayLength() == 0)
        {
            return null;
        }

        var ring = polygonCoordinates[0];
        if (ring.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var points = new List<(double X, double Y)>();
        foreach (var point in ring.EnumerateArray())
        {
            if (point.ValueKind != JsonValueKind.Array || point.GetArrayLength() < 2)
            {
                continue;
            }

            points.Add((point[0].GetDouble(), point[1].GetDouble()));
        }

        if (points.Count < 3)
        {
            return null;
        }

        double area = 0;
        for (var index = 0; index < points.Count - 1; index++)
        {
            var (x0, y0) = points[index];
            var (x1, y1) = points[index + 1];
            area += x0 * y1 - x1 * y0;
        }

        return Math.Abs(area * 0.5);
    }

    private static string? TryGetFeatureGeometryWkt(JsonElement feature)
    {
        if (!feature.TryGetProperty("geometry", out var geometry))
        {
            return null;
        }

        return geometry.GetRawText();
    }

    private static KommuneDto? ApplyRepresentativePointFromFeature(JsonElement feature)
    {
        var mapped = DagiKommuneJsonParser.ParseKommuneList(feature);
        return mapped.FirstOrDefault();
    }

    private static string FormatPointWkt(double easting, double northing) =>
        $"POINT({easting.ToString("0.########", CultureInfo.InvariantCulture)} {northing.ToString("0.########", CultureInfo.InvariantCulture)})";
}
