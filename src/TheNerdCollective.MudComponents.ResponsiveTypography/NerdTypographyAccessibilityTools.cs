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
        CreateRole("Default", theme.Typography.Default, Typo.body1),
        CreateRole("H1", theme.Typography.H1, Typo.h1),
        CreateRole("H2", theme.Typography.H2, Typo.h2),
        CreateRole("H3", theme.Typography.H3, Typo.h3),
        CreateRole("H4", theme.Typography.H4, Typo.h4),
        CreateRole("H5", theme.Typography.H5, Typo.h5),
        CreateRole("H6", theme.Typography.H6, Typo.h6),
        CreateRole("Subtitle1", theme.Typography.Subtitle1, Typo.subtitle1),
        CreateRole("Subtitle2", theme.Typography.Subtitle2, Typo.subtitle2),
        CreateRole("Body1", theme.Typography.Body1, Typo.body1),
        CreateRole("Body2", theme.Typography.Body2, Typo.body2),
        CreateRole("Button", theme.Typography.Button, Typo.button),
        CreateRole("Caption", theme.Typography.Caption, Typo.caption),
        CreateRole("Overline", theme.Typography.Overline, Typo.overline)
    ];

    private static NerdTypographyRole CreateRole(string role, BaseTypography typography, Typo typo) =>
        new(
            role,
            FontSizeOrDefault(typography.FontSize),
            typography.LineHeight,
            typography.LetterSpacing,
            typo,
            WcagStandards.GetTypographyRoleMinimumPixels(role));

    public static IReadOnlyList<NerdTypographyAccessibilityResult> CheckAccessibility(
        NerdResponsiveTypographyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var wcagVersion = string.IsNullOrWhiteSpace(options.WcagVersion)
            ? DefaultWcagVersion
            : options.WcagVersion;
        var theme = options.CreatePreviewTheme();
        var configuredRoles = options.Typography.ConfiguredRoles;

        return GetConfiguredRoles(theme)
            .Where(role => configuredRoles.Contains(role.Role))
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
                ? $"Minimum size {(result.MinimumPixels?.ToString("0.#") ?? "unknown")}px is below WCAG {result.WcagVersion} storyboard floor ({WcagStandards.GetTypographyRoleMinimumLabel(result.Role)})."
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

    private static string FontSizeOrDefault(string? fontSize) =>
        string.IsNullOrWhiteSpace(fontSize) ? "1rem" : fontSize;

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
