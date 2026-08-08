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

    /// <summary>
    /// In-scope gap — expected to be pack-driven but currently falls through to Mud.
    /// Counts against brand-relevant coverage.
    /// </summary>
    Unmapped = 3,

    /// <summary>
    /// Intentional Mud default (structural chrome / opacities / high elevations).
    /// Visible in the inventory but excluded from the coverage score.
    /// </summary>
    AcceptedDefault = 4,

    /// <summary>
    /// Owned by another system (e.g. ResponsiveTypography), not <c>NerdMudThemeFactory</c>.
    /// Excluded from the coverage score.
    /// </summary>
    External = 5
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

/// <summary>
/// Aggregate coverage for the MudTheme inventory.
/// <see cref="CoveredPercent"/> is brand-relevant only: excludes
/// <see cref="NerdMudThemeMappingStatus.AcceptedDefault"/> and
/// <see cref="NerdMudThemeMappingStatus.External"/>.
/// </summary>
public sealed record NerdMudThemeMappingCoverage(
    int Total,
    int Mapped,
    int Hardcoded,
    int Derived,
    int Unmapped,
    int AcceptedDefault,
    int External)
{
    public int Covered => Mapped + Hardcoded + Derived;

    /// <summary>Properties that count toward the brand-relevant score (covered + real gaps).</summary>
    public int InScope => Covered + Unmapped;

    public double CoveredPercent =>
        InScope == 0 ? 100 : Math.Round(100.0 * Covered / InScope, 1);
}
