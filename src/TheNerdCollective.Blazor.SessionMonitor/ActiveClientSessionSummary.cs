namespace TheNerdCollective.Blazor.SessionMonitor;

/// <summary>
/// Active session counts grouped by browser client identifier (shared cookie across tabs).
/// </summary>
public class ActiveClientSessionSummary
{
    public const string UnknownClientLabel = "(unknown browser)";

    /// <summary>
    /// Raw client identifier from the browser cookie, when available.
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// Short label for dashboards (first 8 characters of <see cref="ClientId"/>).
    /// </summary>
    public string ClientLabel { get; set; } = "";

    /// <summary>
    /// Total active circuits for this browser client (typically one per tab).
    /// </summary>
    public int ActiveSessionCount { get; set; }

    /// <summary>
    /// Circuits currently connected for this browser client.
    /// </summary>
    public int ConnectedCount { get; set; }

    /// <summary>
    /// Circuits disconnected but retained for this browser client.
    /// </summary>
    public int DisconnectedCount { get; set; }

    /// <summary>
    /// Number of distinct URL paths open across this browser client's circuits.
    /// </summary>
    public int DistinctPathCount { get; set; }

    /// <summary>
    /// Active circuits belonging to this browser client.
    /// </summary>
    public IReadOnlyList<ActiveCircuitSession> Circuits { get; set; } = [];
}
