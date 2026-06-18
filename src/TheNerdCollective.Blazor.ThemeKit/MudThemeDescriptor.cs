using MudBlazor;

namespace TheNerdCollective.Blazor.ThemeKit;

public sealed record MudThemeDescriptor(
    string Id,
    string DisplayName,
    string Version,
    string? PreviewPrimaryHex,
    string? UpdatedAt = null,
    string? Source = null);
