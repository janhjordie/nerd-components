using System.Reflection;
using MudBlazor;
using MudBlazor.Utilities;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Measures palette-first MudBlazor fidelity (HR-145 / TS-049 / HR-162).
/// </summary>
public static class NerdMudPaletteParityTools
{
    private const int MinimumPopulatedPaletteSlots = 35;

    public static NerdMudPaletteFidelityResult Evaluate(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var css = MudBlazorDesignTokenCssGenerator.Generate(options);
        var theme = NerdMudThemeFactory.Create(options);
        var checks = new List<NerdMudPaletteFidelityCheck>
        {
            EvaluateBrandRoot(options, css),
            EvaluateManifestCoverage(theme),
            EvaluatePrimaryActionIntent(options, css),
            EvaluateBridgesOnlySemantics(options, css),
            EvaluatePackPaletteBindings(options)
        };

        var score = checks.Count == 0
            ? 0
            : (int)Math.Round(checks.Average(check => check.Score));

        return new NerdMudPaletteFidelityResult(score, checks);
    }

    private static NerdMudPaletteFidelityCheck EvaluateBrandRoot(NerdDesignTokenOptions options, string css)
    {
        var brandRoot = $".{MudBlazorPaletteManifest.BrandRootClass(options.Prefix)}";
        var duplicateMarker = $"{brandRoot}, .mud-theme-provider {{";
        var hasDuplicate = css.Contains(duplicateMarker, StringComparison.Ordinal) &&
                           css.Contains("--mud-palette-primary:", StringComparison.Ordinal);

        var score = hasDuplicate ? 0 : 100;
        return new NerdMudPaletteFidelityCheck(
            "Brand root palette",
            score,
            hasDuplicate
                ? "CSS still duplicates global Mud palette at brand root (MudThemeProvider should own :root)."
                : "No duplicate global palette CSS; MudThemeProvider owns brand palette.");
    }

    private static NerdMudPaletteFidelityCheck EvaluateManifestCoverage(MudTheme theme)
    {
        var lightSlots = CountPopulatedPaletteProperties(theme.PaletteLight);
        var darkSlots = CountPopulatedPaletteProperties(theme.PaletteDark);
        var minimum = MinimumPopulatedPaletteSlots;
        var lightScore = Math.Min(100, (int)Math.Round(lightSlots * 100d / minimum));
        var darkScore = Math.Min(100, (int)Math.Round(darkSlots * 100d / minimum));
        var score = (lightScore + darkScore) / 2;

        return new NerdMudPaletteFidelityCheck(
            "Palette manifest",
            score,
            $"MudThemeProvider palette: {lightSlots} light + {darkSlots} dark populated slots (min {minimum} each).");
    }

    private static NerdMudPaletteFidelityCheck EvaluatePrimaryActionIntent(
        NerdDesignTokenOptions options,
        string css)
    {
        if (!options.Aliases.ContainsKey(NerdDesignSystemUi.PrimaryAction))
        {
            return new NerdMudPaletteFidelityCheck(
                "Primary intent",
                50,
                "No primary-action alias in pack.");
        }

        var root = $".{options.Prefix}-{NerdDesignSystemUi.PrimaryAction}";
        var hasIntent = css.Contains(
            $"{root} {{",
            StringComparison.Ordinal) &&
            css.Contains("--mud-palette-primary: var(--", StringComparison.Ordinal);
        var flattened = css.Contains(
            $"{root} {{  --mud-palette-secondary: var(--{options.Prefix}-color-{NerdDesignSystemUi.PrimaryAction})",
            StringComparison.Ordinal);
        var bulkButton = css.Contains(
            $"{root}[class*=\"mud-button-filled\"]",
            StringComparison.Ordinal);

        var score = hasIntent && !flattened && !bulkButton ? 100 : hasIntent ? 60 : 0;
        var detail = bulkButton
            ? "primary-action still uses bulk button patterns (palette-first bridges expected)."
            : flattened
                ? "primary-action still flattens unrelated palette channels."
                : hasIntent
                    ? "primary-action uses intent-scoped --mud-palette-primary override."
                    : "primary-action intent override missing.";

        return new NerdMudPaletteFidelityCheck("Primary intent", score, detail);
    }

    private static NerdMudPaletteFidelityCheck EvaluateBridgesOnlySemantics(
        NerdDesignTokenOptions options,
        string css)
    {
        var semanticAliases = options.Aliases.Keys
            .Where(IsPaletteFirstSemanticAliasForEvaluation)
            .ToList();

        if (semanticAliases.Count == 0)
        {
            return new NerdMudPaletteFidelityCheck("Semantic bridges", 50, "No palette-first semantic aliases.");
        }

        var passing = semanticAliases.Count(alias =>
        {
            var root = $".{options.Prefix}-{alias}";
            var hasBulkButton = css.Contains($"{root}[class*=\"mud-button-filled\"]", StringComparison.Ordinal);
            var hasIntentOrSurface = css.Contains($"{root} {{", StringComparison.Ordinal) &&
                                     css.Contains("--mud-palette-", StringComparison.Ordinal);
            return !hasBulkButton && hasIntentOrSurface;
        });

        var score = (int)Math.Round(passing * 100d / semanticAliases.Count);
        return new NerdMudPaletteFidelityCheck(
            "Semantic bridges",
            score,
            $"{passing}/{semanticAliases.Count} semantic aliases use palette overrides without bulk button patterns.");
    }

    private static NerdMudPaletteFidelityCheck EvaluatePackPaletteBindings(NerdDesignTokenOptions options)
    {
        var bindings = options.FrameworkDefaults?.MudBlazor?.Palette;
        if (bindings is null)
        {
            return new NerdMudPaletteFidelityCheck(
                "Pack palette map",
                options.UsePaletteFirstAdapter ? 70 : 0,
                "No explicit mudPalette bindings; convention fallback used.");
        }

        var checks = new[]
        {
            bindings.Primary,
            bindings.Secondary,
            bindings.Surface,
            bindings.AppbarBackground,
            bindings.DrawerBackground
        };
        var present = checks.Count(value => !string.IsNullOrWhiteSpace(value));
        var score = (int)Math.Round(present * 100d / checks.Length);
        return new NerdMudPaletteFidelityCheck(
            "Pack palette map",
            score,
            $"{present}/{checks.Length} core mudPalette bindings configured in token pack.");
    }

    private static int CountPopulatedPaletteProperties(Palette palette) =>
        palette.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.CanRead && IsPaletteSlotProperty(property.PropertyType))
            .Count(property => IsPopulatedPaletteValue(property.GetValue(palette)));

    private static bool IsPaletteSlotProperty(Type propertyType) =>
        propertyType == typeof(string) || propertyType == typeof(MudColor);

    private static bool IsPopulatedPaletteValue(object? value) =>
        value switch
        {
            null => false,
            string text => !string.IsNullOrWhiteSpace(text),
            MudColor color => color != default,
            _ => false
        };

    private static bool IsPaletteFirstSemanticAliasForEvaluation(string aliasName) =>
        string.Equals(aliasName, NerdDesignSystemUi.PrimaryAction, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.SecondaryAction, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.MutedContent, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.NavItem, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.NavItemActive, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.Highlight, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.Info, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.Success, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.Danger, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.OnBrandChrome, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.OnPrimaryAction, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.FocusRing, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(aliasName, NerdDesignSystemUi.InputBorder, StringComparison.OrdinalIgnoreCase);
}

public sealed record NerdMudPaletteFidelityResult(
    int Score,
    IReadOnlyList<NerdMudPaletteFidelityCheck> Checks);

public sealed record NerdMudPaletteFidelityCheck(
    string Name,
    int Score,
    string Detail);
