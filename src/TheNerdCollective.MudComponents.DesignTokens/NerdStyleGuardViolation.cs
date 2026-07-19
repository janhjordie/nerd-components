namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// A composed UI placement that fails contrast requirements (e.g. accent labels on page-surface).
/// </summary>
public sealed record NerdStyleGuardViolation(
    string Placement,
    string Role,
    string Foreground,
    string Background,
    double ContrastRatio,
    double RequiredRatio);
