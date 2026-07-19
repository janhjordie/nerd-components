using System.Text.RegularExpressions;

namespace TheNerdCollective.MudComponents.DesignTokens;

public static class NerdBrandHealthTools
{
    private static readonly Regex KebabCasePattern = new("^[a-z][a-z0-9]*(-[a-z0-9]+)*$", RegexOptions.Compiled);

    public static NerdBrandHealthResult Evaluate(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var metrics = new[]
        {
            EvaluateContrast(options),
            EvaluateNaming(options),
            EvaluateRecipeCoverage(options),
            EvaluateUnusedTokens(options)
        };

        var score = metrics.Length == 0
            ? 0
            : (int)Math.Round(metrics.Average(metric => metric.Score));
        return new NerdBrandHealthResult(score, metrics);
    }

    private static NerdBrandHealthMetric EvaluateContrast(NerdDesignTokenOptions options)
    {
        if (options.Colors.Count == 0)
        {
            return new NerdBrandHealthMetric("Contrast", 0, "No color tokens configured.");
        }

        var results = NerdDesignTokenTools.CheckAccessibility(options);
        var passing = results.Count(result => result.MeetsAa);
        var score = (int)Math.Round(passing * 100d / results.Count);
        return new NerdBrandHealthMetric(
            "Contrast",
            score,
            $"{passing}/{results.Count} tokens pass WCAG {options.WcagVersion} AA.");
    }

    private static NerdBrandHealthMetric EvaluateNaming(NerdDesignTokenOptions options)
    {
        if (options.Colors.Count == 0)
        {
            return new NerdBrandHealthMetric("Naming", 0, "No color tokens configured.");
        }

        var validNames = options.Colors.Keys.Count(name => KebabCasePattern.IsMatch(name));
        var aliasValid = options.Aliases.Keys.Count(name => KebabCasePattern.IsMatch(name));
        var total = options.Colors.Count + options.Aliases.Count;
        var valid = validNames + aliasValid;
        var score = total == 0 ? 0 : (int)Math.Round(valid * 100d / total);
        return new NerdBrandHealthMetric(
            "Naming",
            score,
            $"{valid}/{total} token and alias names use kebab-case.");
    }

    private static NerdBrandHealthMetric EvaluateRecipeCoverage(NerdDesignTokenOptions options)
    {
        if (options.Colors.Count == 0)
        {
            return new NerdBrandHealthMetric("Recipe coverage", 0, "No color tokens configured.");
        }

        if (options.Recipes.Count == 0)
        {
            return new NerdBrandHealthMetric("Recipe coverage", 0, "No recipes configured.");
        }

        var referenced = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var recipe in options.Recipes.Values)
        {
            referenced.Add(recipe.Surface);
            referenced.Add(recipe.Content);
            if (recipe.Action is not null)
            {
                referenced.Add(recipe.Action);
            }

            if (recipe.Border is not null)
            {
                referenced.Add(recipe.Border);
            }
        }

        var covered = options.Colors.Keys.Count(name => referenced.Contains(name));
        var score = (int)Math.Round(covered * 100d / options.Colors.Count);
        return new NerdBrandHealthMetric(
            "Recipe coverage",
            score,
            $"{covered}/{options.Colors.Count} color tokens used in recipes.");
    }

    private static NerdBrandHealthMetric EvaluateUnusedTokens(NerdDesignTokenOptions options)
    {
        if (options.Colors.Count == 0)
        {
            return new NerdBrandHealthMetric("Unused tokens", 0, "No color tokens configured.");
        }

        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var recipe in options.Recipes.Values)
        {
            used.Add(recipe.Surface);
            used.Add(recipe.Content);
            if (recipe.Action is not null)
            {
                used.Add(recipe.Action);
            }

            if (recipe.Border is not null)
            {
                used.Add(recipe.Border);
            }
        }

        foreach (var alias in options.Aliases.Values)
        {
            used.Add(alias);
        }

        foreach (var opacity in options.Opacities.Values)
        {
            used.Add(opacity.BaseToken);
        }

        var unused = options.Colors.Keys.Count(name => !used.Contains(name));
        var score = (int)Math.Round((options.Colors.Count - unused) * 100d / options.Colors.Count);
        return new NerdBrandHealthMetric(
            "Unused tokens",
            score,
            unused == 0
                ? "All color tokens are referenced by recipes, aliases or overlays."
                : $"{unused} color token{(unused == 1 ? "" : "s")} not referenced yet.");
    }
}
