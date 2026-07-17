namespace TheNerdCollective.MudComponents.Shared;

/// <summary>
/// Derives lighten, darken, and RGB tuple strings from supported CSS colors.
/// </summary>
public static class NerdColorDerivatives
{
    public static string ToRgbString(string color)
    {
        if (!NerdColorParser.TryGetRgb(color, out var red, out var green, out var blue))
        {
            return "0, 0, 0";
        }

        return $"{red}, {green}, {blue}";
    }

    public static string Darken(string color, double amount = 0.12)
    {
        if (!NerdColorParser.TryGetRgb(color, out var red, out var green, out var blue))
        {
            return color;
        }

        return ToHex(Scale(red, 1 - amount), Scale(green, 1 - amount), Scale(blue, 1 - amount));
    }

    public static string Lighten(string color, double amount = 0.12)
    {
        if (!NerdColorParser.TryGetRgb(color, out var red, out var green, out var blue))
        {
            return color;
        }

        return ToHex(
            ScaleToward(red, 255, amount),
            ScaleToward(green, 255, amount),
            ScaleToward(blue, 255, amount));
    }

    private static int Scale(int channel, double factor) => Math.Clamp((int)Math.Round(channel * factor), 0, 255);

    private static int ScaleToward(int channel, int target, double amount) =>
        Math.Clamp((int)Math.Round(channel + (target - channel) * amount), 0, 255);

    private static string ToHex(int red, int green, int blue) =>
        $"#{red:X2}{green:X2}{blue:X2}";
}
