using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Validates composed token placements (intent scopes on parent surfaces) that static token checks miss.
/// </summary>
public static class NerdStyleGuardTools
{
    public const double AaNormalTextRatio = 4.5;
    public const double UiComponentRatio = 3.0;

    public static IReadOnlyList<NerdStyleGuardViolation> ValidatePlacements(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var violations = new List<NerdStyleGuardViolation>();
        violations.AddRange(ValidateCatalogChromePlacement(options));
        return violations;
    }

    /// <summary>
    /// Advisory: status/fill intents whose accent fails UI contrast (≥ 3:1) on page-surface.
    /// Do not use these as Outlined/Text chrome — NRDT001 flags the markup; this surfaces the token risk in catalog.
    /// Not included in <see cref="AssertPlacementCompliance"/> (token can stay light for Filled usage).
    /// </summary>
    public static IReadOnlyList<NerdStyleGuardViolation> ValidateOutlinedStatusIntentWarnings(
        NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (!options.Aliases.ContainsKey(NerdDesignSystemUi.PageSurface))
        {
            return [];
        }

        var variables = NerdDesignTokenColorVariables.Build(options);
        var pageSurface = ResolveAliasSurfaceColor(options, NerdDesignSystemUi.PageSurface);
        var warnings = new List<NerdStyleGuardViolation>();
        foreach (var alias in OutlinedStatusIntentAliases)
        {
            if (!options.Aliases.ContainsKey(alias))
            {
                continue;
            }

            var accent = ResolveAliasAccentColor(options, alias);
            AddContrastCheck(
                warnings,
                NerdDesignSystemUi.CatalogChromePlacement,
                $"outlined-status-intent:{alias}",
                accent,
                pageSurface,
                UiComponentRatio,
                variables);
        }

        return warnings;
    }

    private static readonly string[] OutlinedStatusIntentAliases =
    [
        NerdDesignSystemUi.Info,
        NerdDesignSystemUi.Success,
        NerdDesignSystemUi.Highlight
    ];

    public static IReadOnlyList<NerdStyleGuardViolation> ValidateCatalogChromePlacement(
        NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (!options.Aliases.ContainsKey(NerdDesignSystemUi.PageSurface) ||
            !options.Aliases.ContainsKey(NerdDesignSystemUi.PrimaryAction))
        {
            return [];
        }

        var variables = NerdDesignTokenColorVariables.Build(options);
        var pageSurface = ResolveAliasSurfaceColor(options, NerdDesignSystemUi.PageSurface);
        var pageContent = ResolveAliasContentColor(options, NerdDesignSystemUi.PageSurface);
        var violations = new List<NerdStyleGuardViolation>();

        AddContrastCheck(
            violations,
            NerdDesignSystemUi.CatalogChromePlacement,
            "label-text",
            pageContent,
            pageSurface,
            AaNormalTextRatio,
            variables);

        // Outlined / text BrandChrome controls on page-surface (WCAG UI component contrast ≥ 3:1).
        // Paint must use BrandChrome accent — never OnBrandChrome (filled ContrastText → white-on-white).
        if (options.Aliases.ContainsKey(NerdDesignSystemUi.BrandChrome))
        {
            var brandChrome = ResolveAliasAccentColor(options, NerdDesignSystemUi.BrandChrome);
            AddContrastCheck(
                violations,
                NerdDesignSystemUi.CatalogChromePlacement,
                "outlined-control",
                brandChrome,
                pageSurface,
                UiComponentRatio,
                variables);
        }

        return violations;
    }

    /// <summary>
    /// True when generated CSS paints BrandChrome outlined controls with OnBrandChrome (white-on-light).
    /// </summary>
    public static bool CssPaintsOutlinedBrandChromeWithContrastText(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (!options.Aliases.ContainsKey(NerdDesignSystemUi.BrandChrome) ||
            !options.Aliases.ContainsKey(NerdDesignSystemUi.OnBrandChrome))
        {
            return false;
        }

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);
        var prefix = options.Prefix;
        var outlinedNeedle =
            $".{prefix}-{NerdDesignSystemUi.BrandChrome}.mud-button-outlined";
        var contrastVar = $"var(--{prefix}-color-{NerdDesignSystemUi.OnBrandChrome})";
        var accentVar = $"var(--{prefix}-color-{NerdDesignSystemUi.BrandChrome})";

        var index = 0;
        while ((index = css.IndexOf(outlinedNeedle, index, StringComparison.Ordinal)) >= 0)
        {
            var blockEnd = css.IndexOf('}', index);
            if (blockEnd < 0)
            {
                break;
            }

            var block = css[index..blockEnd];
            if (block.Contains(contrastVar, StringComparison.Ordinal)
                && !block.Contains($"color: {accentVar}", StringComparison.Ordinal))
            {
                return true;
            }

            index = blockEnd + 1;
        }

        return false;
    }

    /// <summary>
    /// Advisory: OnBrandChrome (filled ContrastText) on page-surface fails UI contrast — do not use for outlined controls.
    /// </summary>
    public static IReadOnlyList<NerdStyleGuardViolation> ValidateOutlinedContrastTextAntipattern(
        NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (!options.Aliases.ContainsKey(NerdDesignSystemUi.PageSurface) ||
            !options.Aliases.ContainsKey(NerdDesignSystemUi.OnBrandChrome))
        {
            return [];
        }

        var variables = NerdDesignTokenColorVariables.Build(options);
        var pageSurface = ResolveAliasSurfaceColor(options, NerdDesignSystemUi.PageSurface);
        var onBrandChrome = ResolveAliasAccentColor(options, NerdDesignSystemUi.OnBrandChrome);
        var warnings = new List<NerdStyleGuardViolation>();
        AddContrastCheck(
            warnings,
            NerdDesignSystemUi.CatalogChromePlacement,
            "outlined-contrast-text-antipattern",
            onBrandChrome,
            pageSurface,
            UiComponentRatio,
            variables);
        return warnings;
    }

    public static IReadOnlyList<NerdStyleGuardViolation> ValidateCatalogChromeAccentWarnings(
        NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (!options.Aliases.ContainsKey(NerdDesignSystemUi.PageSurface) ||
            !options.Aliases.ContainsKey(NerdDesignSystemUi.PrimaryAction))
        {
            return [];
        }

        var variables = NerdDesignTokenColorVariables.Build(options);
        var pageSurface = ResolveAliasSurfaceColor(options, NerdDesignSystemUi.PageSurface);
        var accent = ResolveAliasAccentColor(options, NerdDesignSystemUi.PrimaryAction);
        var warnings = new List<NerdStyleGuardViolation>();

        AddContrastCheck(
            warnings,
            NerdDesignSystemUi.CatalogChromePlacement,
            "accent-control",
            accent,
            pageSurface,
            UiComponentRatio,
            variables);

        return warnings;
    }

    public static void AssertPlacementCompliance(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var violations = ValidatePlacements(options);
        if (violations.Count == 0)
        {
            return;
        }

        var summary = string.Join(
            Environment.NewLine,
            violations.Select(violation =>
                $"{violation.Placement}/{violation.Role}: {violation.ContrastRatio:0.0}:1 < {violation.RequiredRatio:0.0}:1 ({violation.Foreground} on {violation.Background})"));

        throw new InvalidOperationException(
            $"Style guard failed with {violations.Count} placement violation(s):{Environment.NewLine}{summary}");
    }

    internal static string ResolveAliasSurfaceColor(NerdDesignTokenOptions options, string aliasName)
    {
        var token = ResolveAliasToken(options, aliasName);
        return token.Surface ?? token.Light ?? token.Value;
    }

    internal static string ResolveAliasContentColor(NerdDesignTokenOptions options, string aliasName)
    {
        var token = ResolveAliasToken(options, aliasName);
        var light = token.Light ?? token.Value;
        var contrast = token.ContrastText ?? NerdColorValue.ContrastText(light);
        return token.Content ?? NerdColorParser.ContentText(light, contrast);
    }

    internal static string ResolveAliasAccentColor(NerdDesignTokenOptions options, string aliasName)
    {
        var token = ResolveAliasToken(options, aliasName);
        return token.Light ?? token.Value;
    }

    private static NerdColorToken ResolveAliasToken(NerdDesignTokenOptions options, string aliasName)
    {
        if (!options.Aliases.TryGetValue(aliasName, out var targetName))
        {
            throw new ArgumentException($"Missing alias '{aliasName}'.", nameof(aliasName));
        }

        var current = targetName;
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (options.Aliases.TryGetValue(current, out var next))
        {
            if (!visited.Add(current))
            {
                break;
            }

            current = next;
        }

        if (!options.Colors.TryGetValue(current, out var token))
        {
            throw new ArgumentException($"Alias '{aliasName}' resolves to missing token '{current}'.", nameof(aliasName));
        }

        return token;
    }

    private static void AddContrastCheck(
        ICollection<NerdStyleGuardViolation> violations,
        string placement,
        string role,
        string foreground,
        string background,
        double requiredRatio,
        IReadOnlyDictionary<string, string> variables)
    {
        var ratio = NerdColorParser.ContrastRatio(background, foreground, variables);
        if (ratio + 0.0001 < requiredRatio)
        {
            violations.Add(new NerdStyleGuardViolation(
                placement,
                role,
                foreground,
                background,
                ratio,
                requiredRatio));
        }
    }
}
