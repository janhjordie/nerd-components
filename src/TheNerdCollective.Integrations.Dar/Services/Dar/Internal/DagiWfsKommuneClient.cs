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
            5,
            cancellationToken).ConfigureAwait(false);

        return SelectBestPointMatch(features, easting, northing);
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
        double easting,
        double northing)
    {
        if (features.Count == 0)
        {
            return null;
        }

        KommuneDto? best = null;
        double bestDistanceSquared = double.MaxValue;

        foreach (var feature in features)
        {
            var mapped = ApplyRepresentativePointFromFeature(feature);
            if (mapped?.RepræsentativPunktLatitude is null || mapped.RepræsentativPunktLongitude is null)
            {
                if (best is null)
                {
                    best = mapped;
                }

                continue;
            }

            var (candidateEasting, candidateNorthing) = Etrs89Utm32NConverter.FromWgs84(
                mapped.RepræsentativPunktLatitude.Value,
                mapped.RepræsentativPunktLongitude.Value);

            var distanceSquared = GeoPointHelper.SquaredDistanceEtrs89(
                candidateEasting,
                candidateNorthing,
                easting,
                northing);

            if (distanceSquared < bestDistanceSquared)
            {
                bestDistanceSquared = distanceSquared;
                best = mapped;
            }
        }

        return best;
    }

    private static KommuneDto? ApplyRepresentativePointFromFeature(JsonElement feature)
    {
        var mapped = DagiKommuneJsonParser.ParseKommuneList(feature);
        return mapped.FirstOrDefault();
    }

    private static string FormatPointWkt(double easting, double northing) =>
        $"POINT({easting.ToString("0.########", CultureInfo.InvariantCulture)} {northing.ToString("0.########", CultureInfo.InvariantCulture)})";
}
