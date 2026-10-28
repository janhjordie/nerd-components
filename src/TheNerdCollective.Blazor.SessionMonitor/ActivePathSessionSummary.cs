namespace TheNerdCollective.Blazor.SessionMonitor;

/// <summary>
/// Active session counts grouped by current URL path.
/// </summary>
public class ActivePathSessionSummary
{
    public const string UnknownPathLabel = "(unknown path)";
    /// <summary>
    /// Relative path and query, or a placeholder when unknown.
    /// </summary>
    public string Path { get; set; } = "";

    /// <summary>
    /// Connected circuits on this path. Retained circuits are <see cref="DisconnectedCount"/>.
    /// </summary>
    public int ActiveSessionCount { get; set; }

    /// <summary>
    /// Circuits currently connected on this path.
    /// </summary>
    public int ConnectedCount { get; set; }

    /// <summary>
    /// Circuits disconnected but retained on this path.
    /// </summary>
    public int DisconnectedCount { get; set; }
}
