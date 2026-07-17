using System.Text.Json;
using Microsoft.Extensions.Logging;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

public static class NerdDesignTokenTools
{
    public const string DefaultWcagVersion = WcagStandards.DefaultVersion;
    public const double AaNormalTextRatio = WcagStandards.AaNormalTextRatio;
    public const double AaaNormalTextRatio = WcagStandards.AaaNormalTextRatio;

    public static void WriteCss(NerdDesignTokenOptions options, string path)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        File.WriteAllText(path, MudBlazorDesignTokenCssGenerator.Generate(options));
    }

    public static string ExportJson(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return JsonSerializer.Serialize(options.Colors);
    }

    public static string ExportStitchDesignMd(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var builder = new System.Text.StringBuilder("# Design tokens\n\n");
        builder.AppendLine("## Colors");
        builder.AppendLine();
        foreach (var pair in options.Colors.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            builder.AppendLine($"- **{pair.Key}**: `{pair.Value.Value}`");
        }

        builder.AppendLine();
        builder.AppendLine("## Implementation notes");
        builder.AppendLine();
        builder.AppendLine("- Apply tokens as CSS classes with the configured prefix.");
        builder.AppendLine("- Preserve WCAG contrast between foreground and background.");
        return builder.ToString();
    }

    public static NerdDesignTokenOptions ImportJson(string json, string prefix = "nerd")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        var colors = JsonSerializer.Deserialize<Dictionary<string, NerdColorToken>>(json)
            ?? throw new ArgumentException("The token JSON did not contain a color object.", nameof(json));
        var options = new NerdDesignTokenOptions { Prefix = prefix };
        foreach (var color in colors)
        {
            options.Add(color.Key, color.Value);
        }

        return options;
    }

    public static IReadOnlyList<NerdAccessibilityResult> CheckAccessibility(
        NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var wcagVersion = string.IsNullOrWhiteSpace(options.WcagVersion)
            ? DefaultWcagVersion
            : options.WcagVersion;

        return options.Colors
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair =>
            {
                var token = pair.Value;
                var lightBackground = token.Light ?? token.Value;
                var darkBackground = token.Dark ?? lightBackground;
                var lightForeground = token.ContrastText ?? NerdColorValue.ContrastText(lightBackground);
                var darkForeground = token.DarkContrastText ?? token.ContrastText ?? NerdColorValue.ContrastText(darkBackground);

                return new NerdAccessibilityResult(
                    pair.Key,
                    wcagVersion,
                    Evaluate("light", lightBackground, lightForeground, wcagVersion),
                    Evaluate("dark", darkBackground, darkForeground, wcagVersion));
            })
            .ToArray();
    }

    public static IReadOnlyList<NerdAccessibilityWarning> GetAccessibilityWarnings(
        NerdDesignTokenOptions options)
    {
        var warnings = new List<NerdAccessibilityWarning>();
        foreach (var result in CheckAccessibility(options))
        {
            AddWarnings(warnings, result.Name, result.WcagVersion, result.Light, "AA", AaNormalTextRatio);
            AddWarnings(warnings, result.Name, result.WcagVersion, result.Dark, "AA", AaNormalTextRatio);
        }

        return warnings;
    }

    public static void LogAccessibilityWarnings(
        NerdDesignTokenOptions options,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        foreach (var warning in GetAccessibilityWarnings(options))
        {
            logger.LogWarning(
                "Design token '{TokenName}' ({Mode}) has contrast {ContrastRatio:0.0}:1, below WCAG {WcagVersion} {Level} requirement of {RequiredRatio:0.0}:1. Recommended foreground: {RecommendedForeground}.",
                warning.TokenName,
                warning.Mode,
                warning.ContrastRatio,
                warning.WcagVersion,
                warning.Level,
                warning.RequiredRatio,
                warning.RecommendedForeground);
        }
    }

    private static void AddWarnings(
        ICollection<NerdAccessibilityWarning> warnings,
        string tokenName,
        string wcagVersion,
        NerdContrastEvaluation evaluation,
        string level,
        double requiredRatio)
    {
        if (evaluation.MeetsAa)
        {
            return;
        }

        warnings.Add(new NerdAccessibilityWarning(
            tokenName,
            evaluation.Mode,
            wcagVersion,
            evaluation.ContrastRatio,
            requiredRatio,
            level,
            evaluation.RecommendedForeground));
    }

    private static NerdContrastEvaluation Evaluate(
        string mode,
        string background,
        string foreground,
        string wcagVersion)
    {
        var ratio = NerdColorParser.ContrastRatio(background, foreground);
        var recommended = NerdColorValue.ContrastText(background);
        return new NerdContrastEvaluation(
            mode,
            background,
            foreground,
            ratio,
            ratio >= AaNormalTextRatio,
            ratio >= AaaNormalTextRatio,
            recommended);
    }
}
