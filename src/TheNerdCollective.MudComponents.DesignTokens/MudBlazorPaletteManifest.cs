namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Complete MudBlazor 9.7.0 palette CSS custom properties from <c>MudThemeProvider.GenerateTheme</c>.
/// Used to validate brand-root emission coverage.
/// </summary>
public static class MudBlazorPaletteManifest
{
    public const string MudBlazorVersion = "9.7.0";

    public static readonly string[] ChannelColors =
    [
        "primary", "secondary", "tertiary", "info", "success", "warning", "error", "dark"
    ];

    public static readonly string[] ChannelSuffixes =
    [
        "", "-rgb", "-text", "-darken", "-lighten", "-hover"
    ];

    public static readonly string[] SemanticKeys =
    [
        "black",
        "white",
        "text-primary",
        "text-primary-rgb",
        "text-secondary",
        "text-secondary-rgb",
        "text-disabled",
        "text-disabled-rgb",
        "action-default",
        "action-default-hover",
        "action-disabled",
        "action-disabled-background",
        "surface",
        "surface-rgb",
        "background",
        "background-gray",
        "drawer-background",
        "drawer-text",
        "drawer-icon",
        "appbar-background",
        "appbar-text",
        "lines-default",
        "lines-inputs",
        "table-lines",
        "table-striped",
        "table-hover",
        "divider",
        "divider-rgb",
        "divider-light",
        "skeleton",
        "gray-default",
        "gray-light",
        "gray-lighter",
        "gray-dark",
        "gray-darker",
        "overlay-dark",
        "overlay-light",
        "border-opacity"
    ];

    public static IEnumerable<string> AllPaletteVariables
    {
        get
        {
            foreach (var key in SemanticKeys)
            {
                yield return $"--mud-palette-{key}";
            }

            foreach (var channel in ChannelColors)
            {
                foreach (var suffix in ChannelSuffixes)
                {
                    yield return $"--mud-palette-{channel}{suffix}";
                }
            }
        }
    }

    public static string PaletteVariable(string key) => $"--mud-palette-{key}";

    public static string BrandRootClass(string prefix) => $"{prefix}-mud-brand";
}
