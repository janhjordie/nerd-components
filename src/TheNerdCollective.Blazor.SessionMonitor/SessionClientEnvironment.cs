namespace TheNerdCollective.Blazor.SessionMonitor;

/// <summary>
/// Parsed browser client environment from HTTP headers at circuit open.
/// </summary>
public class SessionClientEnvironment
{
    public static SessionClientEnvironment Unknown { get; } = new();

    /// <summary>
    /// Browser engine/product name (Chrome, Safari, Firefox, Edge, …).
    /// </summary>
    public string Browser { get; set; } = "Unknown";

    /// <summary>
    /// Device class: Desktop, Tablet, or Mobile.
    /// </summary>
    public string FormFactor { get; set; } = "Desktop";

    /// <summary>
    /// Operating system/platform (Windows, macOS, iOS, Android, Linux, …).
    /// </summary>
    public string Platform { get; set; } = "Unknown";

    /// <summary>
    /// True when the client looks like automated testing (Playwright, headless Chrome, …).
    /// </summary>
    public bool IsAutomation { get; set; }

    /// <summary>
    /// Short automation label when <see cref="IsAutomation"/> is true (Playwright, Headless, …).
    /// </summary>
    public string? AutomationKind { get; set; }

    /// <summary>
    /// Raw user agent captured at circuit open (truncated for dashboards).
    /// </summary>
    public string? UserAgent { get; set; }
}
