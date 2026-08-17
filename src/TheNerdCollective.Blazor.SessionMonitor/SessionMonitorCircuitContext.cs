namespace TheNerdCollective.Blazor.SessionMonitor;

/// <summary>
/// Scoped per-circuit context populated by <see cref="SessionMonitorCircuitHandler"/>.
/// </summary>
public sealed class SessionMonitorCircuitContext
{
  public string? CircuitId { get; internal set; }
}
