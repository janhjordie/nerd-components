namespace TheNerdCollective.MudComponents.DesignTokens;

public sealed record NerdContrastEvaluation(
    string Mode,
    string Background,
    string Foreground,
    double ContrastRatio,
    bool MeetsAa,
    bool MeetsAaa,
    string RecommendedForeground);

public sealed record NerdAccessibilityResult(
    string Name,
    string WcagVersion,
    NerdContrastEvaluation Light,
    NerdContrastEvaluation Dark)
{
    public bool MeetsAa => Light.MeetsAa && Dark.MeetsAa;
    public bool MeetsAaa => Light.MeetsAaa && Dark.MeetsAaa;
}

public sealed record NerdAccessibilityWarning(
    string TokenName,
    string Mode,
    string WcagVersion,
    double ContrastRatio,
    double RequiredRatio,
    string Level,
    string RecommendedForeground);
