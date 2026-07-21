using TheNerdCollective.MudComponents.DesignTokens;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.PlayBook;

/// <summary>Wraps palette-token previews that need a contrasting surface (e.g. chalk text on navy).</summary>
public static class NerdPlayBookPreviewScope
{
    public static bool NeedsContrastBackdrop(NerdDesignTokenOptions options, string tokenClass)
    {
        if (!TryResolveTokenName(options, tokenClass, out var tokenName))
        {
            return false;
        }

        if (TryResolveOnIntentParentSurface(tokenName, out var parentSurface) &&
            options.Aliases.ContainsKey(parentSurface))
        {
            return true;
        }

        if (NerdPlayBookTokenFilter.IsIntentScope(tokenName) ||
            !options.Colors.TryGetValue(tokenName, out var token))
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

        if (TryResolveOnIntentParentSurface(tokenName, out var parentSurface) &&
            options.Aliases.ContainsKey(parentSurface))
        {
            return NerdDesignSystemUi.TokenClass(options.Prefix, parentSurface);
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

    /// <summary>
    /// Non-surface backdrops (palette tokens, action intents) need an explicit surface paint;
    /// surface intents like <c>brand-chrome</c> already set <c>background-color</c> on their root class.
    /// </summary>
    public static string? ResolveBackdropStyle(NerdDesignTokenOptions options, string tokenClass)
    {
        if (!NeedsContrastBackdrop(options, tokenClass) ||
            !TryResolveTokenName(options, tokenClass, out var contentTokenName))
        {
            return null;
        }

        var backdropClass = ResolveBackdropClass(options, tokenClass);
        if (backdropClass is null || !TryResolveTokenName(options, backdropClass, out var surfaceTokenName))
        {
            return null;
        }

        if (IsSurfaceIntent(surfaceTokenName))
        {
            return null;
        }

        var prefix = options.Prefix;
        return
            $"background-color: var(--{prefix}-color-{surfaceTokenName}); color: var(--{prefix}-color-{contentTokenName});";
    }

    private static bool IsSurfaceIntent(string tokenName) =>
        string.Equals(tokenName, NerdDesignSystemUi.PageSurface, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(tokenName, NerdDesignSystemUi.BrandChrome, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(tokenName, NerdDesignSystemUi.NavSurface, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(tokenName, NerdDesignSystemUi.InputSurface, StringComparison.OrdinalIgnoreCase);

    private static bool TryResolveOnIntentParentSurface(string tokenName, out string parentSurface)
    {
        const string onPrefix = "on-";
        if (tokenName.StartsWith(onPrefix, StringComparison.Ordinal) && tokenName.Length > onPrefix.Length)
        {
            parentSurface = tokenName[onPrefix.Length..];
            return true;
        }

        parentSurface = string.Empty;
        return false;
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
