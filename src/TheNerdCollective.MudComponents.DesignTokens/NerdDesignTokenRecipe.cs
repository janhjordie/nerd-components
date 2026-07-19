namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Describes a reusable composition of design-token roles.
/// </summary>
public sealed record NerdDesignTokenRecipe(
    string Surface,
    string Content,
    string? Action = null,
    string? Border = null,
    string? Label = null,
    string? Usage = null);
