namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// One row in the token → <see cref="MudBlazor.MudTheme"/> mapping catalog (<c>/nerd-theme</c>).
/// </summary>
public sealed record NerdMudThemeMappingEntry(
    string Category,
    string MudThemeProperty,
    string CssVariable,
    string? BindingAlias,
    string? ColorToken,
    string LightValue,
    string DarkValue,
    string ValueKind,
    string Notes);