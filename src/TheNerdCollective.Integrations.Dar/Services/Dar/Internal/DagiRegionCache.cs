using System;
using System.Collections.Generic;
using TheNerdCollective.Integrations.Dar.Models;

namespace TheNerdCollective.Integrations.Dar.Services.Dar.Internal;

internal static class DagiRegionCache
{
    private static readonly object Sync = new();
    private static IReadOnlyList<RegionDto>? _cached;
    private static DateTime _expiresAtUtc = DateTime.MinValue;

    internal static bool TryGetAll(TimeSpan cacheDuration, out IReadOnlyList<RegionDto> regions)
    {
        lock (Sync)
        {
            if (_cached is not null && _expiresAtUtc > DateTime.UtcNow)
            {
                regions = _cached;
                return true;
            }
        }

        regions = Array.Empty<RegionDto>();
        return false;
    }

    internal static void SetAll(IReadOnlyList<RegionDto> regions, TimeSpan cacheDuration)
    {
        lock (Sync)
        {
            _cached = regions;
            _expiresAtUtc = DateTime.UtcNow.Add(cacheDuration);
        }
    }
}
