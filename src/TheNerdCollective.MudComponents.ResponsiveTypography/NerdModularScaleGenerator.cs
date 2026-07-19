using System.Globalization;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public static class NerdModularScaleGenerator
{
    private static readonly string[] Roles = ["H1", "H2", "H3", "H4", "Body1", "Body2", "Caption"];

    public static IReadOnlyDictionary<string, string> Generate(
        double baseRem = 1,
        double ratio = 1.25,
        bool wrapInClamp = true)
    {
        if (baseRem <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(baseRem));
        }
        if (ratio <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(ratio));
        }

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < Roles.Length; index++)
        {
            var exponent = Roles.Length - index - 3;
            var rem = baseRem * Math.Pow(ratio, exponent);
            var minimum = (rem * 0.8).ToString("0.###", CultureInfo.InvariantCulture);
            var preferred = (rem * 5).ToString("0.###", CultureInfo.InvariantCulture);
            var maximum = (rem * 1.2).ToString("0.###", CultureInfo.InvariantCulture);
            values[Roles[index]] = wrapInClamp
                ? ResponsiveFontSize.Clamp($"{minimum}rem", $"{preferred}vw", $"{maximum}rem")
                : $"{rem.ToString("0.###", CultureInfo.InvariantCulture)}rem";
        }

        return values;
    }
}
