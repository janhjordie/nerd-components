using Microsoft.AspNetCore.Components.Server.Circuits;

namespace TheNerdCollective.Blazor.SessionMonitor;

/// <summary>
/// Service for tracking and monitoring active Blazor Server sessions/circuits.
/// </summary>
public interface ISessionMonitorService
{
    /// <summary>
    /// Get current session metrics.
    /// </summary>
    SessionMetrics GetCurrentMetrics();

    /// <summary>
    /// Get historical session snapshots.
    /// </summary>
    /// <param name="since">Start time for historical data (optional).</param>
    /// <param name="maxCount">Maximum number of snapshots to return.</param>
    IEnumerable<SessionSnapshot> GetHistory(DateTime? since = null, int maxCount = 100);

    /// <summary>
    /// Get all currently active circuit IDs.
    /// </summary>
    IEnumerable<string> GetActiveCircuitIds();

    /// <summary>
    /// Check if there are any active sessions.
    /// </summary>
    bool HasActiveSessions();

    /// <summary>
    /// Find optimal deployment windows (periods with zero or minimal active sessions).
    /// </summary>
    /// <param name="windowMinutes">Length of deployment window in minutes.</param>
    /// <param name="lookbackHours">How far back to analyze (default 24 hours).</param>
    IEnumerable<DeploymentWindow> FindOptimalDeploymentWindows(int windowMinutes = 5, int lookbackHours = 24);
}

/// <summary>
/// Represents a potential deployment window.
/// </summary>
public class DeploymentWindow
{
    /// <summary>
    /// Start time of the window.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// End time of the window.
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Maximum number of active sessions during this window.
    /// </summary>
    public int MaxActiveSessions { get; set; }

    /// <summary>
    /// Average number of active sessions during this window.
    /// </summary>
    public double AverageActiveSessions { get; set; }

    /// <summary>
    /// Whether this window had zero active sessions.
    /// </summary>
    public bool ZeroSessionsWindow => MaxActiveSessions == 0;
}
