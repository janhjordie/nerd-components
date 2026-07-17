using System.Text;

namespace TheNerdCollective.MudComponents.DesignTokens;

public static class MudBlazorDesignTokenCssGenerator
{
    public static string Generate(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        NerdTokenNameValidator.Validate(options.Prefix);
        NerdTokenNameValidator.Validate(options.CssLayerName);
        if (options.ScopeSelector is not null &&
            (options.ScopeSelector.Contains('{') || options.ScopeSelector.Contains('}')))
        {
            throw new ArgumentException("ScopeSelector cannot contain CSS declarations.", nameof(options));
        }

        var css = new StringBuilder($"/* MudBlazor {options.MudBlazorVersion} design token mapping */\n");
        if (options.UseCssLayer)
        {
            css.AppendLine($"@layer {options.CssLayerName} {{");
        }

        foreach (var pair in options.Colors.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            AppendToken(css, options, pair.Key, pair.Value);
        }
        foreach (var alias in options.Aliases.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            css.AppendLine($".{options.Prefix}-{alias.Key} {{");
            css.AppendLine($"  --{options.Prefix}-color-{alias.Key}: var(--{options.Prefix}-color-{alias.Value});");
            css.AppendLine($"  --{options.Prefix}-color-{alias.Key}-text: var(--{options.Prefix}-color-{alias.Value}-text);");
            css.AppendLine($"  --{options.Prefix}-color-{alias.Key}-hover: var(--{options.Prefix}-color-{alias.Value}-hover);");
            css.AppendLine("}");
        }
        foreach (var radius in options.Radii.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            css.AppendLine($".{options.Prefix}-radius-{radius.Key} {{ border-radius: {radius.Value}; }}");
        }
        foreach (var shadow in options.Shadows.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            css.AppendLine($".{options.Prefix}-shadow-{shadow.Key} {{ box-shadow: {shadow.Value}; }}");
        }
        if (options.UseCssLayer)
        {
            css.AppendLine("}");
        }

        var result = css.ToString();
        return options.MinifyCss
            ? System.Text.RegularExpressions.Regex.Replace(result, @"\s+", " ").Replace(" {", "{").Replace("; }", ";}")
            : result;
    }

    private static void AppendToken(
        StringBuilder css,
        NerdDesignTokenOptions options,
        string name,
        NerdColorToken token)
    {
        var value = NerdColorValue.Validate(token.Value, nameof(token.Value));
        var light = NerdColorValue.Validate(token.Light ?? value, nameof(token.Light));
        var dark = NerdColorValue.Validate(token.Dark ?? light, nameof(token.Dark));
        var contrast = NerdColorValue.Validate(
            token.ContrastText ?? NerdColorValue.ContrastText(light),
            nameof(token.ContrastText));
        var darkContrast = NerdColorValue.Validate(
            token.DarkContrastText ?? token.ContrastText ?? NerdColorValue.ContrastText(dark),
            nameof(token.DarkContrastText));
        var root = string.IsNullOrWhiteSpace(options.ScopeSelector)
            ? $".{options.Prefix}-{name}"
            : $"{options.ScopeSelector} .{options.Prefix}-{name}";
        var prefix = options.Prefix;
        var variable = $"--{prefix}-color-{name}";
        var textVariable = $"{variable}-text";
        var hoverVariable = $"{variable}-hover";
        var activeVariable = $"{variable}-active";
        var borderVariable = $"{variable}-border";
        var disabledVariable = $"{variable}-disabled";

        css.AppendLine($"{root} {{");
        MudBlazorPaletteMapper.AppendPaletteVariables(
            css,
            prefix,
            name,
            variable,
            textVariable,
            hoverVariable,
            activeVariable,
            borderVariable,
            disabledVariable,
            token,
            light,
            dark,
            contrast,
            darkContrast);
        css.AppendLine("}");

        css.AppendLine($"[data-theme=\"dark\"] {root} {{");
        css.AppendLine($"  {variable}: {dark};");
        css.AppendLine($"  {textVariable}: {darkContrast};");
        css.AppendLine("}");

        MudBlazorComponentRuleBuilder.AppendRules(
            css,
            root,
            variable,
            textVariable,
            hoverVariable,
            activeVariable,
            borderVariable,
            disabledVariable,
            options.UseImportantOverrides);
    }
}
