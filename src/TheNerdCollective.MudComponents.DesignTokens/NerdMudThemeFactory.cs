using MudBlazor;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Builds a <see cref="MudTheme"/> from the active brand pack so <see cref="MudThemeProvider"/> owns global :root palette (HR-157).
/// </summary>
public static class NerdMudThemeFactory
{
    public static MudTheme Create(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var lightMap = NerdMudBrandPaletteMap.Resolve(options, NerdMudPaletteMode.Light);
        var darkMap = NerdMudBrandPaletteMap.Resolve(options, NerdMudPaletteMode.Dark);

        return new MudTheme
        {
            PaletteLight = NerdMudThemePaletteConverter.ToPaletteLight(lightMap),
            PaletteDark = NerdMudThemePaletteConverter.ToPaletteDark(darkMap)
        };
    }
}
