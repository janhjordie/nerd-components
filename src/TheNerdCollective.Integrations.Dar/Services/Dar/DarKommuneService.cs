using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TheNerdCollective.Integrations.Dar.Configuration;
using TheNerdCollective.Integrations.Dar.GraphQL;
using TheNerdCollective.Integrations.Dar.Json;
using TheNerdCollective.Integrations.Dar.Mapping;
using TheNerdCollective.Integrations.Dar.Models;
using TheNerdCollective.Integrations.Dar.Services.Dar.Internal;
using TheNerdCollective.Integrations.Dar.Services.Internal;

namespace TheNerdCollective.Integrations.Dar.Services.Dar;

/// <summary>Kommuner via DAGI GraphQL med DAWA/REST/WFS-fallback.</summary>
public sealed class DarKommuneService
{
    private const int MinimumExpectedKommuneCount = 90;
    private static readonly int[] MicroCircleRadiiMeters = { 75, 250, 1000, 5000, 25000 };
    private readonly GraphQlDataAccessor _accessor;
    private readonly DawaKommuneClient _dawaClient;
    private readonly DagiRestKommuneClient _restClient;
    private readonly DagiWfsKommuneClient _wfsClient;
    private readonly DarRegionService _regionService;
    private readonly DarDagiOptions _dagiOptions;

    public DarKommuneService(
        GraphQlDataAccessor accessor,
        HttpClient httpClient,
        string apiKey,
        DarDagiOptions dagiOptions,
        DarRegionService regionService)
    {
        _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        _dagiOptions = dagiOptions ?? throw new ArgumentNullException(nameof(dagiOptions));
        _regionService = regionService ?? throw new ArgumentNullException(nameof(regionService));
        _dawaClient = new DawaKommuneClient(httpClient, dagiOptions);
        _restClient = new DagiRestKommuneClient(httpClient, apiKey, dagiOptions);
        _wfsClient = new DagiWfsKommuneClient(httpClient, apiKey, dagiOptions);
    }

    /// <summary>Returnerer alle aktuelle kommuner sorteret efter navn.</summary>
    public async Task<IReadOnlyList<KommuneDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (DagiKommuneCache.TryGetAll(_dagiOptions.KommuneListCacheDuration, out var cached))
        {
            return cached;
        }

        var graphQlKommuner = await TryGraphQlGetAllAsync(cancellationToken).ConfigureAwait(false);
        if (graphQlKommuner.Count >= MinimumExpectedKommuneCount)
        {
            var enriched = await EnrichFromGraphAsync(graphQlKommuner, cancellationToken).ConfigureAwait(false);
            return CacheAndReturn(enriched);
        }

        if (_dagiOptions.EnableDawaFallback)
        {
            var dawaKommuner = await _dawaClient.GetAllAsync(cancellationToken).ConfigureAwait(false);
            if (dawaKommuner.Count > 0)
            {
                return CacheAndReturn(dawaKommuner);
            }
        }

        var wfsKommuner = await _wfsClient.GetAllAsync(cancellationToken).ConfigureAwait(false);
        if (wfsKommuner.Count > 0)
        {
            var enriched = await EnrichExistingAsync(wfsKommuner, cancellationToken).ConfigureAwait(false);
            return CacheAndReturn(enriched);
        }

        throw new InvalidOperationException(DagiAccessHelp.EmptyKommuneResultMessage);
    }

    /// <summary>
    /// Finder kommunen for et punkt i WGS84 udelukkende via Datafordeler (GraphQL + REST DAGI).
    /// Kalder aldrig DAWA — uafhængigt af <see cref="DarDagiOptions.EnableDawaFallback"/>.
    /// </summary>
    public async Task<KommuneDto> FindByCoordinatesDatafordelerAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        ValidateWgs84Coordinates(latitude, longitude);

        var (easting, northing) = Etrs89Utm32NConverter.FromWgs84(latitude, longitude);

        var microCircleKommune = await TryFindByMicroCircleAsync(latitude, longitude, cancellationToken)
            .ConfigureAwait(false);
        if (microCircleKommune is not null)
        {
            return microCircleKommune;
        }

        var graphQlKommune = await TryGraphQlFindByWgs84Async(latitude, longitude, cancellationToken)
            .ConfigureAwait(false);
        if (graphQlKommune is not null)
        {
            return await EnrichRepresentativePointAsync(graphQlKommune, cancellationToken).ConfigureAwait(false);
        }

        var restKommune = await _restClient.FindByPointAsync(easting, northing, cancellationToken)
            .ConfigureAwait(false);
        if (restKommune is not null)
        {
            return await EnrichRepresentativePointAsync(restKommune, cancellationToken).ConfigureAwait(false);
        }

        var wfsKommune = await _wfsClient.FindByPointAsync(easting, northing, cancellationToken)
            .ConfigureAwait(false);
        if (wfsKommune is not null)
        {
            var enriched = await EnrichExistingAsync(new[] { wfsKommune }, cancellationToken).ConfigureAwait(false);
            wfsKommune = enriched.FirstOrDefault() ?? wfsKommune;
            return await EnrichRepresentativePointAsync(wfsKommune, cancellationToken).ConfigureAwait(false);
        }

        throw new InvalidOperationException(DagiAccessHelp.PointLookupFailedMessage);
    }

    /// <summary>Finder kommuner hvis geometri intersecter den angivne WKT-polygon (EPSG:25832).</summary>
    public Task<IReadOnlyList<KommuneDto>> FindKommunerByGeometryAsync(
        string polygonWkt,
        CancellationToken cancellationToken = default) =>
        FindKommunerByGeometryAsync(polygonWkt, includeWfsFallback: true, cancellationToken);

    internal async Task<IReadOnlyList<KommuneDto>> FindKommunerByGeometryAsync(
        string polygonWkt,
        bool includeWfsFallback,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(polygonWkt))
        {
            throw new ArgumentException("Polygon WKT må ikke være tom.", nameof(polygonWkt));
        }

        var temporal = GraphQlDataAccessor.CreateTemporalVariables();
        var nodes = await _accessor.FetchAllDagiNodesAsync(
            GraphQlQueries.FindKommunerByGeometry,
            after => new GeometryListVariables(polygonWkt, temporal.Virkningstid, temporal.Registreringstid, after),
            "DAGI_Kommuneinddeling",
            cancellationToken).ConfigureAwait(false);

        var graphKommuner = DarJsonSerializer.DeserializeList<KommuneGraphDto>(nodes);
        if (graphKommuner.Count > 0)
        {
            return await EnrichFromGraphAsync(graphKommuner, cancellationToken).ConfigureAwait(false);
        }

        if (!includeWfsFallback)
        {
            return Array.Empty<KommuneDto>();
        }

        var wfsKommuner = await _wfsClient.FindByGeometryAsync(polygonWkt, cancellationToken).ConfigureAwait(false);
        if (wfsKommuner.Count > 0)
        {
            return await EnrichExistingAsync(wfsKommuner, cancellationToken).ConfigureAwait(false);
        }

        return Array.Empty<KommuneDto>();
    }

    /// <summary>
    /// Finder kommunen for et punkt i WGS84 (EPSG:4326), fx fra browserens Geolocation API
    /// (<c>position.coords.latitude</c> / <c>longitude</c>).
    /// </summary>
    public async Task<KommuneDto> FindByCoordinatesAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        ValidateWgs84Coordinates(latitude, longitude);

        try
        {
            return await FindByCoordinatesDatafordelerAsync(latitude, longitude, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (InvalidOperationException)
        {
            // GraphQL + REST DAGI did not resolve the point — try WFS, then optional DAWA.
        }

        var (easting, northing) = Etrs89Utm32NConverter.FromWgs84(latitude, longitude);
        try
        {
            return await FindByEtrs89InternalAsync(easting, northing, cancellationToken).ConfigureAwait(false);
        }
        catch (InvalidOperationException)
        {
        }

        var microCircleKommune = await TryFindByMicroCircleAsync(latitude, longitude, cancellationToken)
            .ConfigureAwait(false);
        if (microCircleKommune is not null)
        {
            return microCircleKommune;
        }

        var nearestReprKommune = await TryFindByNearestRepresentativeFallbackAsync(
                latitude,
                longitude,
                cancellationToken)
            .ConfigureAwait(false);
        if (nearestReprKommune is not null)
        {
            return nearestReprKommune;
        }

        if (_dagiOptions.EnableDawaFallback)
        {
            var dawaKommune = await _dawaClient.FindByWgs84Async(latitude, longitude, cancellationToken)
                .ConfigureAwait(false);
            if (dawaKommune is not null)
            {
                return dawaKommune;
            }
        }

        throw new InvalidOperationException(DagiAccessHelp.PointLookupFailedMessage);
    }

    /// <summary>
    /// Som <see cref="FindByCoordinatesAsync"/>, men med beslutnings-trace (repr-afstande, polygon-match, override).
    /// </summary>
    public async Task<(KommuneDto Kommune, KommuneCoordinateResolutionDiagnostics Diagnostics)> FindByCoordinatesWithDiagnosticsAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        ValidateWgs84Coordinates(latitude, longitude);

        var kommune = await FindByCoordinatesAsync(latitude, longitude, cancellationToken).ConfigureAwait(false);
        var diagnostics = await BuildCoordinateResolutionDiagnosticsAsync(
                latitude,
                longitude,
                kommune,
                cancellationToken)
            .ConfigureAwait(false);

        return (kommune, diagnostics);
    }

    /// <summary>Finder kommunen for et punkt i ETRS89 UTM zone 32N (EPSG:25832).</summary>
    public async Task<KommuneDto> FindByEtrs89Async(
        double easting,
        double northing,
        CancellationToken cancellationToken = default)
    {
        var (latitude, longitude) = Etrs89Utm32NConverter.ToWgs84(easting, northing);
        var graphQlKommune = await TryGraphQlFindByPointAsync(latitude, longitude, easting, northing, cancellationToken)
            .ConfigureAwait(false);
        if (graphQlKommune is not null)
        {
            return await EnrichRepresentativePointAsync(graphQlKommune, cancellationToken).ConfigureAwait(false);
        }

        try
        {
            return await FindByEtrs89InternalAsync(easting, northing, cancellationToken).ConfigureAwait(false);
        }
        catch (InvalidOperationException)
        {
        }

        if (_dagiOptions.EnableDawaFallback)
        {
            var dawaKommune = await _dawaClient.FindByEtrs89Async(easting, northing, cancellationToken)
                .ConfigureAwait(false);
            if (dawaKommune is not null)
            {
                return dawaKommune;
            }
        }

        throw new InvalidOperationException(DagiAccessHelp.PointLookupFailedMessage);
    }

    private async Task<KommuneDto> FindByEtrs89InternalAsync(
        double easting,
        double northing,
        CancellationToken cancellationToken)
    {
        var restKommune = await _restClient.FindByPointAsync(easting, northing, cancellationToken)
            .ConfigureAwait(false);
        if (restKommune is not null)
        {
            return restKommune;
        }

        var wfsKommune = await _wfsClient.FindByPointAsync(easting, northing, cancellationToken)
            .ConfigureAwait(false);
        if (wfsKommune is not null)
        {
            var enriched = await EnrichExistingAsync(new[] { wfsKommune }, cancellationToken).ConfigureAwait(false);
            return enriched.FirstOrDefault() ?? wfsKommune;
        }

        throw new InvalidOperationException(DagiAccessHelp.PointLookupFailedMessage);
    }

    private async Task<KommuneDto?> TryGraphQlFindByWgs84Async(
        double latitude,
        double longitude,
        CancellationToken cancellationToken)
    {
        var (easting, northing) = Etrs89Utm32NConverter.FromWgs84(latitude, longitude);
        return await TryGraphQlFindByPointAsync(latitude, longitude, easting, northing, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<KommuneDto?> TryFindByNearestRepresentativeFallbackAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken)
    {
        if (_dagiOptions.NearestRepresentativeFallbackMaxKilometers <= 0)
        {
            return null;
        }

        var maxMeters = _dagiOptions.NearestRepresentativeFallbackMaxKilometers * 1000;
        var kommuner = await SafeGetAllKommunerAsync(cancellationToken).ConfigureAwait(false);
        if (kommuner.Count == 0)
        {
            return null;
        }

        return KommuneRepresentativePointHelper.TryFindNearestByRepresentativePoint(
            latitude,
            longitude,
            kommuner,
            maxMeters);
    }

    private async Task<IReadOnlyList<KommuneDto>> SafeGetAllKommunerAsync(CancellationToken cancellationToken)
    {
        if (DagiKommuneCache.TryGetAll(_dagiOptions.KommuneListCacheDuration, out var cached) && cached.Count > 0)
        {
            return cached;
        }

        try
        {
            return await GetAllAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (InvalidOperationException)
        {
            return Array.Empty<KommuneDto>();
        }
    }

    private IReadOnlyList<KommuneDto> CacheAndReturn(IReadOnlyList<KommuneDto> kommuner)
    {
        DagiKommuneCache.SetAll(kommuner, _dagiOptions.KommuneListCacheDuration);
        return kommuner;
    }

    private async Task<IReadOnlyList<KommuneGraphDto>> TryGraphQlGetAllAsync(CancellationToken cancellationToken)
    {
        var temporal = GraphQlDataAccessor.CreateTemporalVariables();
        var nodes = await _accessor.FetchAllDagiNodesAsync(
            GraphQlQueries.GetAllKommuner,
            after => new KommuneListVariables(temporal.Virkningstid, temporal.Registreringstid, after),
            "DAGI_Kommuneinddeling",
            cancellationToken).ConfigureAwait(false);

        return DarJsonSerializer.DeserializeList<KommuneGraphDto>(nodes);
    }

    private async Task<IReadOnlyList<KommuneDto>> EnrichFromGraphAsync(
        IReadOnlyList<KommuneGraphDto> graphKommuner,
        CancellationToken cancellationToken)
    {
        var regioner = await SafeGetRegionsAsync(cancellationToken).ConfigureAwait(false);
        IReadOnlyList<KommuneDto>? dawaKommuner = null;

        dawaKommuner = await TryGetDawaKommunerForEnrichmentAsync(cancellationToken).ConfigureAwait(false);

        return KommuneRegionEnricher.EnrichFromGraph(graphKommuner, regioner, dawaKommuner);
    }

    private async Task<IReadOnlyList<KommuneDto>> EnrichExistingAsync(
        IReadOnlyList<KommuneDto> kommuner,
        CancellationToken cancellationToken)
    {
        var regioner = await SafeGetRegionsAsync(cancellationToken).ConfigureAwait(false);
        IReadOnlyList<KommuneDto>? dawaKommuner = null;

        dawaKommuner = await TryGetDawaKommunerForEnrichmentAsync(cancellationToken).ConfigureAwait(false);

        return KommuneRegionEnricher.EnrichExisting(kommuner, regioner, dawaKommuner);
    }

    private async Task<IReadOnlyList<RegionDto>> SafeGetRegionsAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _regionService.GetAllAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (InvalidOperationException)
        {
            return Array.Empty<RegionDto>();
        }
    }

    private async Task<KommuneDto?> TryGraphQlFindByPointAsync(
        double queryLatitude,
        double queryLongitude,
        double easting,
        double northing,
        CancellationToken cancellationToken)
    {
        var wkt = FormatPointWkt(easting, northing);
        var temporal = GraphQlDataAccessor.CreateTemporalVariables();
        var variables = new KommuneByPointVariables(wkt, temporal.Virkningstid, temporal.Registreringstid);

        var nodes = await _accessor.FetchDagiNodesAsync(
            GraphQlQueries.FindKommuneByPoint,
            variables,
            "DAGI_Kommuneinddeling",
            cancellationToken).ConfigureAwait(false);

        if (nodes.GetArrayLength() == 0)
        {
            return null;
        }

        var candidates = new List<KommuneGraphDto>(nodes.GetArrayLength());
        for (var index = 0; index < nodes.GetArrayLength(); index++)
        {
            candidates.Add(DarJsonSerializer.DeserializeRequired<KommuneGraphDto>(nodes[index]));
        }

        var selectedGraph = await SelectGraphContainingPointAsync(
                candidates,
                queryLatitude,
                queryLongitude,
                easting,
                northing,
                cancellationToken)
            .ConfigureAwait(false);
        if (selectedGraph is null)
        {
            return null;
        }

        var enriched = await EnrichFromGraphAsync(new[] { selectedGraph }, cancellationToken).ConfigureAwait(false);
        var kommune = enriched.FirstOrDefault();
        if (kommune is null)
        {
            return null;
        }

        return await EnrichRepresentativePointAsync(kommune, cancellationToken).ConfigureAwait(false);
    }

    private async Task<KommuneGraphDto?> SelectGraphContainingPointAsync(
        IReadOnlyList<KommuneGraphDto> candidates,
        double queryLatitude,
        double queryLongitude,
        double easting,
        double northing,
        CancellationToken cancellationToken)
    {
        var containing = new List<KommuneGraphDto>();
        foreach (var candidate in candidates)
        {
            var geometryGraph = await TryFetchKommuneGraphWithGeometryAsync(candidate, cancellationToken)
                .ConfigureAwait(false);
            if (geometryGraph is null)
            {
                continue;
            }

            if (WktPointInPolygonHelper.ContainsEtrs89(geometryGraph.Geometri?.Wkt, easting, northing))
            {
                containing.Add(geometryGraph);
            }
        }

        if (containing.Count == 0)
        {
            return null;
        }

        if (containing.Count == 1)
        {
            return containing[0];
        }

        return containing
            .OrderBy(candidate => WktCentroidHelper.TryGetAbsoluteAreaSquareMeters(candidate.Geometri?.Wkt) ?? double.MaxValue)
            .ThenBy(candidate => candidate.Kommunekode ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .First();
    }

    private async Task<KommuneGraphDto?> TryFetchKommuneGraphWithGeometryAsync(
        KommuneGraphDto candidate,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(candidate.IdLokalId))
        {
            return string.IsNullOrWhiteSpace(candidate.Geometri?.Wkt) ? null : candidate;
        }

        var temporal = GraphQlDataAccessor.CreateTemporalVariables();
        var nodes = await _accessor.FetchDagiNodesAsync(
            GraphQlQueries.GetKommuneById,
            new KommuneByIdVariables(candidate.IdLokalId!, temporal.Virkningstid, temporal.Registreringstid),
            "DAGI_Kommuneinddeling",
            cancellationToken).ConfigureAwait(false);

        if (nodes.GetArrayLength() == 0)
        {
            return null;
        }

        return DarJsonSerializer.DeserializeRequired<KommuneGraphDto>(nodes[0]);
    }

    private async Task<KommuneDto?> TryFindByMicroCircleAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken)
    {
        var (easting, northing) = Etrs89Utm32NConverter.FromWgs84(latitude, longitude);

        foreach (var radiusMeters in MicroCircleRadiiMeters)
        {
            var polygonWkt = GeoCircleHelper.CreateCirclePolygonWkt(longitude, latitude, radiusMeters);
            var kommuner = await FindKommunerByGeometryAsync(polygonWkt, cancellationToken).ConfigureAwait(false);
            if (kommuner.Count == 0)
            {
                continue;
            }

            var graphCandidates = new List<KommuneGraphDto>();
            foreach (var kommune in kommuner)
            {
                if (string.IsNullOrWhiteSpace(kommune.IdLokalId))
                {
                    continue;
                }

                var graph = await TryFetchKommuneGraphWithGeometryAsync(
                    new KommuneGraphDto { IdLokalId = kommune.IdLokalId, Kommunekode = kommune.Kommunekode, Navn = kommune.Navn },
                    cancellationToken).ConfigureAwait(false);

                if (graph is not null)
                {
                    graphCandidates.Add(graph);
                }
            }

            var selectedGraph = await SelectGraphContainingPointAsync(
                    graphCandidates,
                    latitude,
                    longitude,
                    easting,
                    northing,
                    cancellationToken)
                .ConfigureAwait(false);
            if (selectedGraph is not null)
            {
                var enriched = await EnrichFromGraphAsync(new[] { selectedGraph }, cancellationToken).ConfigureAwait(false);
                var resolved = enriched.FirstOrDefault();
                if (resolved is not null)
                {
                    return resolved;
                }
            }
        }

        return null;
    }

    private async Task<IReadOnlyList<KommuneDto>?> TryGetDawaKommunerForEnrichmentAsync(CancellationToken cancellationToken)
    {
        if (!_dagiOptions.EnableDawaVisualCenterEnrichment && !_dagiOptions.EnableDawaFallback)
        {
            return null;
        }

        try
        {
            return await _dawaClient.GetAllAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private async Task<KommuneDto> EnrichRepresentativePointAsync(
        KommuneDto kommune,
        CancellationToken cancellationToken)
    {
        if (kommune.RepræsentativPunktLatitude is not null && kommune.RepræsentativPunktLongitude is not null)
        {
            return kommune;
        }

        var dawaKommuner = await TryGetDawaKommunerForEnrichmentAsync(cancellationToken).ConfigureAwait(false);
        if (dawaKommuner is not null && !string.IsNullOrWhiteSpace(kommune.Kommunekode))
        {
            var dawa = dawaKommuner.FirstOrDefault(k =>
                string.Equals(k.Kommunekode, kommune.Kommunekode, StringComparison.Ordinal));
            if (dawa?.RepræsentativPunktLatitude is double dawaLat && dawa.RepræsentativPunktLongitude is double dawaLng)
            {
                return kommune with
                {
                    RepræsentativPunktLatitude = dawaLat,
                    RepræsentativPunktLongitude = dawaLng
                };
            }
        }

        if (string.IsNullOrWhiteSpace(kommune.IdLokalId))
        {
            return kommune;
        }

        var kommuneId = kommune.IdLokalId!;
        var temporal = GraphQlDataAccessor.CreateTemporalVariables();
        var nodes = await _accessor.FetchDagiNodesAsync(
            GraphQlQueries.GetKommuneById,
            new KommuneByIdVariables(kommuneId, temporal.Virkningstid, temporal.Registreringstid),
            "DAGI_Kommuneinddeling",
            cancellationToken).ConfigureAwait(false);

        if (nodes.GetArrayLength() == 0)
        {
            return kommune;
        }

        var graph = DarJsonSerializer.DeserializeRequired<KommuneGraphDto>(nodes[0]);
        var centroid = WktCentroidHelper.TryGetCentroidWgs84(graph.Geometri?.Wkt);
        if (centroid is null)
        {
            return kommune;
        }

        return kommune with
        {
            RepræsentativPunktLatitude = centroid.Value.Latitude,
            RepræsentativPunktLongitude = centroid.Value.Longitude
        };
    }

    private async Task<KommuneCoordinateResolutionDiagnostics> BuildCoordinateResolutionDiagnosticsAsync(
        double latitude,
        double longitude,
        KommuneDto resolved,
        CancellationToken cancellationToken)
    {
        var polygonMatch = await TryGraphQlFindByWgs84Async(latitude, longitude, cancellationToken).ConfigureAwait(false);

        var polygonEnriched = polygonMatch is not null
            ? await EnrichRepresentativePointAsync(polygonMatch, cancellationToken).ConfigureAwait(false)
            : null;
        var polygonDistance = polygonEnriched is null
            ? null
            : KommuneRepresentativePointHelper.TryGetDistanceToRepresentativeMeters(latitude, longitude, polygonEnriched);

        var allKommuner = await SafeGetAllKommunerAsync(cancellationToken).ConfigureAwait(false);
        var nearest = KommuneRepresentativePointHelper.TryFindNearestByRepresentativePoint(
            latitude,
            longitude,
            allKommuner,
            double.MaxValue);
        var nearestDistance = nearest is null
            ? null
            : KommuneRepresentativePointHelper.TryGetDistanceToRepresentativeMeters(latitude, longitude, nearest);

        var snapMeters = _dagiOptions.RepresentativeProximitySnapMeters;
        var snapApplied = nearestDistance is not null
            && nearestDistance.Value <= snapMeters
            && string.Equals(nearest?.Kommunekode, resolved.Kommunekode, StringComparison.Ordinal);

        var topNearest = allKommuner
            .Select(k =>
            {
                var distance = KommuneRepresentativePointHelper.TryGetDistanceToRepresentativeMeters(latitude, longitude, k);
                return distance is null
                    ? null
                    : new KommuneReprDistanceSnapshot
                    {
                        Kommunekode = k.Kommunekode,
                        Navn = k.Navn,
                        DistanceMeters = Math.Round(distance.Value, 2)
                    };
            })
            .Where(s => s is not null)
            .Cast<KommuneReprDistanceSnapshot>()
            .OrderBy(s => s.DistanceMeters)
            .Take(8)
            .ToList();

        return new KommuneCoordinateResolutionDiagnostics
        {
            QueryLatitude = latitude,
            QueryLongitude = longitude,
            ResolvedKommunekode = resolved.Kommunekode,
            ResolvedNavn = resolved.Navn,
            ResolutionPath = "FindByCoordinatesAsync",
            PolygonMatchKommunekode = polygonEnriched?.Kommunekode,
            PolygonMatchReprDistanceMeters = polygonDistance,
            NearestReprKommunekode = nearest?.Kommunekode,
            NearestReprDistanceMeters = nearestDistance,
            RepresentativeProximityOverrideApplied = snapApplied,
            RepresentativeProximitySnapMeters = snapMeters,
            TopNearestRepresentativePoints = topNearest
        };
    }

    private static void ValidateWgs84Coordinates(double latitude, double longitude)
    {
        if (latitude is < -90 or > 90)
        {
            throw new ArgumentOutOfRangeException(nameof(latitude), latitude, "Breddegrad skal være mellem -90 og 90.");
        }

        if (longitude is < -180 or > 180)
        {
            throw new ArgumentOutOfRangeException(nameof(longitude), longitude, "Længdegrad skal være mellem -180 og 180.");
        }
    }

    private static string FormatPointWkt(double easting, double northing) =>
        $"POINT ({easting.ToString("0.########", CultureInfo.InvariantCulture)} {northing.ToString("0.########", CultureInfo.InvariantCulture)})";

    private sealed class KommuneByIdVariables
    {
        public KommuneByIdVariables(string kommuneId, string virkningstid, string registreringstid)
        {
            KommuneId = kommuneId;
            Virkningstid = virkningstid;
            Registreringstid = registreringstid;
        }

        public string KommuneId { get; }

        public string Virkningstid { get; }

        public string Registreringstid { get; }
    }

    private sealed class KommuneByPointVariables
    {
        public KommuneByPointVariables(string wkt, string virkningstid, string registreringstid)
        {
            Wkt = wkt;
            Virkningstid = virkningstid;
            Registreringstid = registreringstid;
        }

        public string Wkt { get; }

        public string Virkningstid { get; }

        public string Registreringstid { get; }
    }

    private sealed class KommuneListVariables
    {
        public KommuneListVariables(string virkningstid, string registreringstid, string? after)
        {
            Virkningstid = virkningstid;
            Registreringstid = registreringstid;
            After = after;
        }

        public string Virkningstid { get; }

        public string Registreringstid { get; }

        public string? After { get; }
    }

    private sealed class GeometryListVariables
    {
        public GeometryListVariables(string wkt, string virkningstid, string registreringstid, string? after)
        {
            Wkt = wkt;
            Virkningstid = virkningstid;
            Registreringstid = registreringstid;
            After = after;
        }

        public string Wkt { get; }

        public string Virkningstid { get; }

        public string Registreringstid { get; }

        public string? After { get; }
    }
}
