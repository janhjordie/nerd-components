using MudBlazor;
using MudBlazor.Utilities;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Converts <see cref="NerdMudBrandPaletteMap"/> to MudBlazor <see cref="Palette"/> using <see cref="MudColor"/> derivations (HR-157).
/// </summary>
internal static class NerdMudThemePaletteConverter
{
    private const double HoverOpacity = 0.08;

    public static PaletteLight ToPaletteLight(NerdMudBrandPaletteMap map) => ToPalette<PaletteLight>(map);

    public static PaletteDark ToPaletteDark(NerdMudBrandPaletteMap map) => ToPalette<PaletteDark>(map);

    private static TPalette ToPalette<TPalette>(NerdMudBrandPaletteMap map)
        where TPalette : Palette, new()
    {
        var primary = ToMudColor(map.Primary);
        var secondary = ToMudColor(map.Secondary);
        var tertiary = ToMudColor(map.Tertiary);
        var info = ToMudColor(map.Info);
        var success = ToMudColor(map.Success);
        var warning = ToMudColor(map.Warning);
        var error = ToMudColor(map.Error);
        var dark = ToMudColor(map.Dark);
        var surface = ToMudColor(map.Surface);
        var actionDefault = ToMudColor(map.ActionDefault, surface);

        return new TPalette
        {
            Black = "#000000",
            White = "#FFFFFF",
            Primary = primary.Value,
            PrimaryContrastText = ToMudColor(map.PrimaryText, primary).Value,
            Secondary = secondary.Value,
            SecondaryContrastText = ToMudColor(map.SecondaryText, secondary).Value,
            Tertiary = tertiary.Value,
            TertiaryContrastText = ToMudColor(map.TertiaryText, tertiary).Value,
            Info = info.Value,
            InfoContrastText = ToMudColor(map.InfoText, info).Value,
            Success = success.Value,
            SuccessContrastText = ToMudColor(map.SuccessText, success).Value,
            Warning = warning.Value,
            WarningContrastText = ToMudColor(map.WarningText, warning).Value,
            Error = error.Value,
            ErrorContrastText = ToMudColor(map.ErrorText, error).Value,
            Dark = dark.Value,
            DarkContrastText = ToMudColor(map.DarkText, dark).Value,
            TextPrimary = ToMudColor(map.TextPrimary, surface).Value,
            TextSecondary = ToMudColor(map.TextSecondary, surface).Value,
            TextDisabled = ToMudColor(map.TextDisabled, surface).Value,
            ActionDefault = actionDefault.Value,
            ActionDisabled = ToMudColor(map.ActionDisabled, surface).Value,
            ActionDisabledBackground = ToMudColor(map.ActionDisabledBackground, surface).Value,
            Surface = surface.Value,
            Background = ToMudColor(map.Background, surface).Value,
            BackgroundGray = ToMudColor(map.BackgroundGray, surface).Value,
            DrawerBackground = ToMudColor(map.DrawerBackground, surface).Value,
            DrawerText = ToMudColor(map.DrawerText, surface).Value,
            DrawerIcon = ToMudColor(map.DrawerIcon, surface).Value,
            AppbarBackground = ToMudColor(map.AppbarBackground, primary).Value,
            AppbarText = ToMudColor(map.AppbarText, primary).Value,
            LinesDefault = ToMudColor(map.LinesDefault, surface).Value,
            LinesInputs = ToMudColor(map.LinesInputs, surface).Value,
            Divider = ToMudColor(map.Divider, surface).Value,
            DividerLight = ToMudColor(map.DividerLight, surface).Value,
            TableLines = ToMudColor(map.TableLines, surface).Value,
            TableStriped = ToMudColor(map.TableStriped, surface).Value,
            TableHover = ToMudColor(map.TableHover, surface).Value,
            Skeleton = ToMudColor(map.Skeleton, surface).Value,
            GrayDefault = ToMudColor(map.GrayDefault, surface).Value,
            GrayLight = ToMudColor(map.GrayLight, surface).Value,
            GrayLighter = ToMudColor(map.GrayLighter, surface).Value,
            GrayDark = ToMudColor(map.GrayDark, surface).Value,
            GrayDarker = ToMudColor(map.GrayDarker, surface).Value,
            OverlayDark = ToMudColor(map.OverlayDark, dark).Value,
            OverlayLight = ToMudColor(map.OverlayLight, surface).Value,
            HoverOpacity = HoverOpacity
        };
    }

    private static MudColor ToMudColor(string value, MudColor? fallback = null)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Contains("color-mix", StringComparison.OrdinalIgnoreCase))
        {
            return fallback ?? new MudColor("#000000");
        }

        try
        {
            return new MudColor(value);
        }
        catch (Exception)
        {
            return fallback ?? new MudColor("#000000");
        }
    }
}
