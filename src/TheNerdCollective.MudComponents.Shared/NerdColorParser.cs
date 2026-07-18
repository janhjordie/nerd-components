using System.Globalization;
using System.Text.RegularExpressions;

namespace TheNerdCollective.MudComponents.Shared;

/// <summary>
/// Parses CSS color values and calculates WCAG contrast ratios.
/// </summary>
public static partial class NerdColorParser
{
    private const int MaxVariableDepth = 8;

    public static bool TryGetRgb(string color, out int red, out int green, out int blue) =>
        TryGetRgb(color, variables: null, out red, out green, out blue);

    public static bool TryGetRgb(
        string color,
        IReadOnlyDictionary<string, string>? variables,
        out int red,
        out int green,
        out int blue) =>
        TryResolve(color, variables, out red, out green, out blue, depth: 0);

    public static string ContrastText(string color) => ContrastText(color, variables: null);

    public static string ContrastText(string color, IReadOnlyDictionary<string, string>? variables)
    {
        if (!TryGetRgb(color, variables, out var red, out var green, out var blue))
        {
            return "#FFFFFF";
        }

        return RelativeLuminance(red, green, blue) > 0.179 ? "#1F2937" : "#FFFFFF";
    }

    public static bool IsLight(string color, IReadOnlyDictionary<string, string>? variables = null)
    {
        if (!TryGetRgb(color, variables, out var red, out var green, out var blue))
        {
            return false;
        }

        return RelativeLuminance(red, green, blue) > 0.179;
    }

    public static string ContentText(string brandColor, string contrastText, IReadOnlyDictionary<string, string>? variables = null) =>
        IsLight(brandColor, variables) ? contrastText : brandColor;

    public static double ContrastRatio(string background, string foreground) =>
        ContrastRatio(background, foreground, variables: null);

    public static double ContrastRatio(
        string background,
        string foreground,
        IReadOnlyDictionary<string, string>? variables)
    {
        if (!TryGetRgb(background, variables, out var bgR, out var bgG, out var bgB) ||
            !TryGetRgb(foreground, variables, out var fgR, out var fgG, out var fgB))
        {
            return 0;
        }

        var first = RelativeLuminance(bgR, bgG, bgB);
        var second = RelativeLuminance(fgR, fgG, fgB);
        return (Math.Max(first, second) + 0.05) / (Math.Min(first, second) + 0.05);
    }

    private static bool TryResolve(
        string color,
        IReadOnlyDictionary<string, string>? variables,
        out int red,
        out int green,
        out int blue,
        int depth)
    {
        red = green = blue = 0;
        if (string.IsNullOrWhiteSpace(color) || depth > MaxVariableDepth)
        {
            return false;
        }

        var trimmed = color.Trim();
        if (trimmed.StartsWith("var(", StringComparison.OrdinalIgnoreCase))
        {
            var variableName = trimmed[4..].TrimEnd(')').Trim();
            if (variables is null || !variables.TryGetValue(variableName, out var resolved))
            {
                return false;
            }

            return TryResolve(resolved, variables, out red, out green, out blue, depth + 1);
        }

        if (trimmed.StartsWith('#'))
        {
            return TryParseHex(trimmed, out red, out green, out blue);
        }

        var rgbMatch = RgbRegex().Match(trimmed);
        if (rgbMatch.Success)
        {
            red = int.Parse(rgbMatch.Groups[1].Value, CultureInfo.InvariantCulture);
            green = int.Parse(rgbMatch.Groups[2].Value, CultureInfo.InvariantCulture);
            blue = int.Parse(rgbMatch.Groups[3].Value, CultureInfo.InvariantCulture);
            return true;
        }

        var hslMatch = HslRegex().Match(trimmed);
        if (hslMatch.Success &&
            TryHslToRgb(
                ParseChannel(hslMatch.Groups[1].Value),
                ParseChannel(hslMatch.Groups[2].Value),
                ParseChannel(hslMatch.Groups[3].Value),
                out red,
                out green,
                out blue))
        {
            return true;
        }

        return false;
    }

    private static bool TryParseHex(string value, out int red, out int green, out int blue)
    {
        red = green = blue = 0;
        if (!int.TryParse(value[1..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb))
        {
            return false;
        }

        if (value.Length == 4)
        {
            red = ((rgb >> 8) & 0xF) * 17;
            green = ((rgb >> 4) & 0xF) * 17;
            blue = (rgb & 0xF) * 17;
            return true;
        }

        red = (rgb >> 16) & 0xFF;
        green = (rgb >> 8) & 0xFF;
        blue = rgb & 0xFF;
        return true;
    }

    private static double ParseChannel(string value)
    {
        if (value.EndsWith('%'))
        {
            return double.Parse(value[..^1], CultureInfo.InvariantCulture) / 100d;
        }

        if (value.EndsWith("deg", StringComparison.OrdinalIgnoreCase))
        {
            return double.Parse(value[..^3], CultureInfo.InvariantCulture);
        }

        return double.Parse(value, CultureInfo.InvariantCulture);
    }

    private static bool TryHslToRgb(double hue, double saturation, double lightness, out int red, out int green, out int blue)
    {
        red = green = blue = 0;
        var h = (hue % 360 + 360) % 360 / 360d;

        if (saturation <= 0)
        {
            var gray = Clamp((int)Math.Round(lightness * 255));
            red = green = blue = gray;
            return true;
        }

        static double HueToRgb(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1d / 6) return p + (q - p) * 6 * t;
            if (t < 1d / 2) return q;
            if (t < 2d / 3) return p + (q - p) * (2d / 3 - t) * 6;
            return p;
        }

        var q = lightness < 0.5
            ? lightness * (1 + saturation)
            : lightness + saturation - lightness * saturation;
        var p = 2 * lightness - q;
        red = Clamp((int)Math.Round(HueToRgb(p, q, h + 1d / 3) * 255));
        green = Clamp((int)Math.Round(HueToRgb(p, q, h) * 255));
        blue = Clamp((int)Math.Round(HueToRgb(p, q, h - 1d / 3) * 255));
        return true;
    }

    private static int Clamp(int value) => Math.Clamp(value, 0, 255);

    private static double RelativeLuminance(int red, int green, int blue)
    {
        static double Channel(int value)
        {
            var normalized = value / 255d;
            return normalized <= 0.03928
                ? normalized / 12.92
                : Math.Pow((normalized + 0.055) / 1.055, 2.4);
        }

        return 0.2126 * Channel(red) + 0.7152 * Channel(green) + 0.0722 * Channel(blue);
    }

    [GeneratedRegex(@"rgba?\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex RgbRegex();

    [GeneratedRegex(@"hsla?\(\s*([^\s,]+)\s*,\s*([^\s,]+)\s*,\s*([^\s,)]+)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex HslRegex();
}
