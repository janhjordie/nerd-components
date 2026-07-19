using TheNerdCollective.MudComponents.DesignTokens;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.PlayBook;

/// <summary>Wraps palette-token previews that need a contrasting surface (e.g. chalk text on navy).</summary>
public static class NerdPlayBookPreviewScope
{
    public static bool NeedsContrastBackdrop(NerdDesignTokenOptions options, string tokenClass)
    {
        if (!TryResolveTokenName(options, tokenClass, out var tokenName) ||
            !options.Colors.TryGetValue(tokenName, out var token))
        {
            return false;
        }

        if (NerdPlayBookTokenFilter.IsIntentScope(tokenName))
        {
            return false;
        }

        var surface = token.Surface ?? token.Light ?? token.Value;
        var content = token.Content
                      ?? token.ContrastText
                      ?? NerdColorValue.ContrastText(surface);

        if (string.Equals(surface, content, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return NerdColorParser.ContrastRatio(surface, content) < 3.0;
    }

    public static string? ResolveBackdropClass(NerdDesignTokenOptions options, string tokenClass)
    {
        if (!TryResolveTokenName(options, tokenClass, out var tokenName))
        {
            return null;
        }

        if (options.PairingPolicy is not null)
        {
            foreach (var (content, surface) in options.PairingPolicy.GetApprovedPairings())
            {
                if (string.Equals(content, tokenName, StringComparison.OrdinalIgnoreCase))
                {
                    return NerdDesignSystemUi.TokenClass(options.Prefix, surface);
                }
            }
        }

        foreach (var alias in options.Aliases)
        {
            if (string.Equals(alias.Value, tokenName, StringComparison.OrdinalIgnoreCase) &&
                options.Aliases.TryGetValue(NerdDesignSystemUi.PageSurface, out var pageSurface))
            {
                return NerdDesignSystemUi.TokenClass(options.Prefix, pageSurface);
            }
        }

        return options.Colors.Keys
            .FirstOrDefault(name =>
                !string.Equals(name, tokenName, StringComparison.OrdinalIgnoreCase) &&
                options.Colors.TryGetValue(name, out var candidate) &&
                NerdColorParser.ContrastRatio(
                    candidate.Surface ?? candidate.Light ?? candidate.Value,
                    options.Colors[tokenName].Content
                    ?? options.Colors[tokenName].ContrastText
                    ?? NerdColorValue.ContrastText(options.Colors[tokenName].Value)) >= 4.5)
            is { } fallbackName
            ? NerdDesignSystemUi.TokenClass(options.Prefix, fallbackName)
            : null;
    }

    private static bool TryResolveTokenName(NerdDesignTokenOptions options, string tokenClass, out string tokenName)
    {
        var prefix = $"{options.Prefix}-";
        if (tokenClass.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            tokenName = tokenClass[prefix.Length..];
            return true;
        }

        tokenName = string.Empty;
        return false;
    }
}
