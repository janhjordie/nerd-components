namespace TheNerdCollective.Blazor.SessionMonitor;

/// <summary>
/// Session monitor tracking mode based on active circuit load.
/// </summary>
public enum SessionTrackingMode
{
    /// <summary>
    /// Full tracking: per-circuit details, full history cap.
    /// </summary>
    Normal = 0,

    /// <summary>
    /// High load: URL aggregates and totals only; circuit list and history reduced.
    /// </summary>
    DegradedSummaryOnly = 1
}
