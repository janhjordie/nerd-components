using System.Globalization;

namespace TheNerdCollective.MudComponents.DesignTokens;

internal static class NerdColorValue
{
    public static string Validate(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Contains(';') || value.Contains('{') || value.Contains('}') ||
            value.Contains('<') || value.Contains('>'))
        {
            throw new ArgumentException("Color values must be valid CSS values without declarations or markup.", parameterName);
        }

        return value.Trim();
    }

    public static string ContrastText(string value)
    {
        if (!value.StartsWith('#') ||
            !int.TryParse(value[1..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb))
        {
            return "#FFFFFF";
        }

        if (value.Length == 4)
        {
            var r = ((rgb >> 8) & 0xF) * 17;
            var g = ((rgb >> 4) & 0xF) * 17;
            var b = (rgb & 0xF) * 17;
            return RelativeLuminance(r, g, b) > 0.179 ? "#1F2937" : "#FFFFFF";
        }

        var red = (rgb >> 16) & 0xFF;
        var green = (rgb >> 8) & 0xFF;
        var blue = rgb & 0xFF;
        return RelativeLuminance(red, green, blue) > 0.179 ? "#1F2937" : "#FFFFFF";
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
}
