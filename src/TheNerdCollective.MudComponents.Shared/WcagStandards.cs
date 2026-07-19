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

    /// <summary>Recommended minimum line-height ratio (WCAG 2.1 1.4.12).</summary>
    public const double MinimumLineHeightRatio = 1.5;

    /// <summary>Recommended minimum letter spacing in em units (WCAG 2.1 1.4.12).</summary>
    public const double MinimumLetterSpacingEm = 0.12;

    /// <summary>Recommended minimum word spacing in em units (WCAG 2.1 1.4.12).</summary>
    public const double MinimumWordSpacingEm = 0.16;

    /// <summary>Recommended minimum for primary body copy at a 320px viewport (best practice, not a WCAG AA pixel rule).</summary>
    public const double PrimaryTextMinimumPixels = 16;

    /// <summary>Recommended minimum for secondary UI text at a 320px viewport.</summary>
    public const double SecondaryTextMinimumPixels = 14;

    /// <summary>Recommended minimum for caption/overline metadata at a 320px viewport (aligns with MudBlazor defaults).</summary>
    public const double AuxiliaryTextMinimumPixels = 12;

    /// <summary>Legacy alias for <see cref="PrimaryTextMinimumPixels"/>.</summary>
    public const double MinimumReadableFontSizePixels = PrimaryTextMinimumPixels;

    /// <summary>
    /// WCAG large-text threshold in px (18pt at 96dpi). Used for contrast classification only — not a typography floor.
    /// </summary>
    public const double LargeTextFontSizePixels = 24;

    /// <summary>Returns the storyboard minimum computed size for a typography role at 320px viewport.</summary>
    public static double GetTypographyRoleMinimumPixels(string role) => role switch
    {
        "Caption" or "Overline" => AuxiliaryTextMinimumPixels,
        "Body2" or "Subtitle2" or "Button" => SecondaryTextMinimumPixels,
        _ => PrimaryTextMinimumPixels
    };

    /// <summary>Returns the minimum font size in rem units for a typography role (16px root).</summary>
    public static double GetTypographyRoleMinimumRem(string role) =>
        GetTypographyRoleMinimumPixels(role) / 16d;

    /// <summary>Human-readable WCAG tier label for a typography role minimum.</summary>
    public static string GetTypographyRoleMinimumLabel(string role) => role switch
    {
        "Caption" or "Overline" => "auxiliary (12px)",
        "Body2" or "Subtitle2" or "Button" => "secondary (14px)",
        _ => "primary (16px)"
    };
}
