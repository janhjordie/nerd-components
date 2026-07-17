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
