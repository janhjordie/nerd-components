using System.Text.RegularExpressions;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Actionable naming and structure lint for token packs (HR-108).</summary>
public static class NerdTokenStructureLinterTools
{
    private static readonly Regex KebabCasePattern = new(
        "^[a-z][a-z0-9]*(-[a-z0-9]+)*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static IReadOnlyList<NerdTokenLintIssue> Analyze(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var issues = new List<NerdTokenLintIssue>();

        foreach (var name in options.Colors.Keys)
        {
            if (!KebabCasePattern.IsMatch(name))
            {
                issues.Add(new(NerdTokenLintIssueCode.InvalidName, name, $"Color token '{name}' should use kebab-case."));
            }
        }

        foreach (var name in options.Aliases.Keys)
        {
            if (!KebabCasePattern.IsMatch(name))
            {
                issues.Add(new(NerdTokenLintIssueCode.InvalidName, name, $"Alias '{name}' should use kebab-case."));
            }

            if (!options.Colors.ContainsKey(options.Aliases[name]) && !options.Aliases.ContainsKey(options.Aliases[name]))
            {
                issues.Add(new(NerdTokenLintIssueCode.MissingReference, name, $"Alias '{name}' points to missing token '{options.Aliases[name]}'."));
            }

            if (HasAliasCycle(options, name))
            {
                issues.Add(new(NerdTokenLintIssueCode.AliasCycle, name, $"Alias '{name}' participates in a reference cycle."));
            }
        }

        foreach (var pair in options.Colors.GroupBy(pair => NormalizeHex(pair.Value.Value ?? pair.Value.Light ?? string.Empty), StringComparer.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(pair.Key) || pair.Count() == 1)
            {
                continue;
            }

            issues.Add(new(
                NerdTokenLintIssueCode.DuplicateValue,
                string.Join(", ", pair.Select(entry => entry.Key)),
                $"Duplicate color value {pair.Key} on {pair.Count()} tokens."));
        }

        foreach (var color in options.Colors.Keys)
        {
            if (!IsColorReferenced(options, color))
            {
                issues.Add(new(NerdTokenLintIssueCode.UnusedToken, color, $"Color token '{color}' is not referenced by recipes, aliases, opacities, or transforms."));
            }
        }

        if (options.Recipes.Count == 0 && options.Colors.Count > 0)
        {
            issues.Add(new(NerdTokenLintIssueCode.MissingRecipeCoverage, null, "No recipes configured for semantic coverage."));
        }

        foreach (var transform in options.Transforms)
        {
            if (!options.Colors.ContainsKey(transform.Value.Source) &&
                !options.Aliases.ContainsKey(transform.Value.Source))
            {
                issues.Add(new(
                    NerdTokenLintIssueCode.MissingReference,
                    transform.Key,
                    $"Transform '{transform.Key}' references missing source '{transform.Value.Source}'."));
            }
        }

        return issues
            .OrderBy(issue => issue.Severity)
            .ThenBy(issue => issue.Code)
            .ThenBy(issue => issue.TokenName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool HasAliasCycle(NerdDesignTokenOptions options, string aliasName)
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var current = aliasName;
        while (options.Aliases.TryGetValue(current, out var target))
        {
            if (!visited.Add(current))
            {
                return true;
            }

            current = target;
        }

        return false;
    }

    private static bool IsColorReferenced(NerdDesignTokenOptions options, string colorName)
    {
        if (options.Aliases.Values.Any(value => string.Equals(value, colorName, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        foreach (var recipe in options.Recipes.Values)
        {
            if (TokenEquals(recipe.Surface, colorName) ||
                TokenEquals(recipe.Content, colorName) ||
                TokenEquals(recipe.Action, colorName) ||
                TokenEquals(recipe.Border, colorName))
            {
                return true;
            }
        }

        foreach (var opacity in options.Opacities.Values)
        {
            if (TokenEquals(opacity.BaseToken, colorName))
            {
                return true;
            }
        }

        foreach (var transform in options.Transforms.Values)
        {
            if (TokenEquals(transform.Source, colorName))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TokenEquals(string? left, string right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

    private static string NormalizeHex(string value) =>
        value.Trim().ToUpperInvariant();
}

public enum NerdTokenLintIssueCode
{
    InvalidName,
    MissingReference,
    AliasCycle,
    DuplicateValue,
    UnusedToken,
    MissingRecipeCoverage,
    InvalidTransform
}

public enum NerdTokenLintSeverity
{
    Info,
    Warning,
    Error
}

public sealed record NerdTokenLintIssue(
    NerdTokenLintIssueCode Code,
    string? TokenName,
    string Message)
{
    public NerdTokenLintSeverity Severity => Code switch
    {
        NerdTokenLintIssueCode.MissingReference or NerdTokenLintIssueCode.AliasCycle or NerdTokenLintIssueCode.InvalidTransform => NerdTokenLintSeverity.Error,
        NerdTokenLintIssueCode.InvalidName or NerdTokenLintIssueCode.DuplicateValue or NerdTokenLintIssueCode.UnusedToken => NerdTokenLintSeverity.Warning,
        _ => NerdTokenLintSeverity.Info
    };
}
