namespace TheNerdCollective.MudComponents.PlayBook;

/// <summary>
/// Configuration for the MudBlazor PlayBook playground.
/// </summary>
public sealed class NerdPlayBookOptions
{
    /// <summary>Enables the PlayBook page.</summary>
    public bool EnablePlayBookPage { get; set; } = true;

    /// <summary>Route for the PlayBook page.</summary>
    public string PlayBookRoute { get; set; } = "/nerd-playbook";

    /// <summary>Restricts the PlayBook to development environments.</summary>
    public bool RestrictPlayBookToDevelopment { get; set; } = true;

    /// <summary>Base URL for MudBlazor component documentation links.</summary>
    public string MudBlazorDocsBaseUrl { get; set; } = "https://mudblazor.com/components";

    /// <summary>Enables ThemeKit theme switcher and JSON theme editor in the PlayBook.</summary>
    public bool EnableThemeKit { get; set; } = true;

    /// <summary>Always enables ThemeKit switcher and editor (Playbook mode).</summary>
    public bool ThemeKitPlaybookMode { get; set; } = true;

    /// <summary>Optional absolute path to the themes directory. Defaults to packaged Themes folder.</summary>
    public string? ThemesDirectory { get; set; }

    /// <summary>Default ThemeKit theme id when JSON catalog is used.</summary>
    public string DefaultThemeId { get; set; } = NerdPlayBookDefaultThemeCatalog.DefaultThemeIdValue;

    /// <summary>Enables per-component property playgrounds.</summary>
    public bool EnableComponentPlaygrounds { get; set; } = true;
}
