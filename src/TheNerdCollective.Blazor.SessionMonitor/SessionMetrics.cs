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
    /// Total number of sessions started since tracking began.
    /// </summary>
    public long TotalSessionsStarted { get; set; }

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
}
