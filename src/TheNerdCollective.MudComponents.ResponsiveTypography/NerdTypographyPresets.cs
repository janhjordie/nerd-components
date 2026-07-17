namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public static class NerdTypographyPresets
{
    public static void ApplyMarketing(ResponsiveTypographyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.H1 = ResponsiveFontSize.Clamp("2.5rem", "5vw", "4.5rem");
        options.H2 = ResponsiveFontSize.Clamp("2rem", "4vw", "3.5rem");
        options.Body1 = ResponsiveFontSize.Clamp("1rem", "2.2vw", "1.25rem");
        options.LineHeight = "1.5";
        options.LetterSpacing = "0.01em";
        options.FontWeight = "500";
    }

    public static void ApplyDenseApp(ResponsiveTypographyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Body1 = ResponsiveFontSize.Clamp("0.875rem", "1.6vw", "1rem");
        options.Body2 = ResponsiveFontSize.Clamp("0.8125rem", "1.4vw", "0.9375rem");
        options.Caption = ResponsiveFontSize.Clamp("0.75rem", "1.2vw", "0.875rem");
        options.LineHeight = "1.35";
        options.LetterSpacing = "0.02em";
        options.FontWeight = "400";
    }
}
