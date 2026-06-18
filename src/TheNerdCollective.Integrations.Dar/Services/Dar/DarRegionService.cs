using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TheNerdCollective.Integrations.Dar.Models;
using TheNerdCollective.Integrations.Dar.Services.Dar.Internal;

namespace TheNerdCollective.Integrations.Dar.Services.Dar;

/// <summary>Regioner via DAGI GraphQL med DAWA/WFS-fallback.</summary>
public sealed class DarRegionService
{
    private readonly DagiRegionDataFetcher _fetcher;

    internal DarRegionService(DagiRegionDataFetcher fetcher)
    {
        _fetcher = fetcher;
    }

    /// <summary>Returnerer alle aktuelle regioner sorteret efter navn.</summary>
    public Task<IReadOnlyList<RegionDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _fetcher.GetAllAsync(cancellationToken);
}
