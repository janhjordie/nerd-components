namespace TheNerdCollective.Blazor.ThemeKit;

public sealed class MudThemeKitOptions
{
    public string DefaultThemeId { get; set; } = string.Empty;

    /// <summary>
    /// When true (Playbook), theme switcher and editor are always available.
    /// </summary>
    public bool PlaybookMode { get; set; }

    public string PreferencesStorageKeyPrefix { get; set; } = "nerd.themeKit";
}
