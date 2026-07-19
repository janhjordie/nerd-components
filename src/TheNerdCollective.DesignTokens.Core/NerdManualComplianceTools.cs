using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Brand-manual compliance checks beyond generic brand health (HR-149).</summary>
public static class NerdManualComplianceTools
{
    public static NerdManualComplianceResult Evaluate(
        NerdDesignTokenOptions options,
        bool includeExtendedShellRecipes = true)
    {
        ArgumentNullException.ThrowIfNull(options);

        var requiredRecipes = includeExtendedShellRecipes
            ? NerdShellRecipeCatalog.ExtendedShellRecipes
            : NerdShellRecipeCatalog.CoreShellRecipes;

        var metrics = new List<NerdBrandHealthMetric>
        {
            EvaluateShellRecipes(options, requiredRecipes),
            EvaluateApprovedPairings(options),
            EvaluateHeroOverlayTokens(options)
        };

        var score = metrics.Count == 0
            ? 0
            : (int)Math.Round(metrics.Average(metric => metric.Score));

        return new NerdManualComplianceResult(score, metrics);
    }

    private static NerdBrandHealthMetric EvaluateShellRecipes(
        NerdDesignTokenOptions options,
        IReadOnlyList<string> requiredRecipes)
    {
        var missing = requiredRecipes
            .Where(name => !options.Recipes.ContainsKey(name))
            .ToList();
        var present = requiredRecipes.Count - missing.Count;
        var score = requiredRecipes.Count == 0
            ? 100
            : (int)Math.Round(present * 100d / requiredRecipes.Count);

        var detail = missing.Count == 0
            ? $"All {requiredRecipes.Count} shell recipes configured."
            : $"Missing recipes: {string.Join(", ", missing)}.";

        return new NerdBrandHealthMetric("Shell recipes", score, detail);
    }

    private static NerdBrandHealthMetric EvaluateApprovedPairings(NerdDesignTokenOptions options)
    {
        var policy = options.PairingPolicy;
        if (policy is null)
        {
            return new NerdBrandHealthMetric(
                "Approved pairings",
                options.Recipes.Count > 0 ? 70 : 40,
                "No pairing policy — recipe contrasts not checked against a manual.");
        }

        var approved = policy.GetApprovedPairings();
        if (approved.Count == 0)
        {
            return new NerdBrandHealthMetric("Approved pairings", 50, "Pairing guide has no approved combinations.");
        }

        var covered = approved.Count(pair =>
            options.Recipes.Values.Any(recipe =>
                (string.Equals(recipe.Content, pair.Content, StringComparison.OrdinalIgnoreCase) &&
                 string.Equals(recipe.Surface, pair.Surface, StringComparison.OrdinalIgnoreCase)) ||
                NerdTokenPairingTools.ValidatePairing(pair.Content, pair.Surface, options).MeetsAa));

        var score = (int)Math.Round(covered * 100d / approved.Count);
        return new NerdBrandHealthMetric(
            "Approved pairings",
            score,
            $"{covered}/{approved.Count} manual pairings covered by recipes or WCAG AA.");
    }

    private static NerdBrandHealthMetric EvaluateHeroOverlayTokens(NerdDesignTokenOptions options)
    {
        var needsOverlay = options.Recipes.ContainsKey("hero-photo") ||
                           options.Recipes.ContainsKey("hero-organic");
        if (!needsOverlay)
        {
            return new NerdBrandHealthMetric("Hero overlays", 100, "No photo/organic hero recipes — overlay optional.");
        }

        if (options.Opacities.ContainsKey("hero-overlay"))
        {
            return new NerdBrandHealthMetric(
                "Hero overlays",
                100,
                "hero-overlay opacity token configured for photo/organic heroes.");
        }

        return new NerdBrandHealthMetric(
            "Hero overlays",
            0,
            "Add hero-overlay opacity token for hero-photo / hero-organic recipes.");
    }
}

public sealed record NerdManualComplianceResult(int Score, IReadOnlyList<NerdBrandHealthMetric> Metrics);
