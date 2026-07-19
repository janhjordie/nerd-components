using System.Globalization;
using System.Text;
using MudBlazor;
using MudBlazor.Utilities;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Emits Mud palette CSS variables from a <see cref="Palette"/> (subset of MudThemeProvider.GenerateTheme).
/// </summary>
internal static class NerdMudThemeCssEmitter
{
    private const string PalettePrefix = "mud-palette";

    public static void AppendPalette(StringBuilder css, Palette palette)
    {
        ArgumentNullException.ThrowIfNull(palette);
        AppendChannel(css, "primary", palette.Primary, palette.PrimaryContrastText, palette.PrimaryDarken, palette.PrimaryLighten, palette);
        AppendChannel(css, "secondary", palette.Secondary, palette.SecondaryContrastText, palette.SecondaryDarken, palette.SecondaryLighten, palette);
        AppendChannel(css, "tertiary", palette.Tertiary, palette.TertiaryContrastText, palette.TertiaryDarken, palette.TertiaryLighten, palette);
        AppendChannel(css, "info", palette.Info, palette.InfoContrastText, palette.InfoDarken, palette.InfoLighten, palette);
        AppendChannel(css, "success", palette.Success, palette.SuccessContrastText, palette.SuccessDarken, palette.SuccessLighten, palette);
        AppendChannel(css, "warning", palette.Warning, palette.WarningContrastText, palette.WarningDarken, palette.WarningLighten, palette);
        AppendChannel(css, "error", palette.Error, palette.ErrorContrastText, palette.ErrorDarken, palette.ErrorLighten, palette);
        AppendChannel(css, "dark", palette.Dark, palette.DarkContrastText, palette.DarkDarken, palette.DarkLighten, palette);

        css.AppendLine($"--{PalettePrefix}-black: {palette.Black};");
        css.AppendLine($"--{PalettePrefix}-white: {palette.White};");
        css.AppendLine($"--{PalettePrefix}-text-primary: {palette.TextPrimary};");
        css.AppendLine(
            $"--{PalettePrefix}-text-primary-rgb: {palette.TextPrimary.ToString(MudColorOutputFormats.ColorElements)};");
        css.AppendLine($"--{PalettePrefix}-text-secondary: {palette.TextSecondary};");
        css.AppendLine(
            $"--{PalettePrefix}-text-secondary-rgb: {palette.TextSecondary.ToString(MudColorOutputFormats.ColorElements)};");
        css.AppendLine($"--{PalettePrefix}-text-disabled: {palette.TextDisabled};");
        css.AppendLine(
            $"--{PalettePrefix}-text-disabled-rgb: {palette.TextDisabled.ToString(MudColorOutputFormats.ColorElements)};");
        css.AppendLine($"--{PalettePrefix}-action-default: {palette.ActionDefault};");
        css.AppendLine(
            $"--{PalettePrefix}-action-default-hover: {palette.ActionDefault.SetAlpha(palette.HoverOpacity).ToString(MudColorOutputFormats.RGBA)};");
        css.AppendLine($"--{PalettePrefix}-action-disabled: {palette.ActionDisabled};");
        css.AppendLine($"--{PalettePrefix}-action-disabled-background: {palette.ActionDisabledBackground};");
        css.AppendLine($"--{PalettePrefix}-surface: {palette.Surface};");
        css.AppendLine($"--{PalettePrefix}-surface-rgb: {palette.Surface.ToString(MudColorOutputFormats.ColorElements)};");
        css.AppendLine($"--{PalettePrefix}-background: {palette.Background};");
        css.AppendLine($"--{PalettePrefix}-background-gray: {palette.BackgroundGray};");
        css.AppendLine($"--{PalettePrefix}-drawer-background: {palette.DrawerBackground};");
        css.AppendLine($"--{PalettePrefix}-drawer-text: {palette.DrawerText};");
        css.AppendLine($"--{PalettePrefix}-drawer-icon: {palette.DrawerIcon};");
        css.AppendLine($"--{PalettePrefix}-appbar-background: {palette.AppbarBackground};");
        css.AppendLine($"--{PalettePrefix}-appbar-text: {palette.AppbarText};");
        css.AppendLine($"--{PalettePrefix}-lines-default: {palette.LinesDefault};");
        css.AppendLine($"--{PalettePrefix}-lines-inputs: {palette.LinesInputs};");
        css.AppendLine($"--{PalettePrefix}-table-lines: {palette.TableLines};");
        css.AppendLine($"--{PalettePrefix}-table-striped: {palette.TableStriped};");
        css.AppendLine($"--{PalettePrefix}-table-hover: {palette.TableHover};");
        css.AppendLine($"--{PalettePrefix}-divider: {palette.Divider};");
        css.AppendLine($"--{PalettePrefix}-divider-rgb: {palette.Divider.ToString(MudColorOutputFormats.ColorElements)};");
        css.AppendLine($"--{PalettePrefix}-divider-light: {palette.DividerLight};");
        css.AppendLine($"--{PalettePrefix}-skeleton: {palette.Skeleton};");
        css.AppendLine($"--{PalettePrefix}-gray-default: {palette.GrayDefault};");
        css.AppendLine($"--{PalettePrefix}-gray-light: {palette.GrayLight};");
        css.AppendLine($"--{PalettePrefix}-gray-lighter: {palette.GrayLighter};");
        css.AppendLine($"--{PalettePrefix}-gray-dark: {palette.GrayDark};");
        css.AppendLine($"--{PalettePrefix}-gray-darker: {palette.GrayDarker};");
        css.AppendLine($"--{PalettePrefix}-overlay-dark: {palette.OverlayDark};");
        css.AppendLine($"--{PalettePrefix}-overlay-light: {palette.OverlayLight};");
        css.AppendLine(
            $"--{PalettePrefix}-border-opacity: {palette.BorderOpacity.ToString(CultureInfo.InvariantCulture)};");
    }

    private static void AppendChannel(
        StringBuilder css,
        string channel,
        MudColor color,
        MudColor text,
        MudColor darken,
        MudColor lighten,
        Palette palette)
    {
        css.AppendLine($"--{PalettePrefix}-{channel}: {color};");
        css.AppendLine($"--{PalettePrefix}-{channel}-rgb: {color.ToString(MudColorOutputFormats.ColorElements)};");
        css.AppendLine($"--{PalettePrefix}-{channel}-text: {text};");
        css.AppendLine($"--{PalettePrefix}-{channel}-darken: {darken};");
        css.AppendLine($"--{PalettePrefix}-{channel}-lighten: {lighten};");
        css.AppendLine(
            $"--{PalettePrefix}-{channel}-hover: {color.SetAlpha(palette.HoverOpacity).ToString(MudColorOutputFormats.RGBA)};");
    }
}
