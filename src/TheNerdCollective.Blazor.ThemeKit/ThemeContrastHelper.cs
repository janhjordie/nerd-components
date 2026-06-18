using MudBlazor.Utilities;

namespace TheNerdCollective.Blazor.ThemeKit;

public static class ThemeContrastHelper
{
    public static double GetContrastRatio(MudColor foreground, MudColor background)
    {
        var foregroundLuminance = GetRelativeLuminance(foreground);
        var backgroundLuminance = GetRelativeLuminance(background);
        var lighter = Math.Max(foregroundLuminance, backgroundLuminance);
        var darker = Math.Min(foregroundLuminance, backgroundLuminance);
        return (lighter + 0.05) / (darker + 0.05);
    }

    public static string GetWcagLabel(double ratio) =>
        ratio >= 7 ? "AAA" : ratio >= 4.5 ? "AA" : ratio >= 3 ? "AA Large" : "Low";

    private static double GetRelativeLuminance(MudColor color)
    {
        static double Convert(byte channel)
        {
            var value = channel / 255.0;
            return value <= 0.03928 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
        }

        return 0.2126 * Convert(color.R)
             + 0.7152 * Convert(color.G)
             + 0.0722 * Convert(color.B);
    }
}
