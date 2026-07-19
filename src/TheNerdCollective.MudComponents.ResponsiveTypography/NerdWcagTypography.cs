using System.Globalization;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

/// <summary>Helpers for WCAG 2.1 typography compliance (1.4.4 resize, 1.4.12 text spacing).</summary>
public static class NerdWcagTypography
{
    public static string Clamp(string role, double minimumRem, double preferredVw, double maximumRem)
    {
        var floor = WcagStandards.GetTypographyRoleMinimumRem(role);
        return Clamp(Math.Max(minimumRem, floor), preferredVw, Math.Max(maximumRem, floor));
    }

    public static string Clamp(double minimumRem, double preferredVw, double maximumRem) =>
        ResponsiveFontSize.Clamp(
            ToRem(minimumRem),
            $"{preferredVw.ToString("0.##", CultureInfo.InvariantCulture)}vw",
            ToRem(maximumRem));

    public static string ToRem(double rem) =>
        $"{rem.ToString("0.####", CultureInfo.InvariantCulture)}rem";

    public static void EnsureCompliance(ResponsiveTypographyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Default ??= ToRem(WcagStandards.GetTypographyRoleMinimumRem(nameof(ResponsiveTypographyOptions.Default)));
        options.LineHeight = NormalizeLineHeight(options.LineHeight);
        options.LetterSpacing = NormalizeLetterSpacing(options.LetterSpacing);

        foreach (var role in ResponsiveTypographyRoleStyles.RoleNames)
        {
            if (options.Roles.TryGet(role, out var style))
            {
                style.LineHeight = NormalizeLineHeight(style.LineHeight);
                style.LetterSpacing = NormalizeLetterSpacing(style.LetterSpacing);
            }
        }

        foreach (var role in options.ConfiguredRoles.ToArray())
        {
            var property = typeof(ResponsiveTypographyOptions).GetProperty(role);
            if (property?.GetValue(options) is not string fontSize)
            {
                continue;
            }

            property.SetValue(options, NormalizeFontSize(role, fontSize));
        }
    }

    public static string NormalizeFontSize(string role, string fontSize)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);
        ArgumentException.ThrowIfNullOrWhiteSpace(fontSize);
        var floorRem = WcagStandards.GetTypographyRoleMinimumRem(role);
        var trimmed = fontSize.Trim();

        if (trimmed.StartsWith("clamp(", StringComparison.OrdinalIgnoreCase) &&
            NerdClampEvaluator.TryParseForEditor(trimmed, out var minimumPx, out var preferredVw, out var maximumPx))
        {
            var minimumRem = Math.Max(minimumPx / 16d, floorRem);
            var maximumRem = Math.Max(maximumPx / 16d, minimumRem);
            return Clamp(minimumRem, preferredVw, maximumRem);
        }

        if (trimmed.EndsWith("rem", StringComparison.OrdinalIgnoreCase) &&
            double.TryParse(trimmed[..^3], NumberStyles.Float, CultureInfo.InvariantCulture, out var rem))
        {
            return ToRem(Math.Max(rem, floorRem));
        }

        if (trimmed.EndsWith("px", StringComparison.OrdinalIgnoreCase) &&
            double.TryParse(trimmed[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var px))
        {
            return ToRem(Math.Max(px / 16d, floorRem));
        }

        return trimmed;
    }

    private static string NormalizeLineHeight(string? lineHeight)
    {
        if (string.IsNullOrWhiteSpace(lineHeight))
        {
            return WcagStandards.MinimumLineHeightRatio.ToString("0.0", CultureInfo.InvariantCulture);
        }

        return double.TryParse(lineHeight, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) &&
               value < WcagStandards.MinimumLineHeightRatio
            ? WcagStandards.MinimumLineHeightRatio.ToString("0.0", CultureInfo.InvariantCulture)
            : lineHeight;
    }

    private static string NormalizeLetterSpacing(string? letterSpacing)
    {
        if (string.IsNullOrWhiteSpace(letterSpacing))
        {
            return ToLetterSpacingEm(WcagStandards.MinimumLetterSpacingEm);
        }

        if (letterSpacing.EndsWith("em", StringComparison.OrdinalIgnoreCase) &&
            double.TryParse(letterSpacing[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var em) &&
            em < WcagStandards.MinimumLetterSpacingEm)
        {
            return ToLetterSpacingEm(WcagStandards.MinimumLetterSpacingEm);
        }

        return letterSpacing;
    }

    private static string ToLetterSpacingEm(double em) =>
        $"{em.ToString("0.##", CultureInfo.InvariantCulture)}em";
}
