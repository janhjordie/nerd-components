using MudBlazor;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Builds per-intent <see cref="MudTheme"/> instances for PseudoCss scopes (HR-164).
/// </summary>
public static class NerdMudIntentThemeFactory
{
    public static MudTheme CreateIntentTheme(
        NerdDesignTokenOptions options,
        string alias,
        MudTheme brandTheme,
        bool isDarkMode)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(brandTheme);
        var mode = isDarkMode ? NerdMudPaletteMode.Dark : NerdMudPaletteMode.Light;
        var map = NerdMudIntentPaletteMap.ResolveIntentPaletteMap(options, alias, mode);

        return new MudTheme
        {
            PaletteLight = isDarkMode
                ? brandTheme.PaletteLight
                : NerdMudThemePaletteConverter.ToPaletteLight(map),
            PaletteDark = isDarkMode
                ? NerdMudThemePaletteConverter.ToPaletteDark(map)
                : brandTheme.PaletteDark,
            Typography = brandTheme.Typography,
            Shadows = brandTheme.Shadows,
            LayoutProperties = brandTheme.LayoutProperties,
            ZIndex = brandTheme.ZIndex,
            PseudoCss = brandTheme.PseudoCss
        };
    }
}
