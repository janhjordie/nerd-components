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
using TheNerdCollective.Integrations.Dar.Models;
using TheNerdCollective.Integrations.Dar.Services.Internal;

namespace TheNerdCollective.Integrations.Dar.Services.Dar.Internal;

/// <summary>Henter region-liste via DAGI GraphQL med DAWA/WFS-fallback.</summary>
internal sealed class DagiRegionDataFetcher
{
    private const int MinimumExpectedRegionCount = 5;

    private readonly GraphQlDataAccessor _accessor;
    private readonly DawaRegionClient _dawaClient;
    private readonly DagiWfsRegionClient _wfsClient;
    private readonly DarDagiOptions _dagiOptions;

    public DagiRegionDataFetcher(
        GraphQlDataAccessor accessor,
        HttpClient httpClient,
        string apiKey,
        DarDagiOptions dagiOptions)
    {
        _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        _dagiOptions = dagiOptions ?? throw new ArgumentNullException(nameof(dagiOptions));
        _dawaClient = new DawaRegionClient(httpClient, dagiOptions);
        _wfsClient = new DagiWfsRegionClient(httpClient, apiKey, dagiOptions);
    }

    internal async Task<IReadOnlyList<RegionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (DagiRegionCache.TryGetAll(_dagiOptions.RegionListCacheDuration, out var cached))
        {
            return cached;
        }

        var graphQlRegioner = await TryGraphQlGetAllAsync(cancellationToken).ConfigureAwait(false);
        if (graphQlRegioner.Count >= MinimumExpectedRegionCount)
        {
            return CacheAndReturn(graphQlRegioner);
        }

        if (_dagiOptions.EnableDawaFallback)
        {
            var dawaRegioner = await _dawaClient.GetAllAsync(cancellationToken).ConfigureAwait(false);
            if (dawaRegioner.Count > 0)
            {
                return CacheAndReturn(dawaRegioner);
            }
        }

        var wfsRegioner = await _wfsClient.GetAllAsync(cancellationToken).ConfigureAwait(false);
        if (wfsRegioner.Count > 0)
        {
            return CacheAndReturn(wfsRegioner);
        }

        throw new InvalidOperationException(DagiAccessHelp.EmptyRegionResultMessage);
    }

    private IReadOnlyList<RegionDto> CacheAndReturn(IReadOnlyList<RegionDto> regioner)
    {
        DagiRegionCache.SetAll(regioner, _dagiOptions.RegionListCacheDuration);
        return regioner;
    }

    private async Task<IReadOnlyList<RegionDto>> TryGraphQlGetAllAsync(CancellationToken cancellationToken)
    {
        var temporal = GraphQlDataAccessor.CreateTemporalVariables();
        var nodes = await _accessor.FetchAllDagiNodesAsync(
            GraphQlQueries.GetAllRegioner,
            after => new RegionListVariables(temporal.Virkningstid, temporal.Registreringstid, after),
            "DAGI_Regionsinddeling",
            cancellationToken).ConfigureAwait(false);

        return DarJsonSerializer.DeserializeList<RegionDto>(nodes)
            .OrderBy(r => r.Regionnavn, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private sealed class RegionListVariables
    {
        public RegionListVariables(string virkningstid, string registreringstid, string? after)
        {
            Virkningstid = virkningstid;
            Registreringstid = registreringstid;
            After = after;
        }

        public string Virkningstid { get; }

        public string Registreringstid { get; }

        public string? After { get; }
    }
}
