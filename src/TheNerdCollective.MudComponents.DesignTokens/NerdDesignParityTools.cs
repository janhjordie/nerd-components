namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Full design parity including Mud-specific palette and style-guard checks (adapter layer).
/// </summary>
public static class NerdDesignParityTools
{
    public static readonly string[] CoreShellRecipes = NerdShellRecipeCatalog.CoreShellRecipes;

    public static readonly string[] ExtendedShellRecipes = NerdShellRecipeCatalog.ExtendedShellRecipes;

    public static NerdDesignParityResult Evaluate(
        NerdDesignTokenOptions options,
        bool includeExtendedRecipes = false)
    {
        ArgumentNullException.ThrowIfNull(options);

        var core = NerdCoreParityTools.Evaluate(options, includeExtendedRecipes);
        var metrics = core.Metrics.ToList();
        metrics.Add(EvaluatePaletteBindings(options));
        metrics.Add(EvaluateStyleGuardPlacements(options));

        var score = metrics.Count == 0
            ? 0
            : (int)Math.Round(metrics.Average(metric => metric.Score));
        return new NerdDesignParityResult(score, metrics);
    }

    private static NerdBrandHealthMetric EvaluatePaletteBindings(NerdDesignTokenOptions options)
    {
        var fidelity = NerdMudPaletteParityTools.Evaluate(options);
        var packMap = fidelity.Checks.FirstOrDefault(check =>
            string.Equals(check.Name, "Pack palette map", StringComparison.Ordinal));
        var manifest = fidelity.Checks.FirstOrDefault(check =>
            string.Equals(check.Name, "Palette manifest", StringComparison.Ordinal));
        var score = packMap is null || manifest is null
            ? fidelity.Score
            : (int)Math.Round((packMap.Score + manifest.Score) / 2d);

        return new NerdBrandHealthMetric(
            "Mud palette map",
            score,
            $"{packMap?.Detail ?? "n/a"} · {manifest?.Detail ?? "n/a"}");
    }

    private static NerdBrandHealthMetric EvaluateStyleGuardPlacements(NerdDesignTokenOptions options)
    {
        var violations = NerdStyleGuardTools.ValidateCatalogChromePlacement(options);
        var accentWarnings = NerdStyleGuardTools.ValidateCatalogChromeAccentWarnings(options);
        if (violations.Count == 0)
        {
            var detail = accentWarnings.Count == 0
                ? "Catalog chrome placements pass contrast checks."
                : $"Label text passes; {accentWarnings.Count} accent-on-page warning(s) below 3:1.";
            var score = accentWarnings.Count == 0 ? 100 : 85;
            return new NerdBrandHealthMetric("Style guard", score, detail);
        }

        var summary = string.Join(
            ", ",
            violations.Select(violation => $"{violation.Placement}/{violation.Role}"));
        return new NerdBrandHealthMetric(
            "Style guard",
            0,
            $"{violations.Count} placement violation(s): {summary}.");
    }
}
