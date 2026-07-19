using System.Text;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

public static class MudBlazorDesignTokenCssGenerator
{
    public static string Generate(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        NerdTokenNameValidator.Validate(options.Prefix);
        NerdTokenNameValidator.Validate(options.CssLayerName);
        if (string.IsNullOrWhiteSpace(options.PortalScopeSelector) ||
            options.PortalScopeSelector.Contains('{') ||
            options.PortalScopeSelector.Contains('}'))
        {
            throw new ArgumentException("PortalScopeSelector must be a valid CSS selector.", nameof(options));
        }
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
            if (TryResolveAliasToken(options, alias.Value, out var targetToken))
            {
                var surfaceOnly = IsSurfaceAlias(alias.Key);
                AppendToken(css, options, alias.Key, targetToken, emitComponentRules: !surfaceOnly);
                if (surfaceOnly)
                {
                    AppendSurfaceRootStyles(css, options, alias.Key);
                    if (options.EnablePortalTokenScope)
                    {
                        AppendSurfacePortalRules(css, options, alias.Key);
                    }
                }
            }
        }
        foreach (var radius in options.Radii.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            css.AppendLine($".{options.Prefix}-radius-{radius.Key} {{ border-radius: {radius.Value}; }}");
        }
        foreach (var shadow in options.Shadows.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            css.AppendLine($".{options.Prefix}-shadow-{shadow.Key} {{ box-shadow: {shadow.Value}; }}");
        }
        foreach (var recipe in options.Recipes.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            AppendRecipe(css, options, recipe.Key, recipe.Value);
        }
        AppendPairingSurfaceRules(css, options);
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

    private static void AppendRecipe(
        StringBuilder css,
        NerdDesignTokenOptions options,
        string name,
        NerdDesignTokenRecipe recipe)
    {
        var root = $".{options.Prefix}-recipe-{name}";
        var surfaceToken = options.Colors[recipe.Surface];
        var contentToken = options.Colors[recipe.Content];
        var actionToken = recipe.Action is null ? surfaceToken : options.Colors[recipe.Action];
        var borderToken = recipe.Border is null ? surfaceToken : options.Colors[recipe.Border];
        var surfaceBase = surfaceToken.Surface ?? surfaceToken.Light ?? surfaceToken.Value;
        var surfaceColor = surfaceToken.Surface is not null
            ? surfaceBase
            : NerdColorDerivatives.Lighten(surfaceBase, 0.42);
        var contentColor = contentToken.Content
                           ?? NerdColorParser.ContentText(
                               contentToken.Light ?? contentToken.Value,
                               contentToken.ContrastText ?? NerdColorValue.ContrastText(contentToken.Light ?? contentToken.Value));
        var actionColor = actionToken.Light ?? actionToken.Value;
        var actionText = actionToken.ContrastText ?? NerdColorValue.ContrastText(actionColor);
        var borderColor = borderToken.Border ?? borderToken.Light ?? borderToken.Value;
        var important = options.UseImportantOverrides ? " !important" : string.Empty;

        css.AppendLine($"{root}, {root}.mud-paper, {root}.mud-card {{");
        css.AppendLine($"  background-color: {surfaceColor}{important};");
        css.AppendLine($"  color: {contentColor}{important};");
        css.AppendLine($"  border-color: {borderColor}{important};");
        css.AppendLine("}");
        css.AppendLine($"{root} .mud-typography, {root}.mud-paper .mud-typography, {root}.mud-card .mud-typography {{");
        css.AppendLine($"  color: {contentColor}{important};");
        css.AppendLine("}");
        css.AppendLine($"{root} .mud-button, {root} .mud-link, {root} .mud-icon-button {{");
        css.AppendLine($"  color: {actionColor}{important};");
        css.AppendLine("}");
        css.AppendLine($"{root} .mud-button-filled {{");
        css.AppendLine($"  background-color: {actionColor}{important};");
        css.AppendLine($"  color: {actionText}{important};");
        css.AppendLine("}");
        AppendRecipeOutlinedButtonRules(css, root, contentColor, important);
        css.AppendLine($"{root} .mud-card, {root} .mud-paper {{");
        css.AppendLine($"  background-color: {surfaceColor}{important};");
        css.AppendLine($"  color: {contentColor}{important};");
        css.AppendLine("}");

        var darkSurfaceToken = options.Colors[recipe.Surface];
        var darkContentToken = options.Colors[recipe.Content];
        var darkActionToken = recipe.Action is null ? darkSurfaceToken : options.Colors[recipe.Action];
        var darkBorderToken = recipe.Border is null ? darkSurfaceToken : options.Colors[recipe.Border];
        var darkSurfaceBase = darkSurfaceToken.Surface
                              ?? darkSurfaceToken.Dark
                              ?? darkSurfaceToken.Light
                              ?? darkSurfaceToken.Value;
        var darkSurfaceColor = darkSurfaceToken.Surface is not null
            ? darkSurfaceBase
            : NerdColorDerivatives.Lighten(
                darkSurfaceToken.Dark ?? darkSurfaceToken.Light ?? darkSurfaceToken.Value,
                0.42);
        var darkContentColor = darkContentToken.Content
                               ?? NerdColorParser.ContentText(
                                   darkContentToken.Dark ?? darkContentToken.Light ?? darkContentToken.Value,
                                   darkContentToken.DarkContrastText
                                   ?? darkContentToken.ContrastText
                                   ?? NerdColorValue.ContrastText(
                                       darkContentToken.Dark ?? darkContentToken.Light ?? darkContentToken.Value));
        var darkActionColor = darkActionToken.Dark ?? darkActionToken.Light ?? darkActionToken.Value;
        var darkActionText = darkActionToken.DarkContrastText
                             ?? darkActionToken.ContrastText
                             ?? NerdColorValue.ContrastText(darkActionColor);
        var darkBorderColor = darkBorderToken.Border
                              ?? darkBorderToken.Dark
                              ?? darkBorderToken.Light
                              ?? darkBorderToken.Value;

        css.AppendLine($"[data-theme=\"dark\"] {root}, [data-theme=\"dark\"] {root}.mud-paper, [data-theme=\"dark\"] {root}.mud-card {{");
        css.AppendLine($"  background-color: {darkSurfaceColor}{important};");
        css.AppendLine($"  color: {darkContentColor}{important};");
        css.AppendLine($"  border-color: {darkBorderColor}{important};");
        css.AppendLine("}");
        css.AppendLine($"[data-theme=\"dark\"] {root} .mud-typography, [data-theme=\"dark\"] {root}.mud-paper .mud-typography, [data-theme=\"dark\"] {root}.mud-card .mud-typography {{");
        css.AppendLine($"  color: {darkContentColor}{important};");
        css.AppendLine("}");
        css.AppendLine($"[data-theme=\"dark\"] {root} .mud-button, [data-theme=\"dark\"] {root} .mud-link, [data-theme=\"dark\"] {root} .mud-icon-button {{");
        css.AppendLine($"  color: {darkActionColor}{important};");
        css.AppendLine("}");
        css.AppendLine($"[data-theme=\"dark\"] {root} .mud-button-filled {{");
        css.AppendLine($"  background-color: {darkActionColor}{important};");
        css.AppendLine($"  color: {darkActionText}{important};");
        css.AppendLine("}");
        AppendRecipeOutlinedButtonRules(css, $"[data-theme=\"dark\"] {root}", darkContentColor, important);
        css.AppendLine($"[data-theme=\"dark\"] {root} .mud-card, [data-theme=\"dark\"] {root} .mud-paper {{");
        css.AppendLine($"  background-color: {darkSurfaceColor}{important};");
        css.AppendLine($"  color: {darkContentColor}{important};");
        css.AppendLine("}");
    }

    private static void AppendRecipeOutlinedButtonRules(
        StringBuilder css,
        string root,
        string contentColor,
        string important) =>
        AppendPairingChromeRules(css, root, contentColor, important);

    private static void AppendPairingSurfaceRules(StringBuilder css, NerdDesignTokenOptions options)
    {
        var important = options.UseImportantOverrides ? " !important" : string.Empty;
        var surface = $".{NerdPairingSurfaceStyles.ClassName}";

        css.AppendLine($"{surface} {{");
        css.AppendLine($"  background-color: var({NerdPairingSurfaceStyles.SurfaceColorVariable}){important};");
        css.AppendLine($"  color: var({NerdPairingSurfaceStyles.ContentColorVariable}){important};");
        css.AppendLine("}");
        css.AppendLine($".{NerdPairingSurfaceStyles.SwatchClassName} {{");
        css.AppendLine("  border: 1px solid rgba(0, 0, 0, .12);");
        css.AppendLine("  border-radius: 8px;");
        css.AppendLine("  min-height: 120px;");
        css.AppendLine("  padding: 12px;");
        css.AppendLine("}");
        css.AppendLine($".{NerdPairingSurfaceStyles.StudioClassName} {{");
        css.AppendLine("  border-radius: 8px;");
        css.AppendLine("}");

        AppendPairingChromeRules(css, surface, contentColor: null, important);
    }

    private static void AppendPairingChromeRules(
        StringBuilder css,
        string root,
        string? contentColor,
        string important)
    {
        var colorValue = contentColor is null ? "inherit" : contentColor;

        css.AppendLine($"{root} .mud-typography, {root} code {{");
        css.AppendLine($"  color: {colorValue}{important};");
        css.AppendLine("}");
        css.AppendLine($"{root} .mud-button-outlined, {root} .mud-button-outlined .mud-button-label {{");
        css.AppendLine($"  color: {colorValue}{important};");
        css.AppendLine($"  border-color: currentColor{important};");
        css.AppendLine($"  background-color: transparent{important};");
        css.AppendLine("}");
    }

    private static void AppendToken(
        StringBuilder css,
        NerdDesignTokenOptions options,
        string name,
        NerdColorToken token,
        bool emitComponentRules = true)
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
        var contentVariable = $"--{prefix}-color-{name}-content";

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

        if (!emitComponentRules)
        {
            return;
        }

        MudBlazorComponentRuleBuilder.AppendRules(
            css,
            root,
            variable,
            textVariable,
            contentVariable,
            hoverVariable,
            activeVariable,
            borderVariable,
            disabledVariable,
            options.UseImportantOverrides);

        if (options.EnablePortalTokenScope)
        {
            var portalRoot = $"{root}{options.PortalScopeSelector}";
            MudBlazorComponentRuleBuilder.AppendRules(
                css,
                portalRoot,
                variable,
                textVariable,
                contentVariable,
                hoverVariable,
                activeVariable,
                borderVariable,
                disabledVariable,
                options.UseImportantOverrides);
        }
    }

    private static bool TryResolveAliasToken(
        NerdDesignTokenOptions options,
        string target,
        out NerdColorToken token)
    {
        var current = target;
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (options.Aliases.TryGetValue(current, out var next))
        {
            if (!visited.Add(current))
            {
                break;
            }

            current = next;
        }

        return options.Colors.TryGetValue(current, out token!);
    }

    private static bool IsSurfaceAlias(string aliasName) =>
        string.Equals(aliasName, NerdDesignSystemUi.PageSurface, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.BrandChrome, StringComparison.OrdinalIgnoreCase);

    private static void AppendSurfaceRootStyles(StringBuilder css, NerdDesignTokenOptions options, string name)
    {
        var root = $".{options.Prefix}-{name}";
        var prefix = options.Prefix;
        var important = options.UseImportantOverrides ? " !important" : string.Empty;

        css.AppendLine($"{root} {{");
        css.AppendLine($"  background-color: var(--{prefix}-color-{name}-surface){important};");
        css.AppendLine($"  color: var(--{prefix}-color-{name}-content){important};");
        css.AppendLine("}");
    }

    private static void AppendSurfacePortalRules(StringBuilder css, NerdDesignTokenOptions options, string name)
    {
        var root = $".{options.Prefix}-{name}{options.PortalScopeSelector}";
        var prefix = options.Prefix;
        var important = options.UseImportantOverrides ? " !important" : string.Empty;
        var contentVar = $"--{prefix}-color-{name}-content";
        var surfaceVar = $"--{prefix}-color-{name}-surface";
        var hoverVar = $"--{prefix}-color-{name}-hover";

        css.AppendLine($"{root} {{");
        css.AppendLine($"  background-color: var({surfaceVar}){important};");
        css.AppendLine($"  color: var({contentVar}){important};");
        css.AppendLine("}");

        css.AppendLine($"{root} [class*=\"mud-list-item\"],");
        css.AppendLine($"{root} [class*=\"mud-menu-item\"],");
        css.AppendLine($"{root} .mud-typography {{");
        css.AppendLine($"  color: var({contentVar}){important};");
        css.AppendLine($"  background-color: transparent{important};");
        css.AppendLine("}");

        css.AppendLine($"{root} :where([class*=\"mud-select\"]),");
        css.AppendLine($"{root} :where([class*=\"mud-input\"]),");
        css.AppendLine($"{root} :where([class*=\"mud-autocomplete\"]) {{");
        css.AppendLine($"  color: var({contentVar}){important};");
        css.AppendLine($"  caret-color: var({contentVar}){important};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-list-item-clickable:hover,");
        css.AppendLine($"{root} .mud-selected-item,");
        css.AppendLine($"{root} .mud-selected {{");
        css.AppendLine($"  background-color: color-mix(in srgb, var({contentVar}) 8%, var({surfaceVar})){important};");
        css.AppendLine($"  color: var({contentVar}){important};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-list-item-clickable:hover .mud-typography,");
        css.AppendLine($"{root} .mud-selected-item .mud-typography,");
        css.AppendLine($"{root} .mud-selected .mud-typography {{");
        css.AppendLine($"  color: inherit{important};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-input-label.mud-input-label-inputcontrol {{");
        css.AppendLine($"  color: var({contentVar}){important};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-input-outlined .mud-input-outlined-border {{");
        css.AppendLine($"  border-color: color-mix(in srgb, var({contentVar}) 38%, var({surfaceVar})){important};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-input-outlined.mud-input-focused .mud-input-outlined-border {{");
        css.AppendLine($"  border-color: var({contentVar}){important};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-input-adornment {{");
        css.AppendLine($"  color: var({contentVar}){important};");
        css.AppendLine("}");
    }

    private static void AppendOpacity(
        StringBuilder css,
        NerdDesignTokenOptions options,
        string name,
        NerdOpacityToken token)
    {
        if (!options.Colors.ContainsKey(token.BaseToken))
        {
            return;
        }

        var root = $".{options.Prefix}-opacity-{name}";
        var baseVariable = $"--{options.Prefix}-color-{token.BaseToken}";
        var percent = token.Opacity.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        var important = options.UseImportantOverrides ? " !important" : string.Empty;

        css.AppendLine($"{root} {{");
        css.AppendLine($"  background-color: color-mix(in srgb, var({baseVariable}) calc({percent} * 100%), transparent){important};");
        css.AppendLine($"  color: var({baseVariable}-content){important};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-typography {{");
        css.AppendLine($"  color: var({baseVariable}-content){important};");
        css.AppendLine("}}");
    }
}
