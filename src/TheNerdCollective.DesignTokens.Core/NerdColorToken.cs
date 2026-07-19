namespace TheNerdCollective.MudComponents.DesignTokens;

public sealed class NerdColorToken
{
    public required string Value { get; init; }
    public string? Light { get; init; }
    public string? Dark { get; init; }
    public string? ContrastText { get; init; }
    public string? DarkContrastText { get; init; }
    public string? Surface { get; init; }
    public string? Content { get; init; }
    public string? Interactive { get; init; }
    public string? Hover { get; init; }
    public string? Active { get; init; }
    public string? Border { get; init; }
    public string? Disabled { get; init; }
    public string? Description { get; init; }
    public IReadOnlyList<string>? Roles { get; init; }
}
