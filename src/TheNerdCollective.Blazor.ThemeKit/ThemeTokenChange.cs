namespace TheNerdCollective.Blazor.ThemeKit;

public sealed record ThemeTokenChange(
    string TokenId,
    string Label,
    string? CatalogValue,
    string? CurrentValue);
