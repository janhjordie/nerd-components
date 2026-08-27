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

    /// <summary>
    /// Optional override for <see cref="SessionMetrics.InstanceId"/> (useful in tests).
    /// </summary>
    public string? InstanceIdOverride { get; set; }

    /// <summary>
    /// Cookie name used to persist a browser client identifier across page reloads.
    /// </summary>
    public string ClientIdCookieName { get; set; } = ".bs-sm-client";

    /// <summary>
    /// When true, page reloads from the same browser client (same cookie) do not
    /// increment <see cref="SessionMetrics.TotalSessionsStarted"/>.
    /// </summary>
    public bool DeduplicateReloadStarts { get; set; } = true;

    /// <summary>
    /// URL path prefixes treated as admin session monitor routes. Circuits on these paths,
    /// and other circuits from the same browser client, are excluded from deploy safety counts.
    /// </summary>
    public string[] AdminMonitorPathPrefixes { get; set; } = ["/developer/monitor"];
}
