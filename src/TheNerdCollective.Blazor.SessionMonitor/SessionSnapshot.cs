namespace TheNerdCollective.Blazor.SessionMonitor;

/// <summary>
/// Represents a historical snapshot of session activity.
/// </summary>
public class SessionSnapshot
{
    /// <summary>
    /// Timestamp of the snapshot.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Number of active sessions at this time.
    /// </summary>
    public int ActiveSessions { get; set; }

    /// <summary>
    /// Sessions started in the last interval.
    /// </summary>
    public int SessionsStarted { get; set; }

    /// <summary>
    /// Sessions ended in the last interval.
    /// </summary>
    public int SessionsEnded { get; set; }
}
