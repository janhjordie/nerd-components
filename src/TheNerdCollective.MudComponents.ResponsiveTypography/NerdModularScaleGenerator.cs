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
            values[Roles[index]] = wrapInClamp
                ? ResponsiveFontSize.Clamp($"{rem * 0.8:0.###}rem", $"{rem * 5:0.###}vw", $"{rem * 1.2:0.###}rem")
                : $"{rem:0.###}rem";
        }

        return values;
    }
}
