using MudBlazor;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Host-facing API for brand palette + Mud theme refresh without regenerating token CSS (HR-172).
/// </summary>
public interface INerdMudThemeController
{
    MudTheme CurrentTheme { get; }

    bool IsDarkMode { get; }

    event Action? ThemeChanged;

    void ApplyBrandPack(string brandPackId);

    void SetDarkMode(bool isDarkMode);

    /// <param name="configure">Optional Mud theme mutation (e.g. PlayBook typography preset). Skips <see cref="INerdMudThemeConfigurator"/> when set.</param>
    void RefreshTheme(Action<MudTheme>? configure = null);
}
