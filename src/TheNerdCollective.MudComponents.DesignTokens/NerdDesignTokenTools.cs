using System.Text.Json;

namespace TheNerdCollective.MudComponents.DesignTokens;

public sealed record NerdAccessibilityResult(string Name, double ContrastRatio, bool MeetsAa);

public static class NerdDesignTokenTools
{
    public static void WriteCss(NerdDesignTokenOptions options, string path)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        File.WriteAllText(path, MudBlazorDesignTokenCssGenerator.Generate(options));
    }

    public static string ExportJson(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return JsonSerializer.Serialize(options.Colors);
    }

    public static NerdDesignTokenOptions ImportJson(string json, string prefix = "nerd")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        var colors = JsonSerializer.Deserialize<Dictionary<string, NerdColorToken>>(json)
            ?? throw new ArgumentException("The token JSON did not contain a color object.", nameof(json));
        var options = new NerdDesignTokenOptions { Prefix = prefix };
        foreach (var color in colors)
        {
            options.Add(color.Key, color.Value);
        }

        return options;
    }

    public static IReadOnlyList<NerdAccessibilityResult> CheckAccessibility(
        NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.Colors
            .Select(pair =>
            {
                var text = pair.Value.ContrastText ?? NerdColorValue.ContrastText(pair.Value.Value);
                var ratio = ContrastRatio(pair.Value.Value, text);
                return new NerdAccessibilityResult(pair.Key, ratio, ratio >= 4.5);
            })
            .ToArray();
    }

    private static double ContrastRatio(string background, string foreground)
    {
        static double Luminance(string value)
        {
            if (!value.StartsWith('#') ||
                !int.TryParse(value[1..], System.Globalization.NumberStyles.HexNumber,
                    System.Globalization.CultureInfo.InvariantCulture, out var rgb))
            {
                return 0;
            }

            double Channel(int channel)
            {
                var normalized = channel / 255d;
                return normalized <= 0.03928
                    ? normalized / 12.92
                    : Math.Pow((normalized + 0.055) / 1.055, 2.4);
            }

            return 0.2126 * Channel((rgb >> 16) & 255)
                 + 0.7152 * Channel((rgb >> 8) & 255)
                 + 0.0722 * Channel(rgb & 255);
        }

        var first = Luminance(background);
        var second = Luminance(foreground);
        return (Math.Max(first, second) + 0.05) / (Math.Min(first, second) + 0.05);
    }
}
