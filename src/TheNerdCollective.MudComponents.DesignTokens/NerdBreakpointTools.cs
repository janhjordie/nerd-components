using System.Globalization;
using MudBlazor;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Maps design-token breakpoints to MudBlazor <see cref="Breakpoint"/> and typography columns (HR-101).</summary>
public static class NerdBreakpointTools
{
    private static readonly double[] FallbackComparisonColumns = [320, 375, 768, 1024, 1280, 1440, 1920];

    public static IReadOnlyList<NerdNamedBreakpoint> GetNamedBreakpoints(NerdDesignTokenOptions? options)
    {
        var source = options is { Breakpoints.Count: > 0 }
            ? options.Breakpoints
            : NerdFoundationTaxonomyTools.DefaultBreakpoints;

        return source
            .Select(pair => new NerdNamedBreakpoint(
                pair.Key,
                ParsePx(pair.Value),
                pair.Value,
                ToMudBreakpoint(pair.Key)))
            .OrderBy(breakpoint => breakpoint.WidthPx)
            .ThenBy(breakpoint => breakpoint.Name, StringComparer.Ordinal)
            .ToList();
    }

    public static double[] GetComparisonColumns(NerdDesignTokenOptions? options)
    {
        var widths = GetNamedBreakpoints(options)
            .Select(breakpoint => breakpoint.WidthPx)
            .Where(width => width > 0)
            .Distinct()
            .OrderBy(width => width)
            .ToArray();

        return widths.Length >= 3 ? widths : FallbackComparisonColumns;
    }

    public static Breakpoint? ToMudBreakpoint(string name) =>
        name.Trim().ToLowerInvariant() switch
        {
            "xs" => Breakpoint.Xs,
            "sm" => Breakpoint.Sm,
            "md" => Breakpoint.Md,
            "lg" => Breakpoint.Lg,
            "xl" => Breakpoint.Xl,
            "xxl" => Breakpoint.Xxl,
            _ => null
        };

    public static double ParsePx(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var trimmed = value.Trim();
        if (trimmed.EndsWith("px", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed[..^2];
        }

        return double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out var pixels)
            ? pixels
            : 0;
    }
}

public sealed record NerdNamedBreakpoint(string Name, double WidthPx, string RawValue, Breakpoint? MudBreakpoint);
