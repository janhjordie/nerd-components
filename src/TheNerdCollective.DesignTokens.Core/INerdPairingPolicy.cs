namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Brand-specific foreground/surface pairing rules for recipe studio and catalog previews.
/// </summary>
public interface INerdPairingPolicy
{
    string BrandGuideName { get; }

    bool IsActive(NerdDesignTokenOptions options);

    string SuggestContentToken(string surfaceToken, NerdDesignTokenOptions options);

    string SuggestActionToken(NerdDesignTokenOptions options, string surfaceToken, string contentToken);

    bool IsBrandApprovedPairing(string contentToken, string surfaceToken);

    IReadOnlyList<string> GetApprovedContentTokens(string surfaceToken, NerdDesignTokenOptions options);

    IReadOnlyList<(string Content, string Surface)> GetApprovedPairings();

    string ResolveForegroundColor(string tokenName, NerdDesignTokenOptions options);

    string ResolveSurfaceColor(string tokenName, NerdDesignTokenOptions options);
}
