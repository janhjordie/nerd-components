namespace TheNerdCollective.Blazor.SessionMonitor;

/// <summary>
/// Snapshot of an active Blazor Server circuit for monitoring dashboards.
/// </summary>
public class ActiveCircuitSession
{
  public string CircuitId { get; set; } = "";

  /// <summary>
  /// Relative path and query for the page the circuit is currently on.
  /// </summary>
  public string? CurrentPath { get; set; }

  public DateTime? CurrentPathUpdatedAt { get; set; }

  public DateTime StartedAt { get; set; }

  public bool IsDisconnected { get; set; }
}
