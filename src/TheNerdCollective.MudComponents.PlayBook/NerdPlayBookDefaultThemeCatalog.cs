using MudBlazor;
using TheNerdCollective.Blazor.ThemeKit;

namespace TheNerdCollective.MudComponents.PlayBook;

/// <summary>
/// Default MudTheme catalog used when JSON theme files are unavailable.
/// </summary>
public sealed class NerdPlayBookDefaultThemeCatalog : IMudThemeCatalog
{
    public const string DefaultThemeIdValue = "nerd-default";
    public const string BrandThemeIdValue = "nerd-brand";

    public string DefaultThemeId => DefaultThemeIdValue;

    public IReadOnlyList<MudThemeDescriptor> All { get; } =
    [
        new(DefaultThemeIdValue, "Nerd default", "1.0.0", "#113A5D", "2026-07-17", ThemeCatalogFileOperations.SharedUiSource),
        new(BrandThemeIdValue, "Nerd brand", "1.0.0", "#365C3A", "2026-07-17", ThemeCatalogFileOperations.SharedUiSource)
    ];

    public MudTheme GetTheme(string themeId)
    {
        var theme = new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = themeId.Equals(BrandThemeIdValue, StringComparison.OrdinalIgnoreCase) ? "#365C3A" : "#113A5D",
                Secondary = "#ff7a8a",
                Tertiary = "#E8D8AD",
                AppbarBackground = themeId.Equals(BrandThemeIdValue, StringComparison.OrdinalIgnoreCase) ? "#365C3A" : "#113A5D",
                AppbarText = "#FFFFFF",
                Background = "#fafafa",
                Surface = "#ffffff",
                TextPrimary = "#1F2937",
                TextSecondary = "#4B5563"
            },
            PaletteDark = new PaletteDark
            {
                Primary = themeId.Equals(BrandThemeIdValue, StringComparison.OrdinalIgnoreCase) ? "#4D7A50" : "#90CAF9",
                Secondary = "#ff7a8a",
                Background = "#121212",
                Surface = "#1E1E1E",
                AppbarBackground = "#1E1E1E",
                AppbarText = "#FFFFFF"
            }
        };

        theme.Typography.Default.FontFamily = ["Roboto", "Helvetica", "Arial", "sans-serif"];
        return MudThemeCloner.Clone(theme);
    }
}
