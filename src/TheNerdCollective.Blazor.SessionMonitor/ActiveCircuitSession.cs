namespace TheNerdCollective.Blazor.SessionMonitor;

/// <summary>
/// Snapshot of an active Blazor Server circuit for monitoring dashboards.
/// </summary>
public class ActiveCircuitSession
{
  public string CircuitId { get; set; } = "";

  /// <summary>
  /// Browser client identifier shared across tabs (from session monitor cookie).
  /// </summary>
  public string? ClientId { get; set; }

  /// <summary>
  /// Short label for dashboards (first 8 characters of <see cref="ClientId"/>).
  /// </summary>
  public string? ClientLabel { get; set; }

  /// <summary>
  /// Relative path and query for the page the circuit is currently on.
  /// </summary>
  public string? CurrentPath { get; set; }

  public DateTime? CurrentPathUpdatedAt { get; set; }

  public DateTime StartedAt { get; set; }

  public bool IsDisconnected { get; set; }

  /// <summary>
  /// True when this circuit is on an admin monitor path (for example <c>/developer/monitor</c>).
  /// </summary>
  public bool IsOnAdminMonitorPath { get; set; }

  /// <summary>
  /// True when this circuit belongs to a browser group with an active admin monitor session.
  /// </summary>
  public bool IsAdminMonitorGroup { get; set; }
}
