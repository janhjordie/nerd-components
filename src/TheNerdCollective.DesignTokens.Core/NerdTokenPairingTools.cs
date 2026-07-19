using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

public static class NerdTokenPairingTools
{
    public static string SuggestContentToken(string surfaceToken, NerdDesignTokenOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(surfaceToken);
        ArgumentNullException.ThrowIfNull(options);

        if (!options.Colors.ContainsKey(surfaceToken))
        {
            throw new ArgumentException($"Unknown surface token '{surfaceToken}'.", nameof(surfaceToken));
        }

        var policy = options.PairingPolicy;
        if (policy is not null && policy.IsActive(options))
        {
            return policy.SuggestContentToken(surfaceToken, options);
        }

        var surfaceColor = ResolveTokenColor(surfaceToken, options);
        var preferLightForeground = !NerdColorParser.IsLight(surfaceColor);

        return options.Colors
            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(pair => NerdColorParser.IsLight(pair.Value.Value) == preferLightForeground)
            .Key ?? options.Colors.Keys.First();
    }

    public static string SuggestActionToken(
        NerdDesignTokenOptions options,
        string surfaceToken,
        string contentToken)
    {
        ArgumentNullException.ThrowIfNull(options);

        var policy = options.PairingPolicy;
        if (policy is not null && policy.IsActive(options))
        {
            return policy.SuggestActionToken(options, surfaceToken, contentToken);
        }

        return options.Colors.Keys
            .FirstOrDefault(name =>
                !string.Equals(name, surfaceToken, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(name, contentToken, StringComparison.OrdinalIgnoreCase))
            ?? contentToken;
    }

    public static NerdTokenPairingValidation ValidatePairing(
        string contentToken,
        string surfaceToken,
        NerdDesignTokenOptions options,
        Func<string, string>? colorResolver = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(surfaceToken);
        ArgumentNullException.ThrowIfNull(options);

        var resolveForeground = colorResolver ?? (token => ResolvePairingForegroundColor(token, options));
        var resolveSurface = colorResolver ?? (token => ResolvePairingSurfaceColor(token, options));
        var surfaceColor = resolveSurface(surfaceToken);
        var contentColor = resolveForeground(contentToken);
        var ratio = NerdColorParser.ContrastRatio(surfaceColor, contentColor);
        var meetsAa = ratio >= NerdDesignTokenTools.AaNormalTextRatio;

        var policy = options.PairingPolicy;
        var hasBrandGuide = policy is not null && policy.IsActive(options);
        var isBrandApproved = !hasBrandGuide ||
                              policy!.IsBrandApprovedPairing(contentToken, surfaceToken);

        string? brandMessage = null;
        if (hasBrandGuide)
        {
            var activePolicy = policy!;
            brandMessage = isBrandApproved
                ? $"{contentToken} on {surfaceToken} is approved in the {activePolicy.BrandGuideName} guide."
                : $"{contentToken} on {surfaceToken} is not listed in the {activePolicy.BrandGuideName} guide.";
        }

        return new NerdTokenPairingValidation(
            contentToken,
            surfaceToken,
            contentColor,
            surfaceColor,
            ratio,
            meetsAa,
            isBrandApproved,
            brandMessage);
    }

    public static string ResolveTokenColor(string tokenName, NerdDesignTokenOptions options) =>
        options.Colors[tokenName].Light ?? options.Colors[tokenName].Value;

    public static string ResolvePairingForegroundColor(string tokenName, NerdDesignTokenOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenName);
        ArgumentNullException.ThrowIfNull(options);

        var policy = options.PairingPolicy;
        if (policy is not null && policy.IsActive(options))
        {
            return policy.ResolveForegroundColor(tokenName, options);
        }

        var token = options.Colors[tokenName];
        return token.Content ?? token.Light ?? token.Value;
    }

    public static string ResolvePairingSurfaceColor(string tokenName, NerdDesignTokenOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenName);
        ArgumentNullException.ThrowIfNull(options);

        var policy = options.PairingPolicy;
        if (policy is not null && policy.IsActive(options))
        {
            return policy.ResolveSurfaceColor(tokenName, options);
        }

        var token = options.Colors[tokenName];
        return token.Surface ?? token.Light ?? token.Value;
    }
}
