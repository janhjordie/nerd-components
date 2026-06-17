using System;
using System.Collections.Generic;
using TheNerdCollective.Integrations.Dar.Models;

namespace TheNerdCollective.Integrations.Dar.Services.Dar.Internal;

internal static class DagiKommuneCache
{
    private static readonly object Sync = new();
    private static IReadOnlyList<KommuneDto>? _allKommuner;
    private static DateTimeOffset _expiresAt = DateTimeOffset.MinValue;

    internal static bool TryGetAll(TimeSpan cacheDuration, out IReadOnlyList<KommuneDto> kommuner)
    {
        lock (Sync)
        {
            if (_allKommuner is not null && DateTimeOffset.UtcNow < _expiresAt)
            {
                kommuner = _allKommuner;
                return true;
            }
        }

        kommuner = Array.Empty<KommuneDto>();
        return false;
    }

    internal static void SetAll(IReadOnlyList<KommuneDto> kommuner, TimeSpan cacheDuration)
    {
        lock (Sync)
        {
            _allKommuner = kommuner;
            _expiresAt = DateTimeOffset.UtcNow.Add(cacheDuration);
        }
    }
}
