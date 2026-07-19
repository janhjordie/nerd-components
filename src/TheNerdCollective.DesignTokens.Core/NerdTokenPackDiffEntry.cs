namespace TheNerdCollective.MudComponents.DesignTokens;

public enum NerdTokenPackDiffKind
{
    Added,
    Removed,
    Modified
}

public sealed record NerdTokenPackDiffEntry(
    string Section,
    string Name,
    NerdTokenPackDiffKind Kind,
    string? Before,
    string? After);
