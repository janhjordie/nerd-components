namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Framework-neutral design-to-code parity checks (HR-114 wave 2).</summary>
public static class NerdCoreParityTools
{
    public static NerdDesignParityResult Evaluate(
        NerdDesignTokenOptions options,
        bool includeExtendedRecipes = false)
    {
        ArgumentNullException.ThrowIfNull(options);

        var requiredRecipes = includeExtendedRecipes
            ? NerdShellRecipeCatalog.ExtendedShellRecipes
            : NerdShellRecipeCatalog.CoreShellRecipes;

        var metrics = new List<NerdBrandHealthMetric>
        {
            EvaluateShellBindings(options),
            EvaluateRequiredRecipes(options, requiredRecipes),
            EvaluateApprovedPairings(options),
            EvaluateFrameworkDefaults(options),
            NerdBrandHealthTools.Evaluate(options).Metrics.First(metric =>
                string.Equals(metric.Name, "Contrast", StringComparison.Ordinal))
        };

        var score = metrics.Count == 0
            ? 0
            : (int)Math.Round(metrics.Average(metric => metric.Score));
        return new NerdDesignParityResult(score, metrics);
    }

    private static NerdBrandHealthMetric EvaluateShellBindings(NerdDesignTokenOptions options)
    {
        var shell = NerdTokenPackShellTools.ResolveShell(options);
        var slots = new (string Name, NerdTokenPackShellSlot? Slot)[]
        {
            ("appBar", shell.AppBar),
            ("drawer", shell.Drawer),
            ("navMenu", shell.NavMenu),
            ("main", shell.Main)
        };

        var present = slots.Count(slot => slot.Slot is not null);
        var score = (int)Math.Round(present * 100d / slots.Length);
        return new NerdBrandHealthMetric(
            "Shell bindings",
            score,
            $"{present}/{slots.Length} shell slots configured (appBar, drawer, navMenu, main).");
    }

    private static NerdBrandHealthMetric EvaluateRequiredRecipes(
        NerdDesignTokenOptions options,
        IReadOnlyList<string> requiredRecipes)
    {
        if (requiredRecipes.Count == 0)
        {
            return new NerdBrandHealthMetric("Shell recipes", 100, "No required recipes.");
        }

        var present = requiredRecipes.Count(name => options.Recipes.ContainsKey(name));
        var missing = requiredRecipes
            .Where(name => !options.Recipes.ContainsKey(name))
            .ToList();
        var score = (int)Math.Round(present * 100d / requiredRecipes.Count);
        var detail = missing.Count == 0
            ? $"All {requiredRecipes.Count} required shell recipes present."
            : $"Missing recipes: {string.Join(", ", missing)}.";
        return new NerdBrandHealthMetric("Shell recipes", score, detail);
    }

    private static NerdBrandHealthMetric EvaluateApprovedPairings(NerdDesignTokenOptions options)
    {
        var policy = options.PairingPolicy;
        if (policy is null)
        {
            var recipePairings = options.Recipes.Values
                .Select(recipe => NerdTokenPairingTools.ValidatePairing(recipe.Content, recipe.Surface, options))
                .ToList();
            if (recipePairings.Count == 0)
            {
                return new NerdBrandHealthMetric("Approved pairings", 50, "No pairing guide; recipe contrasts not evaluated.");
            }

            var passing = recipePairings.Count(result => result.MeetsAa);
            var score = (int)Math.Round(passing * 100d / recipePairings.Count);
            return new NerdBrandHealthMetric(
                "Approved pairings",
                score,
                $"{passing}/{recipePairings.Count} recipe content/surface pairs pass WCAG AA.");
        }

        var approved = policy.GetApprovedPairings().ToList();
        if (approved.Count == 0)
        {
            return new NerdBrandHealthMetric("Approved pairings", 0, "Pairing guide has no approved pairings.");
        }

        var approvedPassing = approved.Count(pair =>
            NerdTokenPairingTools.ValidatePairing(pair.Content, pair.Surface, options).MeetsAa);
        var approvedScore = (int)Math.Round(approvedPassing * 100d / approved.Count);
        return new NerdBrandHealthMetric(
            "Approved pairings",
            approvedScore,
            $"{approvedPassing}/{approved.Count} identity-guide pairings pass WCAG AA.");
    }

    private static NerdBrandHealthMetric EvaluateFrameworkDefaults(NerdDesignTokenOptions options)
    {
        var defaults = options.FrameworkDefaults?.MudBlazor
                       ?? NerdTokenPackShellTools.DefaultMudBlazorDefaults;
        var checks = new List<bool>
        {
            !string.IsNullOrWhiteSpace(defaults.Button?.Filled),
            !string.IsNullOrWhiteSpace(defaults.Button?.Outlined),
            !string.IsNullOrWhiteSpace(defaults.NavLink?.Default),
            !string.IsNullOrWhiteSpace(defaults.NavLink?.Active),
            !string.IsNullOrWhiteSpace(defaults.TextField?.Intent)
        };

        var present = checks.Count(check => check);
        var score = (int)Math.Round(present * 100d / checks.Count);
        return new NerdBrandHealthMetric(
            "Framework defaults",
            score,
            $"{present}/{checks.Count} MudBlazor default intent mappings configured.");
    }
}

public sealed record NerdDesignParityResult(int Score, IReadOnlyList<NerdBrandHealthMetric> Metrics);
