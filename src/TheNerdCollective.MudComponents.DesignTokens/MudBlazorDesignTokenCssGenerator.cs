using System.Text;

namespace TheNerdCollective.MudComponents.DesignTokens;

public static class MudBlazorDesignTokenCssGenerator
{
    public static string Generate(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        NerdTokenNameValidator.Validate(options.Prefix);

        var css = new StringBuilder();
        foreach (var pair in options.Colors)
        {
            AppendToken(css, options.Prefix, pair.Key, pair.Value);
        }

        return css.ToString();
    }

    private static void AppendToken(
        StringBuilder css,
        string prefix,
        string name,
        NerdColorToken token)
    {
        var root = $".{prefix}-{name}";
        var variable = $"--{prefix}-color-{name}";
        var textVariable = $"{variable}-text";
        var hoverVariable = $"{variable}-hover";
        var activeVariable = $"{variable}-active";
        var borderVariable = $"{variable}-border";
        var disabledVariable = $"{variable}-disabled";

        css.AppendLine($"{root} {{");
        css.AppendLine($"  {variable}: {token.Value};");
        css.AppendLine($"  {textVariable}: {token.ContrastText};");
        css.AppendLine($"  {hoverVariable}: {token.Hover ?? token.Value};");
        css.AppendLine($"  {activeVariable}: {token.Active ?? token.Hover ?? token.Value};");
        css.AppendLine($"  {borderVariable}: {token.Border ?? token.Value};");
        css.AppendLine($"  {disabledVariable}: {token.Disabled ?? token.Value};");
        css.AppendLine("  --mud-palette-primary: var(" + variable + ");");
        css.AppendLine("  --mud-palette-primary-text: var(" + textVariable + ");");
        css.AppendLine("  --mud-palette-primary-hover: var(" + hoverVariable + ");");
        css.AppendLine("  --mud-palette-action-default: var(" + variable + ");");
        css.AppendLine("  --mud-palette-secondary: var(" + variable + ");");
        css.AppendLine("  --mud-palette-tertiary: var(" + variable + ");");
        css.AppendLine("  --mud-palette-info: var(" + variable + ");");
        css.AppendLine("  --mud-palette-success: var(" + variable + ");");
        css.AppendLine("  --mud-palette-warning: var(" + variable + ");");
        css.AppendLine("  --mud-palette-error: var(" + variable + ");");
        css.AppendLine("  --mud-palette-background: var(" + variable + ");");
        css.AppendLine("  --mud-palette-background-gray: var(" + variable + ");");
        css.AppendLine("  --mud-palette-surface: var(" + variable + ");");
        css.AppendLine("  --mud-palette-drawer-background: var(" + variable + ");");
        css.AppendLine("  --mud-palette-appbar-background: var(" + variable + ");");
        css.AppendLine("  --mud-palette-text-primary: var(" + textVariable + ");");
        css.AppendLine("  --mud-palette-text-secondary: var(" + textVariable + ");");
        css.AppendLine("  --mud-palette-lines-default: var(" + borderVariable + ");");
        css.AppendLine("  --mud-palette-lines-inputs: var(" + borderVariable + ");");
        css.AppendLine("  --mud-palette-divider: var(" + borderVariable + ");");
        css.AppendLine("  color: var(" + textVariable + ");");
        css.AppendLine("  background-color: var(" + variable + ");");
        css.AppendLine("}");

        css.AppendLine($"{root}.mud-button-filled, {root}.mud-chip, {root}.mud-alert,");
        css.AppendLine($"{root}.mud-badge, {root}.mud-progress-linear {{");
        css.AppendLine($"  background-color: var({variable}) !important;");
        css.AppendLine($"  color: var({textVariable}) !important;");
        css.AppendLine("}");

        css.AppendLine($"{root}.mud-button-outlined, {root}.mud-button-text,");
        css.AppendLine($"{root}.mud-icon-button, {root}.mud-link, {root}.mud-typography {{");
        css.AppendLine($"  color: var({variable}) !important;");
        css.AppendLine("}");

        css.AppendLine($"{root}.mud-button-outlined {{");
        css.AppendLine($"  border-color: var({borderVariable}) !important;");
        css.AppendLine("}");

        css.AppendLine($"{root}.mud-button-filled:hover, {root}.mud-button-outlined:hover,");
        css.AppendLine($"{root}.mud-button-text:hover, {root}.mud-chip:hover {{");
        css.AppendLine($"  background-color: var({hoverVariable}) !important;");
        css.AppendLine("}");

        css.AppendLine($"{root}:focus-visible, {root}.mud-button:active, {root}.mud-chip:active {{");
        css.AppendLine($"  outline-color: var({activeVariable}) !important;");
        css.AppendLine("}");

        css.AppendLine($"{root}.mud-disabled, {root}[disabled], {root}.mud-button:disabled {{");
        css.AppendLine($"  color: var({disabledVariable}) !important;");
        css.AppendLine("}");
        css.AppendLine();
    }
}
