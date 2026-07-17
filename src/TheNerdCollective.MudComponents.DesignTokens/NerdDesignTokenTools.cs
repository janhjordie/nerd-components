using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace TheNerdCollective.MudComponents.DesignTokens;

public static class NerdDesignTokenTools
{
    public const string DefaultWcagVersion = "2.1";
    public const double AaNormalTextRatio = 4.5;
    public const double AaaNormalTextRatio = 7;

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
                var darkForeground = token.ContrastText ?? NerdColorValue.ContrastText(darkBackground);

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
        var ratio = ContrastRatio(background, foreground);
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

    internal static double ContrastRatio(string background, string foreground)
    {
        var first = Luminance(background);
        var second = Luminance(foreground);
        return (Math.Max(first, second) + 0.05) / (Math.Min(first, second) + 0.05);
    }

    private static double Luminance(string value)
    {
        if (!value.StartsWith('#') ||
            !int.TryParse(value[1..], System.Globalization.NumberStyles.HexNumber,
                System.Globalization.CultureInfo.InvariantCulture, out var rgb))
        {
            return 0;
        }

        if (value.Length == 4)
        {
            var expanded = ((rgb >> 8) & 0xF) * 17;
            var green = ((rgb >> 4) & 0xF) * 17;
            var blue = (rgb & 0xF) * 17;
            return RelativeLuminance(expanded, green, blue);
        }

        return RelativeLuminance((rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
    }

    private static double RelativeLuminance(int red, int green, int blue)
    {
        static double Channel(int channel)
        {
            var normalized = channel / 255d;
            return normalized <= 0.03928
                ? normalized / 12.92
                : Math.Pow((normalized + 0.055) / 1.055, 2.4);
        }

        return 0.2126 * Channel(red) + 0.7152 * Channel(green) + 0.0722 * Channel(blue);
    }
}
