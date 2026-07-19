namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Single entry point for MudBlazor SCSS harvest and inventory validation (HR-114 wave 3).
/// Keeps harvest concerns in the Mud adapter assembly — not in DesignTokens.Core.
/// </summary>
public static class NerdMudHarvestAdapter
{
    public static IReadOnlyList<NerdMudInventoryRuleEntry> LoadRuleTable(string? mudVersion = null) =>
        NerdMudInventoryRuleTable.Load(mudVersion);

    public static IReadOnlyList<string> ValidateHarvestCoverage(string? mudVersion = null) =>
        NerdMudInventoryValidator.ValidateHarvestCoverage(mudVersion);

    public static IReadOnlyList<string> ValidateGeneratedCss(NerdDesignTokenOptions options) =>
        NerdMudInventoryRuleTable.ValidateGeneratedCss(options);

    public static NerdMudStateParityResult EvaluateStateParity(NerdDesignTokenOptions options) =>
        NerdMudStateParityTools.Evaluate(options);
}
