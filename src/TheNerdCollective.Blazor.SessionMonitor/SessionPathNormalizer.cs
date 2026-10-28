namespace TheNerdCollective.Blazor.SessionMonitor;

internal static class SessionPathNormalizer
{
    internal static string Normalize(string location)
    {
        if (location.Contains("://", StringComparison.Ordinal)
            && Uri.TryCreate(location, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri.PathAndQuery;
        }

        return location.StartsWith('/') ? location : "/" + location;
    }

    internal static string GroupKey(string? path)
    {
        return string.IsNullOrWhiteSpace(path) ? ActivePathSessionSummary.UnknownPathLabel : Normalize(path);
    }

    internal static bool MatchesPathPrefix(string? path, IReadOnlyList<string> prefixes)
    {
        if (string.IsNullOrWhiteSpace(path) || prefixes.Count == 0)
        {
            return false;
        }

        var normalized = GroupKey(path);
        var pathOnly = normalized;
        var queryIndex = normalized.IndexOf('?');
        if (queryIndex >= 0)
        {
            pathOnly = normalized[..queryIndex];
        }

        foreach (var prefix in prefixes)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                continue;
            }

            var normalizedPrefix = prefix.StartsWith('/') ? prefix : "/" + prefix;
            var prefixQueryIndex = normalizedPrefix.IndexOf('?');
            if (prefixQueryIndex >= 0)
            {
                normalizedPrefix = normalizedPrefix[..prefixQueryIndex];
            }

            normalizedPrefix = normalizedPrefix.TrimEnd('/');
            if (normalizedPrefix.Length == 0)
            {
                normalizedPrefix = "/";
            }

            if (pathOnly.Equals(normalizedPrefix, StringComparison.OrdinalIgnoreCase)
                || pathOnly.StartsWith(normalizedPrefix + "/", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
