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

    public static string ExportJson(NerdDesignTokenOptions options) =>
        ExportPackJson(options);

    public static string ExportPackJson(NerdDesignTokenOptions options, string clientId = "export") =>
        NerdTokenPack.FromOptions(options, clientId).ToJson();

    public static string ExportColorsJson(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return JsonSerializer.Serialize(options.Colors);
    }

    public static string ExportStitchDesignMd(
        NerdDesignTokenOptions options,
        IReadOnlyDictionary<string, string>? typographyRoles = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        var builder = new System.Text.StringBuilder("# Design tokens\n\n");
        builder.AppendLine("## Colors");
        builder.AppendLine();
        foreach (var pair in options.Colors.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            builder.AppendLine($"- **{pair.Key}**: `{pair.Value.Value}`");
        }

        if (options.Aliases.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("## Aliases");
            builder.AppendLine();
            foreach (var pair in options.Aliases.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                builder.AppendLine($"- **{pair.Key}** → `{pair.Value}`");
            }
        }

        if (options.Recipes.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("## Recipes");
            builder.AppendLine();
            foreach (var pair in options.Recipes.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                builder.AppendLine(
                    $"- **{pair.Key}**: surface `{pair.Value.Surface}`, content `{pair.Value.Content}`, action `{pair.Value.Action ?? pair.Value.Surface}`");
            }
        }

        if (options.Radii.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("## Radii");
            builder.AppendLine();
            foreach (var pair in options.Radii.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                builder.AppendLine($"- **{pair.Key}**: `{pair.Value}`");
            }
        }

        if (options.Shadows.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("## Shadows");
            builder.AppendLine();
            foreach (var pair in options.Shadows.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                builder.AppendLine($"- **{pair.Key}**: `{pair.Value}`");
            }
        }

        if (typographyRoles is { Count: > 0 })
        {
            builder.AppendLine();
            builder.AppendLine("## Typography");
            builder.AppendLine();
            foreach (var pair in typographyRoles.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                builder.AppendLine($"- **{pair.Key}**: `{pair.Value}`");
            }
        }

        builder.AppendLine();
        builder.AppendLine("## Implementation notes");
        builder.AppendLine();
        builder.AppendLine("- Apply tokens as CSS classes with the configured prefix.");
        builder.AppendLine("- Preserve WCAG contrast between foreground and background.");
        return builder.ToString();
    }

    public static string ExportTokensStudioJson(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var colorTokens = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in options.Colors.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            colorTokens[pair.Key] = new Dictionary<string, string>
            {
                ["value"] = pair.Value.Value,
                ["type"] = "color"
            };
        }

        var payload = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["$schema"] = "https://tokens.studio/schemas/1.0.0",
            ["prefix"] = options.Prefix,
            ["color"] = colorTokens
        };

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
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
        var variables = NerdDesignTokenColorVariables.Build(options);

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
                    Evaluate("light", lightBackground, lightForeground, wcagVersion, variables),
                    Evaluate("dark", darkBackground, darkForeground, wcagVersion, variables));
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

    public static void AssertAccessibilityCompliance(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var warnings = GetAccessibilityWarnings(options);
        if (warnings.Count == 0)
        {
            return;
        }

        var summary = string.Join(
            Environment.NewLine,
            warnings.Select(warning =>
                $"{warning.TokenName} ({warning.Mode}): {warning.ContrastRatio:0.0}:1 < {warning.RequiredRatio:0.0}:1"));
        throw new InvalidOperationException(
            $"Design token accessibility gate failed with {warnings.Count} WCAG violation(s):{Environment.NewLine}{summary}");
    }

    public static IReadOnlyList<NerdContrastPairResult> BuildContrastMatrix(
        NerdDesignTokenOptions options,
        bool dark = false)
    {
        ArgumentNullException.ThrowIfNull(options);
        var variables = NerdDesignTokenColorVariables.Build(options);
        var tokenNames = options.Colors.Keys.OrderBy(name => name, StringComparer.Ordinal).ToArray();
        var results = new List<NerdContrastPairResult>(tokenNames.Length * tokenNames.Length);

        foreach (var foregroundName in tokenNames)
        {
            var foregroundToken = options.Colors[foregroundName];
            var foregroundColor = dark
                ? foregroundToken.Dark ?? foregroundToken.Light ?? foregroundToken.Value
                : foregroundToken.Light ?? foregroundToken.Value;

            foreach (var backgroundName in tokenNames)
            {
                var backgroundToken = options.Colors[backgroundName];
                var backgroundColor = dark
                    ? backgroundToken.Dark ?? backgroundToken.Light ?? backgroundToken.Value
                    : backgroundToken.Light ?? backgroundToken.Value;
                var ratio = NerdColorParser.ContrastRatio(backgroundColor, foregroundColor, variables);
                results.Add(new NerdContrastPairResult(
                    foregroundName,
                    backgroundName,
                    foregroundColor,
                    backgroundColor,
                    ratio,
                    ratio >= AaNormalTextRatio,
                    ratio >= AaaNormalTextRatio));
            }
        }

        return results;
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
        string wcagVersion,
        IReadOnlyDictionary<string, string> variables)
    {
        var ratio = NerdColorParser.ContrastRatio(background, foreground, variables);
        var recommended = NerdColorParser.ContrastText(background, variables);
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
