namespace TheNerdCollective.Blazor.SessionMonitor;

internal static class SessionPathNormalizer
{
    internal static string Normalize(string location)
    {
        if (Uri.TryCreate(location, UriKind.Absolute, out var absoluteUri))
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
        foreach (var prefix in prefixes)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                continue;
            }

            var normalizedPrefix = prefix.StartsWith('/') ? prefix : "/" + prefix;
            if (normalized.StartsWith(normalizedPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
