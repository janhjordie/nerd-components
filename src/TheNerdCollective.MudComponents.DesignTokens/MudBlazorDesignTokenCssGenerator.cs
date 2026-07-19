using System.Text;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

public static class MudBlazorDesignTokenCssGenerator
{
    public static string Generate(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        NerdThemeSetTools.SyncColorTokensFromThemeSets(options);
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

        if (options.UsePaletteFirstAdapter)
        {
            MudBlazorBrandPaletteGenerator.AppendBrandRootPalette(css, options);
            if (options.EmitFrameworkNeutralIntents)
            {
                NerdIntentCssGenerator.AppendBrandIntentVariables(css, options);
            }
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
            AppendFoundationRootVariables(css, options);
        }

        foreach (var duration in options.MotionDurations.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            css.AppendLine($".{options.Prefix}-motion-{duration.Key} {{ transition-property: all; transition-duration: var(--{options.Prefix}-duration-{duration.Key}); }}");
        }

        foreach (var easing in options.MotionEasings.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            css.AppendLine($".{options.Prefix}-ease-{easing.Key} {{ transition-timing-function: var(--{options.Prefix}-ease-{easing.Key}); }}");
        }

        foreach (var zIndex in options.ZIndex.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            css.AppendLine($".{options.Prefix}-z-{zIndex.Key} {{ z-index: var(--{options.Prefix}-z-{zIndex.Key}); }}");
        }

        foreach (var recipe in options.Recipes.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            AppendRecipe(css, options, recipe.Key, recipe.Value);
        }
        foreach (var opacity in options.Opacities.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            AppendOpacity(css, options, opacity.Key, opacity.Value);
        }

        AppendCatalogChromeRules(css, options);
        AppendCatalogToolbarRules(css, options);
        AppendPairingSurfaceRules(css, options);
        AppendRecipeTypographyOverrides(css, options);
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
        css.AppendLine($"  --mud-palette-text-primary: {contentColor}{important};");
        css.AppendLine($"  --mud-palette-text-secondary: {contentColor}{important};");
        css.AppendLine("}");
        css.AppendLine($"{root} .mud-typography, {root}.mud-paper .mud-typography, {root}.mud-card .mud-typography,");
        css.AppendLine($"{root}.mud-paper .mud-typography-h1, {root}.mud-paper .mud-typography-h2, {root}.mud-paper .mud-typography-h3,");
        css.AppendLine($"{root}.mud-paper .mud-typography-h4, {root}.mud-paper .mud-typography-h5, {root}.mud-paper .mud-typography-h6 {{");
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

        AppendRecipeNavigationRules(css, root, contentColor, actionColor, actionText, important);

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
        css.AppendLine($"[data-theme=\"dark\"] {root} .mud-typography, [data-theme=\"dark\"] {root}.mud-paper .mud-typography, [data-theme=\"dark\"] {root}.mud-card .mud-typography,");
        css.AppendLine($"[data-theme=\"dark\"] {root}.mud-paper .mud-typography-h1, [data-theme=\"dark\"] {root}.mud-paper .mud-typography-h2, [data-theme=\"dark\"] {root}.mud-paper .mud-typography-h3,");
        css.AppendLine($"[data-theme=\"dark\"] {root}.mud-paper .mud-typography-h4, [data-theme=\"dark\"] {root}.mud-paper .mud-typography-h5, [data-theme=\"dark\"] {root}.mud-paper .mud-typography-h6 {{");
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

        AppendRecipeNavigationRules(
            css,
            $"[data-theme=\"dark\"] {root}",
            darkContentColor,
            darkActionColor,
            darkActionText,
            important);
    }

    private static void AppendRecipeNavigationRules(
        StringBuilder css,
        string root,
        string contentColor,
        string actionColor,
        string actionText,
        string important)
    {
        css.AppendLine($"{root} .mud-navmenu, {root}.mud-navmenu {{");
        css.AppendLine($"  background-color: transparent{important};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-navmenu .mud-nav-link, {root}.mud-navmenu .mud-nav-link,");
        css.AppendLine($"{root} .mud-nav-link, {root}.mud-nav-link {{");
        css.AppendLine($"  color: {contentColor}{important};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-navmenu .mud-nav-link .mud-nav-link-text,");
        css.AppendLine($"{root} .mud-navmenu .mud-nav-link .mud-nav-link-icon,");
        css.AppendLine($"{root} .mud-navmenu .mud-nav-link .mud-icon-root,");
        css.AppendLine($"{root}.mud-navmenu .mud-nav-link .mud-nav-link-text,");
        css.AppendLine($"{root}.mud-navmenu .mud-nav-link .mud-nav-link-icon,");
        css.AppendLine($"{root}.mud-navmenu .mud-nav-link .mud-icon-root,");
        css.AppendLine($"{root} .mud-nav-link .mud-nav-link-text,");
        css.AppendLine($"{root} .mud-nav-link .mud-nav-link-icon,");
        css.AppendLine($"{root} .mud-nav-link .mud-icon-root,");
        css.AppendLine($"{root}.mud-nav-link .mud-nav-link-text,");
        css.AppendLine($"{root}.mud-nav-link .mud-nav-link-icon,");
        css.AppendLine($"{root}.mud-nav-link .mud-icon-root {{");
        css.AppendLine($"  color: inherit{important};");
        css.AppendLine($"  font-size: 0.875rem{important};");
        css.AppendLine($"  line-height: 1.43{important};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-navmenu .mud-nav-link:hover, {root}.mud-navmenu .mud-nav-link:hover,");
        css.AppendLine($"{root} .mud-navmenu .mud-nav-link.active, {root}.mud-navmenu .mud-nav-link.active,");
        css.AppendLine($"{root} .mud-nav-link:hover, {root}.mud-nav-link:hover,");
        css.AppendLine($"{root} .mud-nav-link.active, {root}.mud-nav-link.active {{");
        css.AppendLine($"  background-color: color-mix(in srgb, {actionColor} 12%, transparent){important};");
        css.AppendLine($"  color: {actionColor}{important};");
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-nav-link:hover .mud-nav-link-text,");
        css.AppendLine($"{root} .mud-nav-link:hover .mud-nav-link-icon,");
        css.AppendLine($"{root} .mud-nav-link:hover .mud-icon-root,");
        css.AppendLine($"{root} .mud-nav-link.active .mud-nav-link-text,");
        css.AppendLine($"{root} .mud-nav-link.active .mud-nav-link-icon,");
        css.AppendLine($"{root} .mud-nav-link.active .mud-icon-root,");
        css.AppendLine($"{root}.mud-nav-link:hover .mud-nav-link-text,");
        css.AppendLine($"{root}.mud-nav-link:hover .mud-nav-link-icon,");
        css.AppendLine($"{root}.mud-nav-link:hover .mud-icon-root,");
        css.AppendLine($"{root}.mud-nav-link.active .mud-nav-link-text,");
        css.AppendLine($"{root}.mud-nav-link.active .mud-nav-link-icon,");
        css.AppendLine($"{root}.mud-nav-link.active .mud-icon-root {{");
        css.AppendLine($"  color: {actionColor}{important};");
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
        var pageSurfaceVariable = options.Aliases.ContainsKey(NerdDesignSystemUi.PageSurface)
            ? $"--{prefix}-color-{NerdDesignSystemUi.PageSurface}"
            : string.Empty;

        css.AppendLine($"{root} {{");
        if (options.UsePaletteFirstAdapter)
        {
            MudBlazorPaletteMapper.AppendNerdColorVariables(
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
        }
        else
        {
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
        }
        css.AppendLine("}");

        if (options.UsePaletteFirstAdapter && options.IsAlias(name))
        {
            var importantSuffix = options.UseImportantOverrides ? " !important" : string.Empty;
            if (!options.UseIntentPseudoCssThemes && IsPaletteFirstSemanticAlias(name))
            {
                MudBlazorBrandPaletteGenerator.AppendIntentPaletteOverrides(
                    css,
                    root,
                    name,
                    variable,
                    textVariable,
                    hoverVariable,
                    contentVariable,
                    borderVariable,
                    importantSuffix);
            }
        }

        css.AppendLine($"[data-theme=\"dark\"] {root} {{");
        css.AppendLine($"  {variable}: {dark};");
        css.AppendLine($"  {textVariable}: {darkContrast};");
        css.AppendLine("}");

        if (!emitComponentRules)
        {
            return;
        }

        var bridgesOnly = options.UsePaletteFirstAdapter && options.IsAlias(name);
        var inactiveTabContentVariable = contentVariable;
        if (IsActionIntentAlias(name) && options.Aliases.ContainsKey(NerdDesignSystemUi.PageSurface))
        {
            inactiveTabContentVariable = $"--{prefix}-color-{NerdDesignSystemUi.PageSurface}-content";
        }

        var switchThumbVariable = textVariable;
        if (options.Aliases.ContainsKey(NerdDesignSystemUi.PageSurface)
            && NerdColorParser.ContrastRatio(light, contrast) < 3.0)
        {
            switchThumbVariable = $"--{prefix}-color-{NerdDesignSystemUi.PageSurface}-content";
        }

        var switchCheckedTrackBackground = ResolveSwitchCheckedTrackBackground(
            options,
            prefix,
            variable,
            pageSurfaceVariable,
            light);
        var switchTrackBorderVariable = hoverVariable;
        if (!string.IsNullOrWhiteSpace(pageSurfaceVariable)
            && options.Aliases.TryGetValue(NerdDesignSystemUi.PageSurface, out var surfaceName)
            && options.Colors.TryGetValue(surfaceName, out var surfaceToken)
            && NerdColorParser.ContrastRatio(
                light,
                NerdColorValue.Validate(surfaceToken.Light ?? surfaceToken.Value, nameof(surfaceToken.Value))) < 1.5)
        {
            switchTrackBorderVariable = $"--{prefix}-color-{NerdDesignSystemUi.PageSurface}-content";
        }

        var inputValueVariable = contentVariable;
        var inputBorderMixVariable = variable;
        if (options.Aliases.ContainsKey(NerdDesignSystemUi.PageSurface))
        {
            inputValueVariable = $"--{prefix}-color-{NerdDesignSystemUi.PageSurface}-content";
            if (!string.IsNullOrWhiteSpace(pageSurfaceVariable)
                && options.Aliases.TryGetValue(NerdDesignSystemUi.PageSurface, out var pageSurfaceName)
                && options.Colors.TryGetValue(pageSurfaceName, out var pageSurfaceToken))
            {
                var pageSurfaceLight = NerdColorValue.Validate(
                    pageSurfaceToken.Light ?? pageSurfaceToken.Value,
                    nameof(pageSurfaceToken.Value));
                if (NerdColorParser.ContrastRatio(light, pageSurfaceLight) < 3.0)
                {
                    inputBorderMixVariable = inputValueVariable;
                }
            }
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
            pageSurfaceVariable,
            options.UseImportantOverrides,
            bridgesOnly,
            inactiveTabContentVariable,
            switchThumbVariable,
            switchCheckedTrackBackground,
            switchTrackBorderVariable,
            inputValueVariable,
            inputBorderMixVariable);

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
                pageSurfaceVariable,
                options.UseImportantOverrides,
                bridgesOnly,
                inactiveTabContentVariable,
                switchThumbVariable,
                switchCheckedTrackBackground,
                switchTrackBorderVariable,
                inputValueVariable,
                inputBorderMixVariable);
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
        string.Equals(aliasName, NerdDesignSystemUi.BrandChrome, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.NavSurface, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.InputSurface, StringComparison.OrdinalIgnoreCase);

    private static string ResolveAliasColorVariable(NerdDesignTokenOptions options, string aliasName)
    {
        if (!options.Aliases.TryGetValue(aliasName, out var target))
        {
            return $"var(--{options.Prefix}-color-{aliasName})";
        }

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

        return $"var(--{options.Prefix}-color-{current})";
    }

    private static void AppendSurfaceRootStyles(StringBuilder css, NerdDesignTokenOptions options, string name)
    {
        var root = $".{options.Prefix}-{name}";
        var prefix = options.Prefix;
        var important = options.UseImportantOverrides ? " !important" : string.Empty;
        var surfaceVariable = $"--{prefix}-color-{name}-surface";
        var contentVariable = $"--{prefix}-color-{name}-content";
        var borderVariable = $"--{prefix}-color-{name}-border";
        var textVariable = $"--{prefix}-color-{name}-text";

        css.AppendLine($"{root} {{");
        css.AppendLine($"  background-color: var({surfaceVariable}){important};");
        css.AppendLine($"  color: var({contentVariable}){important};");
        if (options.UsePaletteFirstAdapter && !options.UseIntentPseudoCssThemes)
        {
            AppendSurfacePaletteLines(css, name, surfaceVariable, contentVariable, borderVariable, textVariable, important);
        }
        css.AppendLine("}");
        css.AppendLine();
    }

    private static void AppendSurfacePaletteLines(
        StringBuilder css,
        string aliasName,
        string surfaceVariable,
        string contentVariable,
        string borderVariable,
        string textVariable,
        string important)
    {
        if (string.Equals(aliasName, NerdDesignSystemUi.PageSurface, StringComparison.OrdinalIgnoreCase))
        {
            css.AppendLine($"  --mud-palette-surface: var({surfaceVariable}){important};");
            css.AppendLine($"  --mud-palette-background: var({surfaceVariable}){important};");
            css.AppendLine($"  --mud-palette-text-primary: var({contentVariable}){important};");
        }
        else if (string.Equals(aliasName, NerdDesignSystemUi.BrandChrome, StringComparison.OrdinalIgnoreCase))
        {
            css.AppendLine($"  --mud-palette-appbar-background: var({surfaceVariable}){important};");
            css.AppendLine($"  --mud-palette-appbar-text: var({textVariable}){important};");
        }
        else if (string.Equals(aliasName, NerdDesignSystemUi.NavSurface, StringComparison.OrdinalIgnoreCase))
        {
            css.AppendLine($"  --mud-palette-drawer-background: var({surfaceVariable}){important};");
            css.AppendLine($"  --mud-palette-drawer-text: var({contentVariable}){important};");
            css.AppendLine($"  --mud-palette-drawer-icon: var({contentVariable}){important};");
        }
        else if (string.Equals(aliasName, NerdDesignSystemUi.InputSurface, StringComparison.OrdinalIgnoreCase))
        {
            css.AppendLine($"  --mud-palette-surface: var({surfaceVariable}){important};");
            css.AppendLine($"  --mud-palette-lines-inputs: var({borderVariable}){important};");
            css.AppendLine($"  --mud-palette-text-primary: var({contentVariable}){important};");
        }
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

    private static void AppendCatalogChromeRules(StringBuilder css, NerdDesignTokenOptions options)
    {
        if (!options.Aliases.ContainsKey(NerdDesignSystemUi.PageSurface) ||
            !options.Aliases.ContainsKey(NerdDesignSystemUi.PrimaryAction))
        {
            return;
        }

        var prefix = options.Prefix;
        var important = options.UseImportantOverrides ? " !important" : string.Empty;
        var pageContent = $"var(--{prefix}-color-{NerdDesignSystemUi.PageSurface}-content)";
        var accent = $"var(--{prefix}-color-{NerdDesignSystemUi.PrimaryAction})";
        var accentRoot =
            $".{NerdDesignSystemUi.CatalogChromeClass} [data-nerd-accent=\"{prefix}-{NerdDesignSystemUi.PrimaryAction}\"]";

        css.AppendLine($"{accentRoot} .mud-typography:not(.mud-tab),");
        css.AppendLine($"{accentRoot} .mud-input-label,");
        css.AppendLine($"{accentRoot} .mud-input-label-inputcontrol,");
        css.AppendLine($"{accentRoot} .mud-input-control .mud-typography,");
        css.AppendLine($"{accentRoot} .mud-input-control-input-container .mud-typography,");
        css.AppendLine($"{accentRoot} .mud-switch + .mud-typography,");
        css.AppendLine($"{accentRoot} label,");
        css.AppendLine($"{accentRoot} .mud-input,");
        css.AppendLine($"{accentRoot} .mud-input-control {{");
        css.AppendLine($"  color: {pageContent}{important};");
        css.AppendLine($"  caret-color: {accent}{important};");
        css.AppendLine("}");
    }

    private static void AppendCatalogToolbarRules(StringBuilder css, NerdDesignTokenOptions options)
    {
        if (!options.Aliases.ContainsKey(NerdDesignSystemUi.PageSurface) ||
            !options.Aliases.ContainsKey(NerdDesignSystemUi.PrimaryAction))
        {
            return;
        }

        var prefix = options.Prefix;
        var important = options.UseImportantOverrides ? " !important" : string.Empty;
        var pageContent = $"var(--{prefix}-color-{NerdDesignSystemUi.PageSurface}-content)";
        var root = $"[data-nerd-style-guard=\"{NerdDesignSystemUi.CatalogToolbarPlacement}\"]";
        var accentRoot = $"{root} .{prefix}-{NerdDesignSystemUi.PrimaryAction}";

        css.AppendLine($"{root} .mud-typography:not(.mud-tab),");
        css.AppendLine($"{root} label {{");
        css.AppendLine($"  color: {pageContent}{important};");
        css.AppendLine("}");

        css.AppendLine($"{accentRoot} .mud-switch + .mud-typography,");
        css.AppendLine($"{accentRoot} .mud-switch .mud-typography,");
        css.AppendLine($"{accentRoot} label {{");
        css.AppendLine($"  color: {pageContent}{important};");
        css.AppendLine("}");
    }

    private static void AppendFoundationRootVariables(StringBuilder css, NerdDesignTokenOptions options)
    {
        var root = $".{MudBlazorPaletteManifest.BrandRootClass(options.Prefix)}";
        css.AppendLine($"{root} {{");
        foreach (var breakpoint in options.Breakpoints.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            css.AppendLine($"  --{options.Prefix}-breakpoint-{breakpoint.Key}: {breakpoint.Value};");
        }
        foreach (var duration in options.MotionDurations.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            css.AppendLine($"  --{options.Prefix}-duration-{duration.Key}: {duration.Value};");
        }
        foreach (var easing in options.MotionEasings.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            css.AppendLine($"  --{options.Prefix}-ease-{easing.Key}: {easing.Value};");
        }
        foreach (var zIndex in options.ZIndex.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            css.AppendLine($"  --{options.Prefix}-z-{zIndex.Key}: {zIndex.Value};");
        }
        css.AppendLine("}");
        css.AppendLine();
    }

    private static void AppendRecipeTypographyOverrides(StringBuilder css, NerdDesignTokenOptions options)
    {
        if (options.Recipes.Count == 0)
        {
            return;
        }

        var important = options.UseImportantOverrides ? " !important" : string.Empty;
        foreach (var recipe in options.Recipes.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            if (!options.Colors.TryGetValue(recipe.Value.Content, out var contentToken))
            {
                continue;
            }

            var contentColor = contentToken.Content
                               ?? NerdColorParser.ContentText(
                                   contentToken.Light ?? contentToken.Value,
                                   contentToken.ContrastText ?? NerdColorValue.ContrastText(contentToken.Light ?? contentToken.Value));
            var root = $".{options.Prefix}-recipe-{recipe.Key}.mud-paper";
            css.AppendLine($"{root} .mud-typography,");
            css.AppendLine($"{root} .mud-typography-h1, {root} .mud-typography-h2, {root} .mud-typography-h3,");
            css.AppendLine($"{root} .mud-typography-h4, {root} .mud-typography-h5, {root} .mud-typography-h6 {{");
            css.AppendLine($"  color: {contentColor}{important};");
            css.AppendLine($"  --mud-palette-text-primary: {contentColor}{important};");
            css.AppendLine("}");
        }

        css.AppendLine();
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
        css.AppendLine("}");
    }

    private static string ResolveSwitchCheckedTrackBackground(
        NerdDesignTokenOptions options,
        string prefix,
        string variable,
        string pageSurfaceVariable,
        string channelLight)
    {
        if (string.IsNullOrWhiteSpace(pageSurfaceVariable)
            || !options.Aliases.TryGetValue(NerdDesignSystemUi.PageSurface, out var pageName)
            || !options.Colors.TryGetValue(pageName, out var pageToken))
        {
            return $"var({variable})";
        }

        var pageLight = NerdColorValue.Validate(pageToken.Light ?? pageToken.Value, nameof(pageToken.Value));
        if (NerdColorParser.ContrastRatio(channelLight, pageLight) >= 1.5)
        {
            return $"var({variable})";
        }

        var pageContentVariable = $"--{prefix}-color-{NerdDesignSystemUi.PageSurface}-content";
        return $"color-mix(in srgb, var({pageContentVariable}) 50%, var({pageSurfaceVariable}))";
    }

    private static bool IsPaletteFirstSemanticAlias(string aliasName) =>
        IsActionIntentAlias(aliasName) || IsContentIntentAlias(aliasName);

    private static bool IsContentIntentAlias(string aliasName) =>
        string.Equals(aliasName, NerdDesignSystemUi.NavItem, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.NavItemActive, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.MutedContent, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.OnBrandChrome, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.OnPrimaryAction, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.FocusRing, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.InputBorder, StringComparison.OrdinalIgnoreCase);

    private static bool IsActionIntentAlias(string aliasName) =>
        string.Equals(aliasName, NerdDesignSystemUi.PrimaryAction, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.SecondaryAction, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.Highlight, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.Info, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.Success, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.Danger, StringComparison.OrdinalIgnoreCase);
}
