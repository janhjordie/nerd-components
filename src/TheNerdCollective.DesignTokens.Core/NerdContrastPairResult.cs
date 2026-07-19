namespace TheNerdCollective.MudComponents.DesignTokens;

public sealed record NerdContrastPairResult(
    string ForegroundToken,
    string BackgroundToken,
    string ForegroundColor,
    string BackgroundColor,
    double Ratio,
    bool MeetsAa,
    bool MeetsAaa);
