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
}
