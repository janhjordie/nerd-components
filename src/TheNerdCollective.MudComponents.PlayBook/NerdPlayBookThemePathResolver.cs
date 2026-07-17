namespace TheNerdCollective.MudComponents.PlayBook;

internal static class NerdPlayBookThemePathResolver
{
    public static string ResolveThemesDirectory(NerdPlayBookOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.ThemesDirectory))
        {
            return options.ThemesDirectory;
        }

        var assemblyLocation = Path.GetDirectoryName(typeof(NerdPlayBookThemePathResolver).Assembly.Location)!;
        return Path.Combine(assemblyLocation, "Themes");
    }
}
