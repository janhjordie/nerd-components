namespace TheNerdCollective.MudComponents.DesignTokens;

public sealed class NerdColorToken
{
    public required string Value { get; init; }
    public required string ContrastText { get; init; }
    public string? Hover { get; init; }
    public string? Active { get; init; }
    public string? Border { get; init; }
    public string? Disabled { get; init; }
}
