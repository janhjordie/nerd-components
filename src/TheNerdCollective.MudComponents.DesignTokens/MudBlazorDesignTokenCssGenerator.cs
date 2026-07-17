using System.Text;

namespace TheNerdCollective.MudComponents.DesignTokens;

public static class MudBlazorDesignTokenCssGenerator
{
    public static string Generate(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        NerdTokenNameValidator.Validate(options.Prefix);

        var css = new StringBuilder("@layer nerd-design-tokens {\n");
        foreach (var pair in options.Colors)
        {
            AppendToken(css, options.Prefix, pair.Key, pair.Value);
        }
        foreach (var alias in options.Aliases)
        {
            css.AppendLine($".{options.Prefix}-{alias.Key} {{");
            css.AppendLine($"  --{options.Prefix}-color-{alias.Key}: var(--{options.Prefix}-color-{alias.Value});");
            css.AppendLine($"  --{options.Prefix}-color-{alias.Key}-text: var(--{options.Prefix}-color-{alias.Value}-text);");
            css.AppendLine($"  --{options.Prefix}-color-{alias.Key}-hover: var(--{options.Prefix}-color-{alias.Value}-hover);");
            css.AppendLine("}");
        }
        foreach (var radius in options.Radii)
        {
            css.AppendLine($".{options.Prefix}-radius-{radius.Key} {{ border-radius: {radius.Value}; }}");
        }
        foreach (var shadow in options.Shadows)
        {
            css.AppendLine($".{options.Prefix}-shadow-{shadow.Key} {{ box-shadow: {shadow.Value}; }}");
        }
        css.AppendLine("}");

        return css.ToString();
    }

    private static void AppendToken(
        StringBuilder css,
        string prefix,
        string name,
        NerdColorToken token)
    {
        var value = NerdColorValue.Validate(token.Value, nameof(token.Value));
        var light = NerdColorValue.Validate(token.Light ?? value, nameof(token.Light));
        var dark = NerdColorValue.Validate(token.Dark ?? light, nameof(token.Dark));
        var contrast = NerdColorValue.Validate(
            token.ContrastText ?? NerdColorValue.ContrastText(light),
            nameof(token.ContrastText));
        var root = $".{prefix}-{name}";
        var variable = $"--{prefix}-color-{name}";
        var textVariable = $"{variable}-text";
        var hoverVariable = $"{variable}-hover";
        var activeVariable = $"{variable}-active";
        var borderVariable = $"{variable}-border";
        var disabledVariable = $"{variable}-disabled";

        css.AppendLine($"{root} {{");
        css.AppendLine($"  {variable}: {light};");
        css.AppendLine($"  {textVariable}: {contrast};");
        css.AppendLine($"  {hoverVariable}: {token.Hover ?? light};");
        css.AppendLine($"  {activeVariable}: {token.Active ?? token.Hover ?? light};");
        css.AppendLine($"  {borderVariable}: {token.Border ?? light};");
        css.AppendLine($"  {disabledVariable}: {token.Disabled ?? light};");
        css.AppendLine($"  --{prefix}-color-{name}-surface: {token.Surface ?? light};");
        css.AppendLine($"  --{prefix}-color-{name}-content: {token.Content ?? contrast};");
        css.AppendLine($"  --{prefix}-color-{name}-interactive: {token.Interactive ?? light};");
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
        css.AppendLine($"[data-theme=\"dark\"] {root} {{");
        css.AppendLine($"  {variable}: {dark};");
        css.AppendLine($"  {textVariable}: {NerdColorValue.ContrastText(dark)};");
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

        css.AppendLine($"{root}:focus-visible, {root}.mud-button:focus, {root}.mud-button:active,");
        css.AppendLine($"{root}.mud-chip:active, {root}.mud-selected, {root}.mud-checked,");
        css.AppendLine($"{root}.mud-expanded, {root}[aria-pressed=\"true\"] {{");
        css.AppendLine($"  outline-color: var({activeVariable}) !important;");
        css.AppendLine("}");

        css.AppendLine($"{root}.mud-disabled, {root}[disabled], {root}.mud-button:disabled,");
        css.AppendLine($"{root}.mud-chip.mud-disabled, {root}[aria-disabled=\"true\"] {{");
        css.AppendLine($"  color: var({disabledVariable}) !important;");
        css.AppendLine("}");
        css.AppendLine();
    }
}
