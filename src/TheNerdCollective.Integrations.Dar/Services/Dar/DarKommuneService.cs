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

        var graphQlKommune = await TryGraphQlFindByWgs84Async(latitude, longitude, cancellationToken)
            .ConfigureAwait(false);
        if (graphQlKommune is not null)
        {
            return await EnrichRepresentativePointAsync(graphQlKommune, cancellationToken).ConfigureAwait(false);
        }

        var (easting, northing) = Etrs89Utm32NConverter.FromWgs84(latitude, longitude);
        var restKommune = await _restClient.FindByPointAsync(easting, northing, cancellationToken)
            .ConfigureAwait(false);
        if (restKommune is not null)
        {
            return await EnrichRepresentativePointAsync(restKommune, cancellationToken).ConfigureAwait(false);
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

        var graphQlKommune = await TryGraphQlFindByWgs84Async(latitude, longitude, cancellationToken)
            .ConfigureAwait(false);
        if (graphQlKommune is not null)
        {
            return graphQlKommune;
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

        var (easting, northing) = Etrs89Utm32NConverter.FromWgs84(latitude, longitude);
        return await FindByEtrs89InternalAsync(easting, northing, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Finder kommunen for et punkt i ETRS89 UTM zone 32N (EPSG:25832).</summary>
    public async Task<KommuneDto> FindByEtrs89Async(
        double easting,
        double northing,
        CancellationToken cancellationToken = default)
    {
        var graphQlKommune = await TryGraphQlFindByPointAsync(easting, northing, cancellationToken)
            .ConfigureAwait(false);
        if (graphQlKommune is not null)
        {
            return graphQlKommune;
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

        return await FindByEtrs89InternalAsync(easting, northing, cancellationToken).ConfigureAwait(false);
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
        return await TryGraphQlFindByPointAsync(easting, northing, cancellationToken).ConfigureAwait(false);
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

        if (_dagiOptions.EnableDawaFallback)
        {
            dawaKommuner = await _dawaClient.GetAllAsync(cancellationToken).ConfigureAwait(false);
        }

        return KommuneRegionEnricher.EnrichFromGraph(graphKommuner, regioner, dawaKommuner);
    }

    private async Task<IReadOnlyList<KommuneDto>> EnrichExistingAsync(
        IReadOnlyList<KommuneDto> kommuner,
        CancellationToken cancellationToken)
    {
        var regioner = await SafeGetRegionsAsync(cancellationToken).ConfigureAwait(false);
        IReadOnlyList<KommuneDto>? dawaKommuner = null;

        if (_dagiOptions.EnableDawaFallback)
        {
            dawaKommuner = await _dawaClient.GetAllAsync(cancellationToken).ConfigureAwait(false);
        }

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

        var graph = DarJsonSerializer.DeserializeRequired<KommuneGraphDto>(nodes[0]);
        var enriched = await EnrichFromGraphAsync(new[] { graph }, cancellationToken).ConfigureAwait(false);
        var kommune = enriched.FirstOrDefault();
        if (kommune is null)
        {
            return null;
        }

        return await EnrichRepresentativePointAsync(kommune, cancellationToken).ConfigureAwait(false);
    }

    private async Task<KommuneDto> EnrichRepresentativePointAsync(
        KommuneDto kommune,
        CancellationToken cancellationToken)
    {
        if (kommune.RepræsentativPunktLatitude is not null && kommune.RepræsentativPunktLongitude is not null)
        {
            return kommune;
        }

        if (string.IsNullOrWhiteSpace(kommune.IdLokalId))
        {
            return kommune;
        }

        var temporal = GraphQlDataAccessor.CreateTemporalVariables();
        var nodes = await _accessor.FetchDagiNodesAsync(
            GraphQlQueries.GetKommuneById,
            new KommuneByIdVariables(kommune.IdLokalId, temporal.Virkningstid, temporal.Registreringstid),
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
