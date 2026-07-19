namespace TheNerdCollective.MudComponents.DesignTokens;

public sealed class NerdTokenTreeNavigation
{
    public NerdTokenTreeNavigation(NerdTokenTreeTargetKind kind, string targetId, string? anchorId = null)
    {
        Kind = kind;
        TargetId = targetId;
        AnchorId = anchorId;
    }

    public NerdTokenTreeTargetKind Kind { get; }

    public string TargetId { get; }

    public string? AnchorId { get; }
}
