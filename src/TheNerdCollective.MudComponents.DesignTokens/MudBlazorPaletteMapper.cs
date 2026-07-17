using System.Text;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

internal static class MudBlazorPaletteMapper
{
    public static void AppendPaletteVariables(
        StringBuilder css,
        string prefix,
        string name,
        string variable,
        string textVariable,
        string hoverVariable,
        string activeVariable,
        string borderVariable,
        string disabledVariable,
        NerdColorToken token,
        string light,
        string dark,
        string lightContrast,
        string darkContrast)
    {
        var lightRgb = NerdColorDerivatives.ToRgbString(light);
        var darkRgb = NerdColorDerivatives.ToRgbString(dark);
        var surface = token.Surface ?? Lighten(light, 0.42);
        var content = token.Content ?? lightContrast;
        var interactive = token.Interactive ?? token.Hover ?? light;

        AppendChannel(css, "primary", variable, textVariable, hoverVariable, light);
        AppendChannel(css, "secondary", variable, textVariable, hoverVariable, light);
        AppendChannel(css, "tertiary", variable, textVariable, hoverVariable, light);
        AppendChannel(css, "info", variable, textVariable, hoverVariable, light);
        AppendChannel(css, "success", variable, textVariable, hoverVariable, light);
        AppendChannel(css, "warning", variable, textVariable, hoverVariable, light);
        AppendChannel(css, "error", variable, textVariable, hoverVariable, light);
        AppendChannel(css, "dark", variable, textVariable, hoverVariable, dark, darkRgb);

        css.AppendLine($"  {variable}: {light};");
        css.AppendLine($"  {textVariable}: {lightContrast};");
        css.AppendLine($"  {hoverVariable}: {token.Hover ?? interactive};");
        css.AppendLine($"  {activeVariable}: {token.Active ?? token.Hover ?? interactive};");
        css.AppendLine($"  {borderVariable}: {token.Border ?? light};");
        css.AppendLine($"  {disabledVariable}: {token.Disabled ?? Lighten(light, 0.25)};");
        css.AppendLine($"  --{prefix}-color-{name}-surface: {surface};");
        css.AppendLine($"  --{prefix}-color-{name}-content: {content};");
        css.AppendLine($"  --{prefix}-color-{name}-interactive: {interactive};");
        css.AppendLine($"  --mud-palette-action-default: var({variable});");
        css.AppendLine($"  --mud-palette-action-default-hover: var({hoverVariable});");
        css.AppendLine($"  --mud-palette-action-disabled: var({disabledVariable});");
        css.AppendLine($"  --mud-palette-action-disabled-background: {Lighten(light, 0.35)};");
        css.AppendLine($"  --mud-palette-background: {surface};");
        css.AppendLine($"  --mud-palette-background-gray: {Lighten(light, 0.3)};");
        css.AppendLine($"  --mud-palette-surface: {surface};");
        css.AppendLine($"  --mud-palette-drawer-background: {surface};");
        css.AppendLine($"  --mud-palette-drawer-text: var({textVariable});");
        css.AppendLine($"  --mud-palette-drawer-icon: var({textVariable});");
        css.AppendLine($"  --mud-palette-appbar-background: var({variable});");
        css.AppendLine($"  --mud-palette-appbar-text: var({textVariable});");
        css.AppendLine($"  --mud-palette-text-primary: {content};");
        css.AppendLine($"  --mud-palette-text-primary-rgb: {NerdColorDerivatives.ToRgbString(content)};");
        css.AppendLine($"  --mud-palette-text-secondary: {Darken(content, 0.15)};");
        css.AppendLine($"  --mud-palette-text-disabled: var({disabledVariable});");
        css.AppendLine($"  --mud-palette-lines-default: var({borderVariable});");
        css.AppendLine($"  --mud-palette-lines-inputs: var({borderVariable});");
        css.AppendLine($"  --mud-palette-divider: var({borderVariable});");
        css.AppendLine($"  --mud-palette-divider-light: {Lighten(light, 0.35)};");
        css.AppendLine($"  --mud-palette-divider-rgb: {lightRgb};");
        css.AppendLine($"  --mud-palette-table-hover: {Lighten(light, 0.38)};");
        css.AppendLine($"  --mud-palette-table-lines: var({borderVariable});");
        css.AppendLine($"  --mud-palette-table-striped: {Lighten(light, 0.42)};");
        css.AppendLine($"  --mud-palette-skeleton: {Lighten(light, 0.35)};");
        css.AppendLine($"  --mud-palette-overlay-light: rgba({lightRgb}, 0.3);");
        css.AppendLine($"  --mud-palette-overlay-dark: rgba({darkRgb}, 0.3);");
        css.AppendLine($"  --mud-palette-gray-default: {Lighten(light, 0.25)};");
        css.AppendLine($"  --mud-palette-gray-light: {Lighten(light, 0.35)};");
        css.AppendLine($"  --mud-palette-gray-darker: {Darken(light, 0.2)};");
        css.AppendLine($"  --mud-palette-white: #FFFFFF;");
        css.AppendLine($"  --mud-palette-border-opacity: 1;");
        css.AppendLine("  color: var(" + textVariable + ");");
        css.AppendLine("  background-color: var(" + variable + ");");
    }

    private static void AppendChannel(
        StringBuilder css,
        string channel,
        string colorVariable,
        string textVariable,
        string hoverVariable,
        string lightColorForDerivatives,
        string? rgbOverride = null)
    {
        var rgb = rgbOverride ?? NerdColorDerivatives.ToRgbString(lightColorForDerivatives);
        css.AppendLine($"  --mud-palette-{channel}: var({colorVariable});");
        css.AppendLine($"  --mud-palette-{channel}-text: var({textVariable});");
        css.AppendLine($"  --mud-palette-{channel}-hover: var({hoverVariable});");
        css.AppendLine($"  --mud-palette-{channel}-darken: {Darken(lightColorForDerivatives)};");
        css.AppendLine($"  --mud-palette-{channel}-lighten: {Lighten(lightColorForDerivatives)};");
        css.AppendLine($"  --mud-palette-{channel}-rgb: {rgb};");
    }

    private static string Lighten(string color, double amount = 0.12) =>
        NerdColorDerivatives.Lighten(color, amount);

    private static string Darken(string color, double amount = 0.12) =>
        NerdColorDerivatives.Darken(color, amount);
}
