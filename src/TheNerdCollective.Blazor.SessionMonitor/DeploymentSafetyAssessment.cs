namespace TheNerdCollective.Blazor.SessionMonitor;

/// <summary>
/// Deployment safety based on connected circuits, excluding admin monitor browsers.
/// </summary>
public class DeploymentSafetyAssessment
{
    /// <summary>
    /// Total connected circuits on this replica.
    /// </summary>
    public int ActiveSessions { get; set; }

    /// <summary>
    /// Connected circuits that are not the admin viewing the monitor (nor other tabs from that browser).
    /// </summary>
    public int NonAdminActiveSessions { get; set; }

    /// <summary>
    /// Connected circuits excluded as admin monitor traffic.
    /// </summary>
    public int AdminMonitorSessions { get; set; }

    /// <summary>
    /// Whether at least one connected circuit is on an admin monitor path.
    /// </summary>
    public bool HasAdminMonitorSession { get; set; }

    /// <summary>
    /// Threshold used for the assessment.
    /// </summary>
    public int MaxActiveSessions { get; set; }

    /// <summary>
    /// True when <see cref="NonAdminActiveSessions"/> is within the threshold.
    /// </summary>
    public bool CanDeploy { get; set; }
}
