namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Named token set for a color mode (light, dark, …).</summary>
public sealed class NerdThemeSet
{
    public string Id { get; init; } = "light";
    public string? DisplayName { get; init; }
    public Dictionary<string, NerdThemeSetColorToken> Colors { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);
}
