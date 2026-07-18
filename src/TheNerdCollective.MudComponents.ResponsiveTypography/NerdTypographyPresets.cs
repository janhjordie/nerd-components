namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public static class NerdTypographyPresets
{
    public static void ApplyMarketing(ResponsiveTypographyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.H1 = ResponsiveFontSize.Clamp("2.5rem", "5vw", "4.5rem");
        options.H2 = ResponsiveFontSize.Clamp("2rem", "4vw", "3.5rem");
        options.Body1 = ResponsiveFontSize.Clamp("1rem", "2.2vw", "1.25rem");
        options.Roles.H1.LineHeight = "1.15";
        options.Roles.H2.LineHeight = "1.2";
        options.Roles.Body1.LineHeight = "1.6";
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

    public static void ApplyEditorial(ResponsiveTypographyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.H1 = ResponsiveFontSize.Clamp("2.25rem", "4.5vw", "4rem");
        options.H2 = ResponsiveFontSize.Clamp("1.875rem", "3.5vw", "3rem");
        options.Body1 = ResponsiveFontSize.Clamp("1.0625rem", "1.8vw", "1.375rem");
        options.Roles.H1.LineHeight = "1.1";
        options.Roles.H2.LineHeight = "1.2";
        options.Roles.Body1.LineHeight = "1.7";
        options.LineHeight = "1.6";
        options.FontWeight = "450";
    }

    public static void ApplyDashboard(ResponsiveTypographyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.H1 = ResponsiveFontSize.Clamp("1.75rem", "3vw", "2.75rem");
        options.H2 = ResponsiveFontSize.Clamp("1.5rem", "2.5vw", "2.25rem");
        options.Body1 = "1rem";
        options.Body2 = "0.875rem";
        options.Caption = "0.75rem";
        options.LineHeight = "1.4";
        options.FontWeight = "400";
    }
}
