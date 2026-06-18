namespace TheNerdCollective.Blazor.ThemeKit;

public static class ThemeVersionHelper
{
    public static string BumpPatch(string version)
    {
        if (!Version.TryParse(NormalizeVersion(version), out var parsed))
        {
            return version;
        }

        return new Version(parsed.Major, parsed.Minor, parsed.Build + 1).ToString(3);
    }

    public static string TodayIsoDate()
        => DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd");

    private static string NormalizeVersion(string version)
    {
        var parts = version.Split('.');
        return parts.Length switch
        {
            1 => $"{parts[0]}.0.0",
            2 => $"{parts[0]}.{parts[1]}.0",
            _ => version,
        };
    }
}
