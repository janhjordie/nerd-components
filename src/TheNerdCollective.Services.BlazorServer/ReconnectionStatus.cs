namespace TheNerdCollective.Services.BlazorServer;

public sealed class ReconnectionStatus
{
    public string Status { get; set; } = "ok"; // ok | deploying | maintenance
    public string? ReconnectingMessage { get; set; }
    public string? DeploymentMessage { get; set; }
    public string? Version { get; set; }
    public IReadOnlyList<string>? Features { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
}
