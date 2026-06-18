namespace TheNerdCollective.Blazor.ThemeKit;

public enum ThemeTokenKind
{
    Color,
    Text,
}

public sealed record ThemeTokenDefinition(
    string Id,
    string Label,
    ThemeTokenKind Kind,
    string Group,
    Func<MudBlazor.MudTheme, string?> GetValue,
    Action<MudBlazor.MudTheme, string> SetValue);
