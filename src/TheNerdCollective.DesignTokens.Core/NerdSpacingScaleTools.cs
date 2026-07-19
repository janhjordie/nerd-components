namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Default spacing scale helpers (HR-098 / HR-100).</summary>
public static class NerdSpacingScaleTools
{
    public static IReadOnlyDictionary<string, string> DefaultScale { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["0"] = "0px",
            ["1"] = "4px",
            ["2"] = "8px",
            ["3"] = "12px",
            ["4"] = "16px",
            ["5"] = "20px",
            ["6"] = "24px",
            ["8"] = "32px",
            ["10"] = "40px",
            ["12"] = "48px",
            ["16"] = "64px"
        };

    public static IReadOnlyList<string> DefaultStepNames { get; } = DefaultScale.Keys.ToList();

    public static void ApplyDefaultScale(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ApplyScale(options, DefaultScale, overwrite: false);
    }

    public static IReadOnlyDictionary<string, string> GenerateScale(
        int baseUnitPx,
        double ratio = 1d,
        NerdSpacingScaleCurve curve = NerdSpacingScaleCurve.Linear,
        IReadOnlyList<string>? stepNames = null)
    {
        if (baseUnitPx <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(baseUnitPx), "Base unit must be greater than zero.");
        }

        if (ratio <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ratio), "Ratio must be greater than zero.");
        }

        stepNames ??= DefaultStepNames;
        var scale = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in stepNames)
        {
            if (string.Equals(name, "0", StringComparison.OrdinalIgnoreCase))
            {
                scale[name] = "0px";
                continue;
            }

            if (!int.TryParse(name, out var step) || step < 0)
            {
                throw new ArgumentException($"Spacing step '{name}' must be a non-negative integer.", nameof(stepNames));
            }

            var pixels = curve switch
            {
                NerdSpacingScaleCurve.Linear => baseUnitPx * step,
                NerdSpacingScaleCurve.Geometric when step == 0 => 0,
                NerdSpacingScaleCurve.Geometric => (int)Math.Round(baseUnitPx * Math.Pow(ratio, step - 1)),
                _ => baseUnitPx * step
            };

            scale[name] = $"{pixels}px";
        }

        return scale;
    }

    public static void ApplyGeneratedScale(
        NerdDesignTokenOptions options,
        int baseUnitPx,
        double ratio = 1d,
        NerdSpacingScaleCurve curve = NerdSpacingScaleCurve.Linear,
        bool overwrite = true)
    {
        ArgumentNullException.ThrowIfNull(options);
        ApplyScale(options, GenerateScale(baseUnitPx, ratio, curve), overwrite);
    }

    private static void ApplyScale(
        NerdDesignTokenOptions options,
        IReadOnlyDictionary<string, string> scale,
        bool overwrite)
    {
        foreach (var (name, value) in scale)
        {
            if (overwrite || !options.Spacing.ContainsKey(name))
            {
                options.AddSpacing(name, value);
            }
        }
    }
}

public enum NerdSpacingScaleCurve
{
    Linear,
    Geometric
}
