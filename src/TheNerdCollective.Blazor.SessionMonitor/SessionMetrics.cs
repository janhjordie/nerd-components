namespace TheNerdCollective.Blazor.SessionMonitor;

/// <summary>
/// Represents a snapshot of session metrics at a point in time.
/// </summary>
public class SessionMetrics
{
    /// <summary>
    /// Number of currently active sessions.
    /// </summary>
    public int ActiveSessions { get; set; }

    /// <summary>
    /// Timestamp when the metrics were recorded.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Peak number of concurrent sessions in the current tracking window.
    /// </summary>
    public int PeakSessions { get; set; }

    /// <summary>
    /// Total number of unique client sessions started since tracking began.
    /// When reload deduplication is enabled, page reloads from the same browser
    /// client do not increment this counter.
    /// </summary>
    public long TotalSessionsStarted { get; set; }

    /// <summary>
    /// Total number of Blazor circuits opened since tracking began (includes reloads).
    /// </summary>
    public long TotalCircuitsOpened { get; set; }

    /// <summary>
    /// Total number of sessions ended since tracking began.
    /// </summary>
    public long TotalSessionsEnded { get; set; }

    /// <summary>
    /// Average session duration in seconds (if available).
    /// </summary>
    public double? AverageSessionDurationSeconds { get; set; }

    /// <summary>
    /// Number of circuits currently in disconnected-but-retained state
    /// (WebSocket dropped, server holding circuit in retention window).
    /// </summary>
    public int DisconnectedSessions { get; set; }

    /// <summary>
    /// Total number of WebSocket disconnects since tracking began.
    /// </summary>
    public long TotalDisconnects { get; set; }

    /// <summary>
    /// Total number of successful reconnects since tracking began.
    /// </summary>
    public long TotalReconnects { get; set; }

    /// <summary>
    /// When session tracking started (process start time).
    /// </summary>
    public DateTime TrackingSince { get; set; }

    /// <summary>
    /// Current tracking mode (normal or degraded summary-only).
    /// </summary>
    public SessionTrackingMode TrackingMode { get; set; }

    /// <summary>
    /// Whether degraded summary-only mode is active.
    /// </summary>
    public bool IsDegradedMode => TrackingMode == SessionTrackingMode.DegradedSummaryOnly;

    /// <summary>
    /// Threshold that triggers degraded mode.
    /// </summary>
    public int DegradedModeThreshold { get; set; }

    /// <summary>
    /// Current effective history snapshot cap.
    /// </summary>
    public int EffectiveHistoryCap { get; set; }

    /// <summary>
    /// Replica or process identifier for this monitor instance (metrics are per-process).
    /// </summary>
    public string? InstanceId { get; set; }

    /// <summary>
    /// Host machine name for this monitor instance.
    /// </summary>
    public string? MachineName { get; set; }
}
