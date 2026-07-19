namespace TheNerdCollective.MudComponents.DesignTokens;

public sealed record NerdTokenPairingValidation(
    string ContentToken,
    string SurfaceToken,
    string ContentColor,
    string SurfaceColor,
    double ContrastRatio,
    bool MeetsAa,
    bool IsBrandApproved,
    string? BrandMessage);
