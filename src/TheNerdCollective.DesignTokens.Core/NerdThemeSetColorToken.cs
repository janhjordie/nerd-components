namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Per-mode color override inside a theme set (HR-104).</summary>
public sealed class NerdThemeSetColorToken
{
    public string? Value { get; init; }
    public string? Light { get; init; }
    public string? Dark { get; init; }
    public string? ContrastText { get; init; }
    public string? DarkContrastText { get; init; }
    public string? Surface { get; init; }
    public string? Content { get; init; }
}
