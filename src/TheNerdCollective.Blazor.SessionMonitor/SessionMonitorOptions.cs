namespace TheNerdCollective.Blazor.SessionMonitor;

/// <summary>
/// Configuration for session monitoring behaviour under load.
/// </summary>
public class SessionMonitorOptions
{
    /// <summary>
    /// When active circuits reach this count, tracking switches to summary-only mode.
    /// </summary>
    public int DegradedModeThreshold { get; set; } = 500;

    /// <summary>
    /// Maximum history snapshots retained in normal mode.
    /// </summary>
    public int NormalMaxHistorySize { get; set; } = 10000;

    /// <summary>
    /// Maximum history snapshots retained in degraded mode (history is trimmed when entering degraded).
    /// </summary>
    public int DegradedMaxHistorySize { get; set; } = 2000;

    /// <summary>
    /// Maximum URL path rows returned in degraded mode (top paths by session count).
    /// </summary>
    public int DegradedMaxPathSummaries { get; set; } = 100;
}
