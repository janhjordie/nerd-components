using MudBlazor;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public static class MudThemeResponsiveTypographyExtensions
{
    public static MudTheme UseResponsiveTypography(
        this MudTheme theme,
        Action<ResponsiveTypographyOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(theme);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new ResponsiveTypographyOptions();
        configure(options);

        Set(options.Default, value => theme.Typography.Default.FontSize = value);
        Set(options.H1, value => theme.Typography.H1.FontSize = value);
        Set(options.H2, value => theme.Typography.H2.FontSize = value);
        Set(options.H3, value => theme.Typography.H3.FontSize = value);
        Set(options.H4, value => theme.Typography.H4.FontSize = value);
        Set(options.H5, value => theme.Typography.H5.FontSize = value);
        Set(options.H6, value => theme.Typography.H6.FontSize = value);
        Set(options.Subtitle1, value => theme.Typography.Subtitle1.FontSize = value);
        Set(options.Subtitle2, value => theme.Typography.Subtitle2.FontSize = value);
        Set(options.Body1, value => theme.Typography.Body1.FontSize = value);
        Set(options.Body2, value => theme.Typography.Body2.FontSize = value);
        Set(options.Button, value => theme.Typography.Button.FontSize = value);
        Set(options.Caption, value => theme.Typography.Caption.FontSize = value);
        Set(options.Overline, value => theme.Typography.Overline.FontSize = value);

        return theme;
    }

    private static void Set(string? value, Action<string> setter)
    {
        if (value is not null)
        {
            setter(value);
        }
    }
}
