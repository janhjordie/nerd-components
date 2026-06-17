using System;
using System.Collections.Generic;
using TheNerdCollective.Integrations.Dar.Models;

namespace TheNerdCollective.Integrations.Dar.Services.Dar.Internal;

internal static class PostnummerCache
{
    private static readonly object Sync = new();
    private static IReadOnlyList<PostnummerDto>? _activePostnumre;
    private static DateTimeOffset _activeExpiresAt = DateTimeOffset.MinValue;

    internal static bool TryGetActive(TimeSpan cacheDuration, out IReadOnlyList<PostnummerDto> postnumre)
    {
        lock (Sync)
        {
            if (_activePostnumre is not null && DateTimeOffset.UtcNow < _activeExpiresAt)
            {
                postnumre = _activePostnumre;
                return true;
            }
        }

        postnumre = Array.Empty<PostnummerDto>();
        return false;
    }

    internal static void SetActive(IReadOnlyList<PostnummerDto> postnumre, TimeSpan cacheDuration)
    {
        lock (Sync)
        {
            _activePostnumre = postnumre;
            _activeExpiresAt = DateTimeOffset.UtcNow.Add(cacheDuration);
        }
    }
}
