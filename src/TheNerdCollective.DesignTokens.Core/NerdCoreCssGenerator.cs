using System.Text;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Framework-neutral CSS: custom properties, spacing, foundation tokens, recipes (region only), opacity, intents.
/// MudBlazor adapter adds component-specific rules on top (HR-114).
/// </summary>
public static class NerdCoreCssGenerator
{
    public static string Generate(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        NerdThemeSetTools.SyncColorTokensFromThemeSets(options);
        NerdTokenNameValidator.Validate(options.Prefix);
        NerdTokenNameValidator.Validate(options.CssLayerName);

        var css = new StringBuilder("/* DesignTokens.Core — CSS variables + shell recipes */\n");
        if (options.UseCssLayer)
        {
            css.AppendLine($"@layer {options.CssLayerName} {{");
        }

        var brandRoot = $".{NerdIntentCssManifest.BrandRootClass(options.Prefix)}";
        AppendColorRootVariables(css, options, brandRoot);

        if (options.EmitFrameworkNeutralIntents)
        {
            NerdIntentCssGenerator.AppendBrandIntentVariables(css, options);
        }

        foreach (var radius in options.Radii.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            css.AppendLine($".{options.Prefix}-radius-{radius.Key} {{ border-radius: {radius.Value}; }}");
        }

        foreach (var shadow in options.Shadows.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            css.AppendLine($".{options.Prefix}-shadow-{shadow.Key} {{ box-shadow: {shadow.Value}; }}");
        }

        foreach (var spacing in options.Spacing.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            var variable = $"--{options.Prefix}-space-{spacing.Key}";
            css.AppendLine($".{options.Prefix}-space-{spacing.Key} {{ {variable}: {spacing.Value}; gap: var({variable}); }}");
            css.AppendLine($".{options.Prefix}-pa-{spacing.Key} {{ padding: {spacing.Value}; }}");
            css.AppendLine($".{options.Prefix}-ma-{spacing.Key} {{ margin: {spacing.Value}; }}");
        }

        if (options.Breakpoints.Count > 0 ||
            options.MotionDurations.Count > 0 ||
            options.MotionEasings.Count > 0 ||
            options.ZIndex.Count > 0)
        {
            AppendFoundationRootVariables(css, options, brandRoot);
        }

        foreach (var recipe in options.Recipes.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            AppendRecipeRegion(css, options, recipe.Key, recipe.Value);
        }

        foreach (var opacity in options.Opacities.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            AppendOpacity(css, options, opacity.Key, opacity.Value);
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

    private static void AppendColorRootVariables(StringBuilder css, NerdDesignTokenOptions options, string root)
    {
        var variables = NerdDesignTokenColorVariables.Build(options);
        css.AppendLine($"{root} {{");
        foreach (var pair in variables.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            css.AppendLine($"  {pair.Key}: {pair.Value};");
        }

        foreach (var alias in options.Aliases.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            if (!options.Colors.ContainsKey(alias.Value))
            {
                continue;
            }

            var baseVar = $"--{options.Prefix}-color-{alias.Key}";
            css.AppendLine($"  {baseVar}: var(--{options.Prefix}-color-{alias.Value});");
            css.AppendLine($"  {baseVar}-surface: var(--{options.Prefix}-color-{alias.Value}-surface);");
            css.AppendLine($"  {baseVar}-content: var(--{options.Prefix}-color-{alias.Value}-content);");
            css.AppendLine($"  {baseVar}-interactive: var(--{options.Prefix}-color-{alias.Value}-interactive);");
        }

        css.AppendLine("}");
        css.AppendLine();
    }

    private static void AppendFoundationRootVariables(
        StringBuilder css,
        NerdDesignTokenOptions options,
        string root)
    {
        css.AppendLine($"{root} {{");
        foreach (var pair in options.Breakpoints.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            css.AppendLine($"  --{options.Prefix}-breakpoint-{pair.Key}: {pair.Value};");
        }

        foreach (var pair in options.MotionDurations.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            css.AppendLine($"  --{options.Prefix}-duration-{pair.Key}: {pair.Value};");
        }

        foreach (var pair in options.MotionEasings.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            css.AppendLine($"  --{options.Prefix}-ease-{pair.Key}: {pair.Value};");
        }

        foreach (var pair in options.ZIndex.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            css.AppendLine($"  --{options.Prefix}-z-{pair.Key}: {pair.Value};");
        }

        css.AppendLine("}");
        css.AppendLine();
    }

    private static void AppendRecipeRegion(
        StringBuilder css,
        NerdDesignTokenOptions options,
        string name,
        NerdDesignTokenRecipe recipe)
    {
        var root = $".{options.Prefix}-recipe-{name}";
        var surfaceToken = options.Colors[recipe.Surface];
        var contentToken = options.Colors[recipe.Content];
        var actionToken = recipe.Action is null ? surfaceToken : options.Colors[recipe.Action];
        var surfaceColor = surfaceToken.Surface ?? surfaceToken.Light ?? surfaceToken.Value;
        var contentColor = contentToken.Content
                           ?? NerdColorParser.ContentText(
                               contentToken.Light ?? contentToken.Value,
                               contentToken.ContrastText ?? NerdColorValue.ContrastText(contentToken.Light ?? contentToken.Value));
        var actionColor = actionToken.Light ?? actionToken.Value;
        var important = options.UseImportantOverrides ? " !important" : string.Empty;

        css.AppendLine($"{root} {{");
        css.AppendLine($"  background-color: {surfaceColor}{important};");
        css.AppendLine($"  color: {contentColor}{important};");
        css.AppendLine($"  --{options.Prefix}-recipe-action: {actionColor}{important};");
        css.AppendLine("}");
        css.AppendLine();
    }

    private static void AppendOpacity(
        StringBuilder css,
        NerdDesignTokenOptions options,
        string name,
        NerdOpacityToken opacity)
    {
        if (!options.Colors.TryGetValue(opacity.BaseToken, out var baseToken))
        {
            return;
        }

        var baseColor = baseToken.Light ?? baseToken.Value;
        var className = $".{options.Prefix}-opacity-{name}";
        css.AppendLine($"{className} {{");
        css.AppendLine($"  background-color: color-mix(in srgb, {baseColor} {opacity.Opacity * 100:0.#}%, transparent){(options.UseImportantOverrides ? " !important" : string.Empty)};");
        css.AppendLine("}");
        css.AppendLine();
    }
}
