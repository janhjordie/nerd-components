using System;
using System.Collections.Generic;
using TheNerdCollective.Integrations.Dar.Models;

namespace TheNerdCollective.Integrations.Dar.Services.Dar.Internal;

internal static class PostnummerCache
{
    private static readonly object Sync = new();
    private static IReadOnlyList<PostnummerDto>? _activePostnumre;
    private static DateTimeOffset _activeExpiresAt = DateTimeOffset.MinValue;
    private static readonly Dictionary<string, (IReadOnlyList<PostnummerMedKommunerDto> Results, DateTimeOffset ExpiresAt)> CircleCache =
        new(StringComparer.Ordinal);
    private static readonly Dictionary<string, (PostnummerMedKommuneDto Result, DateTimeOffset ExpiresAt)> KommuneByPostnrCache =
        new(StringComparer.Ordinal);

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

    internal static bool TryGetCircle(string cacheKey, TimeSpan cacheDuration, out IReadOnlyList<PostnummerMedKommunerDto> results)
    {
        lock (Sync)
        {
            if (CircleCache.TryGetValue(cacheKey, out var entry) && DateTimeOffset.UtcNow < entry.ExpiresAt)
            {
                results = entry.Results;
                return true;
            }
        }

        results = Array.Empty<PostnummerMedKommunerDto>();
        return false;
    }

    internal static void SetCircle(string cacheKey, IReadOnlyList<PostnummerMedKommunerDto> results, TimeSpan cacheDuration)
    {
        lock (Sync)
        {
            CircleCache[cacheKey] = (results, DateTimeOffset.UtcNow.Add(cacheDuration));
        }
    }

    internal static bool TryGetKommuneByPostnr(string postnummer, TimeSpan cacheDuration, out PostnummerMedKommuneDto result)
    {
        lock (Sync)
        {
            if (KommuneByPostnrCache.TryGetValue(postnummer, out var entry) && DateTimeOffset.UtcNow < entry.ExpiresAt)
            {
                result = entry.Result;
                return true;
            }
        }

        result = default!;
        return false;
    }

    internal static void SetKommuneByPostnr(string postnummer, PostnummerMedKommuneDto result, TimeSpan cacheDuration)
    {
        lock (Sync)
        {
            KommuneByPostnrCache[postnummer] = (result, DateTimeOffset.UtcNow.Add(cacheDuration));
        }
    }
}
