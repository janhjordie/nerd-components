namespace TheNerdCollective.Blazor.SessionMonitor;

/// <summary>
/// Bounds for session-monitor query parameters. Rejects values that can stall or over-allocate.
/// </summary>
internal static class SessionMonitorQueryLimits
{
    internal const int MaxHistoryCount = 10000;
    internal const int MaxWindowMinutes = 24 * 60;
    internal const int MaxLookbackHours = 24 * 14;

    internal static string? ValidateHistoryCount(int maxCount)
    {
        if (maxCount is < 1 or > MaxHistoryCount)
        {
            return $"maxCount must be between 1 and {MaxHistoryCount}.";
        }

        return null;
    }

    internal static string? ValidateDeploymentWindow(int windowMinutes, int lookbackHours)
    {
        if (windowMinutes is < 1 or > MaxWindowMinutes)
        {
            return $"windowMinutes must be between 1 and {MaxWindowMinutes}.";
        }

        if (lookbackHours is < 1 or > MaxLookbackHours)
        {
            return $"lookbackHours must be between 1 and {MaxLookbackHours}.";
        }

        return null;
    }
}
