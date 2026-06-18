using System.Text.RegularExpressions;

namespace TheNerdCollective.Blazor.ThemeKit;

public static partial class ThemeIdHelper
{
    public static bool IsValidThemeId(string? themeId)
        => !string.IsNullOrWhiteSpace(themeId) && ThemeIdRegex().IsMatch(themeId);

    public static string SuggestFromDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return string.Empty;
        }

        var normalized = displayName.Trim().ToLowerInvariant();
        normalized = NonAlphanumericRegex().Replace(normalized, "-");
        normalized = MultiHyphenRegex().Replace(normalized, "-").Trim('-');

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return string.Empty;
        }

        return char.IsDigit(normalized[0]) ? $"theme-{normalized}" : normalized;
    }

    [GeneratedRegex(@"^[a-z][a-z0-9]*(-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex ThemeIdRegex();

    [GeneratedRegex(@"[^a-z0-9]+", RegexOptions.CultureInvariant)]
    private static partial Regex NonAlphanumericRegex();

    [GeneratedRegex(@"-{2,}", RegexOptions.CultureInvariant)]
    private static partial Regex MultiHyphenRegex();
}
