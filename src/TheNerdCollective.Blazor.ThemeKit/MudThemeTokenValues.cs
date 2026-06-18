using MudBlazor;
using MudBlazor.Utilities;

namespace TheNerdCollective.Blazor.ThemeKit;

internal static class MudThemeTokenValues
{
    public static string? GetColor(MudColor color)
        => color.ToString(MudColorOutputFormats.Hex);

    public static void SetColor(MudTheme theme, Action<MudTheme, MudColor> assign, string value)
    {
        assign(theme, new MudColor(value));
    }

    public static string? GetFontFamily(string[]? families)
        => families is null || families.Length == 0
            ? null
            : string.Join(", ", families);

    public static void SetFontFamily(MudTheme theme, string value)
    {
        theme.Typography.Default.FontFamily = value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
