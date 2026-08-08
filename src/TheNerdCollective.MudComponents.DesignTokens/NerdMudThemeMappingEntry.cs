namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Mapping coverage status for one MudBlazor <c>MudTheme</c> property on <c>/nerd-theme</c>.
/// </summary>
public enum NerdMudThemeMappingStatus
{
    /// <summary>Driven by the active token pack / brand palette map.</summary>
    Mapped = 0,

    /// <summary>Factory sets a fixed value (not pack-driven).</summary>
    Hardcoded = 1,

    /// <summary>MudBlazor (or MudColor) derives this from another mapped slot.</summary>
    Derived = 2,

    /// <summary>Not mapped — Mud default remains. Visible gap on <c>/nerd-theme</c>.</summary>
    Unmapped = 3
}

/// <summary>
/// One row in the token → <see cref="MudBlazor.MudTheme"/> mapping catalog (<c>/nerd-theme</c>).
/// </summary>
public sealed record NerdMudThemeMappingEntry(
    string Category,
    string MudThemeProperty,
    string CssVariable,
    string? BindingAlias,
    string? ColorToken,
    string LightValue,
    string DarkValue,
    string ValueKind,
    NerdMudThemeMappingStatus Status,
    string Notes);

/// <summary>Aggregate coverage for the full MudTheme inventory.</summary>
public sealed record NerdMudThemeMappingCoverage(
    int Total,
    int Mapped,
    int Hardcoded,
    int Derived,
    int Unmapped)
{
    public int Covered => Mapped + Hardcoded + Derived;

    public double CoveredPercent =>
        Total == 0 ? 0 : Math.Round(100.0 * Covered / Total, 1);
}
