namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Default breakpoint, motion, and z-index tokens (HR-098).</summary>
public static class NerdFoundationTaxonomyTools
{
    public static IReadOnlyDictionary<string, string> DefaultBreakpoints { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["xs"] = "0px",
            ["sm"] = "600px",
            ["md"] = "960px",
            ["lg"] = "1280px",
            ["xl"] = "1920px"
        };

    public static IReadOnlyDictionary<string, string> DefaultMotionDurations { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["instant"] = "0ms",
            ["fast"] = "150ms",
            ["normal"] = "250ms",
            ["slow"] = "400ms"
        };

    public static IReadOnlyDictionary<string, string> DefaultMotionEasings { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["standard"] = "cubic-bezier(0.4, 0, 0.2, 1)",
            ["emphasized"] = "cubic-bezier(0.2, 0, 0, 1)",
            ["decelerate"] = "cubic-bezier(0, 0, 0.2, 1)"
        };

    public static IReadOnlyDictionary<string, string> DefaultZIndex { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["dropdown"] = "1000",
            ["sticky"] = "1100",
            ["modal"] = "1300",
            ["tooltip"] = "1500"
        };

    public static IReadOnlyDictionary<string, string> DefaultRadii { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["sm"] = "4px",
            ["default"] = "8px",
            ["md"] = "8px",
            ["lg"] = "12px",
            ["card"] = "12px"
        };

    public static IReadOnlyDictionary<string, string> DefaultShadows { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["sm"] = "0 1px 3px rgba(0,0,0,.12)",
            ["md"] = "0 4px 12px rgba(0,0,0,.12)",
            ["lg"] = "0 8px 24px rgba(0,0,0,.16)",
            ["elevated"] = "0 4px 16px rgba(0,0,0,.16)"
        };

    public static void ApplyDefaults(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ApplyMap(options.Breakpoints, options.AddBreakpoint, DefaultBreakpoints);
        ApplyMap(options.MotionDurations, options.AddMotionDuration, DefaultMotionDurations);
        ApplyMap(options.MotionEasings, options.AddMotionEasing, DefaultMotionEasings);
        ApplyMap(options.ZIndex, options.AddZIndex, DefaultZIndex);
        ApplyMap(options.Radii, options.AddRadius, DefaultRadii);
        ApplyMap(options.Shadows, options.AddShadow, DefaultShadows);
    }

    private static void ApplyMap(
        IReadOnlyDictionary<string, string> existing,
        Func<string, string, NerdDesignTokenOptions> add,
        IReadOnlyDictionary<string, string> defaults)
    {
        foreach (var (name, value) in defaults)
        {
            if (!existing.ContainsKey(name))
            {
                add(name, value);
            }
        }
    }
}
