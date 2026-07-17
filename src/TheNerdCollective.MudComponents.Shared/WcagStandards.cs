namespace TheNerdCollective.MudComponents.Shared;

/// <summary>
/// Shared WCAG 2.1 thresholds used by design token and typography packages.
/// </summary>
public static class WcagStandards
{
    /// <summary>Default WCAG version label used in accessibility badges.</summary>
    public const string DefaultVersion = "2.1";

    /// <summary>WCAG AA contrast ratio for normal text.</summary>
    public const double AaNormalTextRatio = 4.5;

    /// <summary>WCAG AA contrast ratio for large text.</summary>
    public const double AaLargeTextRatio = 3.0;

    /// <summary>WCAG AAA contrast ratio for normal text.</summary>
    public const double AaaNormalTextRatio = 7.0;

    /// <summary>WCAG AAA contrast ratio for large text.</summary>
    public const double AaaLargeTextRatio = 4.5;

    /// <summary>Recommended minimum line-height ratio for body text.</summary>
    public const double MinimumLineHeightRatio = 1.5;

    /// <summary>Recommended minimum letter spacing in em units.</summary>
    public const double MinimumLetterSpacingEm = 0.12;

    /// <summary>Recommended minimum word spacing in em units.</summary>
    public const double MinimumWordSpacingEm = 0.16;
}
