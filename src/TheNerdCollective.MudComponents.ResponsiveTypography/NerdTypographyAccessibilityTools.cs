using MudBlazor;
using Microsoft.Extensions.Logging;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public sealed record NerdTypographyAccessibilityResult(
    string Role,
    string FontSize,
    string WcagVersion,
    double? MinimumPixels,
    double RequiredMinimumPixels,
    bool MeetsResizeGuidance,
    bool MeetsMinimumSize,
    bool MeetsLineHeightGuidance,
    bool MeetsSpacingGuidance);

public sealed record NerdTypographyAccessibilityWarning(
    string Role,
    string WcagVersion,
    string FontSize,
    double? MinimumPixels,
    double RequiredMinimumPixels,
    string Message);

public static partial class NerdTypographyAccessibilityTools
{
    public const string DefaultWcagVersion = WcagStandards.DefaultVersion;

    public static IReadOnlyList<NerdTypographyRole> GetConfiguredRoles(MudTheme theme) =>
    [
        new("Default", theme.Typography.Default.FontSize, theme.Typography.Default.LineHeight, theme.Typography.Default.LetterSpacing, Typo.body1, 16),
        new("H1", theme.Typography.H1.FontSize, theme.Typography.H1.LineHeight, theme.Typography.H1.LetterSpacing, Typo.h1, 24),
        new("H2", theme.Typography.H2.FontSize, theme.Typography.H2.LineHeight, theme.Typography.H2.LetterSpacing, Typo.h2, 21),
        new("H3", theme.Typography.H3.FontSize, theme.Typography.H3.LineHeight, theme.Typography.H3.LetterSpacing, Typo.h3, 18),
        new("H4", theme.Typography.H4.FontSize, theme.Typography.H4.LineHeight, theme.Typography.H4.LetterSpacing, Typo.h4, 16),
        new("H5", theme.Typography.H5.FontSize, theme.Typography.H5.LineHeight, theme.Typography.H5.LetterSpacing, Typo.h5, 14),
        new("H6", theme.Typography.H6.FontSize, theme.Typography.H6.LineHeight, theme.Typography.H6.LetterSpacing, Typo.h6, 14),
        new("Subtitle1", theme.Typography.Subtitle1.FontSize, theme.Typography.Subtitle1.LineHeight, theme.Typography.Subtitle1.LetterSpacing, Typo.subtitle1, 16),
        new("Subtitle2", theme.Typography.Subtitle2.FontSize, theme.Typography.Subtitle2.LineHeight, theme.Typography.Subtitle2.LetterSpacing, Typo.subtitle2, 14),
        new("Body1", theme.Typography.Body1.FontSize, theme.Typography.Body1.LineHeight, theme.Typography.Body1.LetterSpacing, Typo.body1, 16),
        new("Body2", theme.Typography.Body2.FontSize, theme.Typography.Body2.LineHeight, theme.Typography.Body2.LetterSpacing, Typo.body2, 14),
        new("Button", theme.Typography.Button.FontSize, theme.Typography.Button.LineHeight, theme.Typography.Button.LetterSpacing, Typo.button, 14),
        new("Caption", theme.Typography.Caption.FontSize, theme.Typography.Caption.LineHeight, theme.Typography.Caption.LetterSpacing, Typo.caption, 12),
        new("Overline", theme.Typography.Overline.FontSize, theme.Typography.Overline.LineHeight, theme.Typography.Overline.LetterSpacing, Typo.overline, 10)
    ];

    public static IReadOnlyList<NerdTypographyAccessibilityResult> CheckAccessibility(
        NerdResponsiveTypographyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var wcagVersion = string.IsNullOrWhiteSpace(options.WcagVersion)
            ? DefaultWcagVersion
            : options.WcagVersion;
        var theme = options.CreatePreviewTheme();

        return GetConfiguredRoles(theme)
            .Select(role => Evaluate(role, wcagVersion))
            .ToArray();
    }

    public static IReadOnlyList<NerdTypographyAccessibilityWarning> GetAccessibilityWarnings(
        NerdResponsiveTypographyOptions options)
    {
        var warnings = new List<NerdTypographyAccessibilityWarning>();
        foreach (var result in CheckAccessibility(options))
        {
            if (result.MeetsMinimumSize && result.MeetsResizeGuidance &&
                result.MeetsLineHeightGuidance && result.MeetsSpacingGuidance)
            {
                continue;
            }

            var message = !result.MeetsMinimumSize
                ? $"Minimum size {(result.MinimumPixels?.ToString("0.#") ?? "unknown")}px is below WCAG {result.WcagVersion} recommended {result.RequiredMinimumPixels:0.#}px."
                : !result.MeetsResizeGuidance
                    ? $"Font size '{result.FontSize}' should use relative units for WCAG {result.WcagVersion} resize guidance."
                    : !result.MeetsLineHeightGuidance
                        ? $"Line height should be at least {WcagStandards.MinimumLineHeightRatio:0.0} for readability."
                        : $"Letter spacing should be at least {WcagStandards.MinimumLetterSpacingEm:0.2}em.";

            warnings.Add(new NerdTypographyAccessibilityWarning(
                result.Role,
                result.WcagVersion,
                result.FontSize,
                result.MinimumPixels,
                result.RequiredMinimumPixels,
                message));
        }

        return warnings;
    }

    public static void LogAccessibilityWarnings(
        NerdResponsiveTypographyOptions options,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        foreach (var warning in GetAccessibilityWarnings(options))
        {
            logger.LogWarning(
                "Responsive typography role '{Role}' ({FontSize}) accessibility warning: {Message}",
                warning.Role,
                warning.FontSize,
                warning.Message);
        }
    }

    internal static bool UsesRelativeUnits(string fontSize)
    {
        var value = fontSize.Trim();
        return value.Contains("rem", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("em", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("vw", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("vh", StringComparison.OrdinalIgnoreCase) ||
               value.Contains('%') ||
               value.StartsWith("clamp(", StringComparison.OrdinalIgnoreCase);
    }

    private static NerdTypographyAccessibilityResult Evaluate(
        NerdTypographyRole role,
        string wcagVersion)
    {
        var minimum = NerdClampEvaluator.EvaluateAtViewport(role.FontSize, 320);
        var meetsResize = UsesRelativeUnits(role.FontSize);
        var meetsMinimum = minimum is null || minimum >= role.RequiredMinimumPixels;
        var meetsLineHeight = ParseLineHeight(role.LineHeight) is null ||
                              ParseLineHeight(role.LineHeight) >= WcagStandards.MinimumLineHeightRatio;
        var meetsSpacing = ParseEm(role.LetterSpacing) is null ||
                           ParseEm(role.LetterSpacing) >= WcagStandards.MinimumLetterSpacingEm;

        return new NerdTypographyAccessibilityResult(
            role.Role,
            role.FontSize,
            wcagVersion,
            minimum,
            role.RequiredMinimumPixels,
            meetsResize,
            meetsMinimum,
            meetsLineHeight,
            meetsSpacing);
    }

    private static double? ParseLineHeight(string? lineHeight)
    {
        if (string.IsNullOrWhiteSpace(lineHeight))
        {
            return null;
        }

        return double.TryParse(lineHeight, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }

    private static double? ParseEm(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.EndsWith("em", StringComparison.OrdinalIgnoreCase) &&
               double.TryParse(value[..^2], System.Globalization.NumberStyles.Float,
                   System.Globalization.CultureInfo.InvariantCulture, out var em)
            ? em
            : null;
    }
}

public sealed record NerdTypographyRole(
    string Role,
    string FontSize,
    string? LineHeight,
    string? LetterSpacing,
    Typo Typo,
    double RequiredMinimumPixels);
