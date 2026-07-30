using System.Globalization;
using TheNerdCollective.Blazor.Observability;

namespace TheNerdCollective.MudComponents.ObservabilityDashboard;

/// <summary>Formats observability scalar values for MudBlazor display.</summary>
public static class ObservabilityValueFormatter
{
    /// <summary>Formats a scalar result for display.</summary>
    public static string FormatScalar(ObservabilityScalarResult? result)
    {
        if (result is null)
        {
            return "—";
        }

        return FormatValue(result.Value, result.Unit);
    }

    /// <summary>Formats a numeric value with its unit token.</summary>
    public static string FormatValue(double value, string unit) =>
        unit switch
        {
            "reqps" => $"{value.ToString("F2", CultureInfo.InvariantCulture)}/s",
            "ms" => $"{value.ToString("F0", CultureInfo.InvariantCulture)} ms",
            "percentunit" => value.ToString("P1", CultureInfo.InvariantCulture),
            "bytes" => FormatBytes(value),
            "short" => value.ToString("F0", CultureInfo.InvariantCulture),
            _ => value.ToString("F2", CultureInfo.InvariantCulture)
        };

    private static string FormatBytes(double bytes)
    {
        if (bytes >= 1_073_741_824)
        {
            return $"{bytes / 1_073_741_824:F1} GB";
        }

        if (bytes >= 1_048_576)
        {
            return $"{bytes / 1_048_576:F1} MB";
        }

        if (bytes >= 1024)
        {
            return $"{bytes / 1024:F1} KB";
        }

        return $"{bytes:F0} B";
    }
}
