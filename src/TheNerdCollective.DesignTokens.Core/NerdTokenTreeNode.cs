namespace TheNerdCollective.MudComponents.DesignTokens;

public sealed class NerdTokenTreeNode
{
    public required string Id { get; init; }

    public required string Label { get; init; }

    public NerdTokenTreeTargetKind Kind { get; init; } = NerdTokenTreeTargetKind.Group;

    public string? TargetId { get; init; }

    public string? Detail { get; init; }

    public string? AnchorId { get; init; }

    public IReadOnlyList<NerdTokenTreeNode> Children { get; init; } = [];
}
