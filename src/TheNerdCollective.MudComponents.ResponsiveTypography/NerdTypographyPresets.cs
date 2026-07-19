namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public static class NerdTypographyPresets
{
    public static void ApplyMarketing(ResponsiveTypographyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Default = NerdWcagTypography.ToRem(1);
        options.H1 = NerdWcagTypography.Clamp(nameof(options.H1), 1.75, 3.5, 2.5);
        options.H2 = NerdWcagTypography.Clamp(nameof(options.H2), 1.5, 2.8, 2);
        options.H3 = NerdWcagTypography.Clamp(nameof(options.H3), 1.25, 2.2, 1.625);
        options.Body1 = NerdWcagTypography.ToRem(1);
        options.Body2 = NerdWcagTypography.ToRem(0.875);
        options.Button = NerdWcagTypography.ToRem(0.875);
        options.Caption = NerdWcagTypography.Clamp(nameof(options.Caption), 0.75, 0.7, 0.8125);
        options.FontWeight = "500";
        NerdWcagTypography.EnsureCompliance(options);
    }

    public static void ApplyDenseApp(ResponsiveTypographyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Default = NerdWcagTypography.ToRem(1);
        options.Body1 = NerdWcagTypography.ToRem(1);
        options.Body2 = NerdWcagTypography.ToRem(0.875);
        options.Caption = NerdWcagTypography.Clamp(nameof(options.Caption), 0.75, 0.6, 0.8125);
        options.Button = NerdWcagTypography.ToRem(0.875);
        options.FontWeight = "400";
        NerdWcagTypography.EnsureCompliance(options);
    }

    public static void ApplyEditorial(ResponsiveTypographyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Default = NerdWcagTypography.ToRem(1);
        options.H1 = NerdWcagTypography.Clamp(nameof(options.H1), 1.75, 3.5, 2.75);
        options.H2 = NerdWcagTypography.Clamp(nameof(options.H2), 1.5, 2.8, 2.125);
        options.H3 = NerdWcagTypography.Clamp(nameof(options.H3), 1.25, 2.2, 1.75);
        options.Body1 = NerdWcagTypography.Clamp(nameof(options.Body1), 1, 1.4, 1.125);
        options.Roles.Body1.LineHeight = "1.7";
        options.FontWeight = "450";
        NerdWcagTypography.EnsureCompliance(options);
    }

    public static void ApplyDashboard(ResponsiveTypographyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Default = NerdWcagTypography.ToRem(1);
        options.H1 = NerdWcagTypography.Clamp(nameof(options.H1), 1.5, 2.5, 2);
        options.H2 = NerdWcagTypography.Clamp(nameof(options.H2), 1.25, 2, 1.625);
        options.Body1 = NerdWcagTypography.ToRem(1);
        options.Body2 = NerdWcagTypography.ToRem(0.875);
        options.Caption = NerdWcagTypography.Clamp(nameof(options.Caption), 0.75, 0.6, 0.8125);
        options.Button = NerdWcagTypography.ToRem(0.875);
        options.FontWeight = "400";
        NerdWcagTypography.EnsureCompliance(options);
    }
}
