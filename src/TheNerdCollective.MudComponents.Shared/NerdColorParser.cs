using System.Globalization;
using System.Text.RegularExpressions;

namespace TheNerdCollective.MudComponents.Shared;

public static partial class NerdColorParser
{
    public static bool TryGetRgb(string color, out int red, out int green, out int blue)
    {
        red = green = blue = 0;
        if (string.IsNullOrWhiteSpace(color))
        {
            return false;
        }

        var trimmed = color.Trim();
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

        return false;
    }

    public static string ContrastText(string color)
    {
        if (!TryGetRgb(color, out var red, out var green, out var blue))
        {
            return "#FFFFFF";
        }

        return RelativeLuminance(red, green, blue) > 0.179 ? "#1F2937" : "#FFFFFF";
    }

    public static double ContrastRatio(string background, string foreground)
    {
        if (!TryGetRgb(background, out var bgR, out var bgG, out var bgB) ||
            !TryGetRgb(foreground, out var fgR, out var fgG, out var fgB))
        {
            return 0;
        }

        var first = RelativeLuminance(bgR, bgG, bgB);
        var second = RelativeLuminance(fgR, fgG, fgB);
        return (Math.Max(first, second) + 0.05) / (Math.Min(first, second) + 0.05);
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
}
