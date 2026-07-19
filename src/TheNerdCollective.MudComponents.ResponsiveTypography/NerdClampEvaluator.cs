using System.Globalization;
using System.Text.RegularExpressions;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public static partial class NerdClampEvaluator
{
    public static double? EvaluateAtViewport(string fontSize, double viewportWidthPx, double rootFontPixels = 16)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fontSize);
        var match = ClampRegex().Match(fontSize.Trim());
        if (!match.Success)
        {
            return ParseSize(fontSize.Trim(), viewportWidthPx, rootFontPixels);
        }

        var minimum = ParseSize(match.Groups[1].Value.Trim(), viewportWidthPx, rootFontPixels) ?? 0;
        var preferred = ParseSize(match.Groups[2].Value.Trim(), viewportWidthPx, rootFontPixels) ?? minimum;
        var maximum = ParseSize(match.Groups[3].Value.Trim(), viewportWidthPx, rootFontPixels) ?? preferred;
        return Math.Min(maximum, Math.Max(minimum, preferred));
    }

    public static bool TryParseForEditor(
        string fontSize,
        out double minimumPx,
        out double preferredVw,
        out double maximumPx,
        double rootFontPixels = 16)
    {
        minimumPx = 0;
        preferredVw = 0;
        maximumPx = 0;
        ArgumentException.ThrowIfNullOrWhiteSpace(fontSize);
        var match = ClampRegex().Match(fontSize.Trim());
        if (!match.Success)
        {
            return false;
        }

        var minimum = ParseSize(match.Groups[1].Value.Trim(), 1280, rootFontPixels);
        var preferred = ParseSize(match.Groups[2].Value.Trim(), 1280, rootFontPixels);
        var maximum = ParseSize(match.Groups[3].Value.Trim(), 1280, rootFontPixels);
        if (minimum is null || preferred is null || maximum is null)
        {
            return false;
        }

        minimumPx = minimum.Value;
        maximumPx = maximum.Value;
        preferredVw = ParsePreferredVw(match.Groups[2].Value.Trim(), preferred.Value, rootFontPixels);
        return true;
    }

    private static double ParsePreferredVw(string preferredValue, double preferredPx, double rootFontPixels)
    {
        if (preferredValue.EndsWith("vw", StringComparison.OrdinalIgnoreCase) &&
            double.TryParse(preferredValue[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var vw))
        {
            return vw;
        }

        return preferredPx / 12.8d;
    }

    private static double? ParseSize(string value, double viewportWidthPx, double rootFontPixels)
    {
        if (value.EndsWith("vw", StringComparison.OrdinalIgnoreCase) &&
            double.TryParse(value[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var vw))
        {
            return viewportWidthPx * vw / 100d;
        }

        if (value.EndsWith("rem", StringComparison.OrdinalIgnoreCase) &&
            double.TryParse(value[..^3], NumberStyles.Float, CultureInfo.InvariantCulture, out var rem))
        {
            return rem * rootFontPixels;
        }

        if (value.EndsWith("px", StringComparison.OrdinalIgnoreCase) &&
            double.TryParse(value[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var px))
        {
            return px;
        }

        return null;
    }

    [GeneratedRegex(@"clamp\(\s*([^,]+)\s*,\s*([^,]+)\s*,\s*([^)]+)\)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex ClampRegex();
}
