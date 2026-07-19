namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>MudBlazor-specific CSS export (full adapter rules on top of Core variables).</summary>
public static class NerdDesignTokenMudTools
{
    public static void WriteMudCss(NerdDesignTokenOptions options, string path)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        File.WriteAllText(path, MudBlazorDesignTokenCssGenerator.Generate(options));
    }

    public static string ExportMudCss(NerdDesignTokenOptions options) =>
        MudBlazorDesignTokenCssGenerator.Generate(options);
}
