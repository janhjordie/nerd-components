using System.Globalization;
using System.Text.RegularExpressions;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public sealed record NerdTypographyAccessibilityResult(
    string Role,
    string FontSize,
    string WcagVersion,
    double? MinimumPixels,
    double RequiredMinimumPixels,
    bool MeetsResizeGuidance,
    bool MeetsMinimumSize);

public sealed record NerdTypographyAccessibilityWarning(
    string Role,
    string WcagVersion,
    string FontSize,
    double? MinimumPixels,
    double RequiredMinimumPixels,
    string Message);

public static partial class NerdTypographyAccessibilityTools
{
    public const string DefaultWcagVersion = "2.1";
  private const double RootFontPixels = 16;

    public static IReadOnlyList<NerdTypographyRole> GetConfiguredRoles(MudBlazor.MudTheme theme) =>
    [
        new("Default", theme.Typography.Default.FontSize, MudBlazor.Typo.body1, 16),
        new("H1", theme.Typography.H1.FontSize, MudBlazor.Typo.h1, 24),
        new("H2", theme.Typography.H2.FontSize, MudBlazor.Typo.h2, 21),
        new("H3", theme.Typography.H3.FontSize, MudBlazor.Typo.h3, 18),
        new("H4", theme.Typography.H4.FontSize, MudBlazor.Typo.h4, 16),
        new("H5", theme.Typography.H5.FontSize, MudBlazor.Typo.h5, 14),
        new("H6", theme.Typography.H6.FontSize, MudBlazor.Typo.h6, 14),
        new("Subtitle1", theme.Typography.Subtitle1.FontSize, MudBlazor.Typo.subtitle1, 16),
        new("Subtitle2", theme.Typography.Subtitle2.FontSize, MudBlazor.Typo.subtitle2, 14),
        new("Body1", theme.Typography.Body1.FontSize, MudBlazor.Typo.body1, 16),
        new("Body2", theme.Typography.Body2.FontSize, MudBlazor.Typo.body2, 14),
        new("Button", theme.Typography.Button.FontSize, MudBlazor.Typo.button, 14),
        new("Caption", theme.Typography.Caption.FontSize, MudBlazor.Typo.caption, 12),
        new("Overline", theme.Typography.Overline.FontSize, MudBlazor.Typo.overline, 10)
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
            if (result.MeetsMinimumSize && result.MeetsResizeGuidance)
            {
                continue;
            }

            var message = !result.MeetsMinimumSize
                ? $"Minimum size {(result.MinimumPixels?.ToString("0.#") ?? "unknown")}px is below WCAG {result.WcagVersion} recommended {result.RequiredMinimumPixels:0.#}px."
                : $"Font size '{result.FontSize}' should use relative units for WCAG {result.WcagVersion} resize guidance.";

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
        Microsoft.Extensions.Logging.ILogger logger)
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

    internal static double? ParseMinimumPixels(string fontSize, double rootPixels = RootFontPixels)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fontSize);
        var trimmed = fontSize.Trim();

        var clampMatch = ClampRegex().Match(trimmed);
        if (clampMatch.Success)
        {
            return ParseSize(clampMatch.Groups[1].Value.Trim(), rootPixels);
        }

        return ParseSize(trimmed, rootPixels);
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
        var minimum = ParseMinimumPixels(role.FontSize);
        var meetsResize = UsesRelativeUnits(role.FontSize);
        var meetsMinimum = minimum is null || minimum >= role.RequiredMinimumPixels;

        return new NerdTypographyAccessibilityResult(
            role.Role,
            role.FontSize,
            wcagVersion,
            minimum,
            role.RequiredMinimumPixels,
            meetsResize,
            meetsMinimum);
    }

    private static double? ParseSize(string value, double rootPixels)
    {
        if (value.EndsWith("rem", StringComparison.OrdinalIgnoreCase) &&
            double.TryParse(value[..^3], NumberStyles.Float, CultureInfo.InvariantCulture, out var rem))
        {
            return rem * rootPixels;
        }

        if (value.EndsWith("px", StringComparison.OrdinalIgnoreCase) &&
            double.TryParse(value[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var px))
        {
            return px;
        }

        return null;
    }

    [GeneratedRegex(@"clamp\(\s*([^,]+)\s*,", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex ClampRegex();
}

public sealed record NerdTypographyRole(
    string Role,
    string FontSize,
    MudBlazor.Typo Typo,
    double RequiredMinimumPixels);
