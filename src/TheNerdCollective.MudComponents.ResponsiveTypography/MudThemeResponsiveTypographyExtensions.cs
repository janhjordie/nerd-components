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

        Set(options.Default, value => theme.Typography.Default.FontSize = value, theme.Typography.Default, options);
        Set(options.H1, value => theme.Typography.H1.FontSize = value, theme.Typography.H1, options);
        Set(options.H2, value => theme.Typography.H2.FontSize = value, theme.Typography.H2, options);
        Set(options.H3, value => theme.Typography.H3.FontSize = value, theme.Typography.H3, options);
        Set(options.H4, value => theme.Typography.H4.FontSize = value, theme.Typography.H4, options);
        Set(options.H5, value => theme.Typography.H5.FontSize = value, theme.Typography.H5, options);
        Set(options.H6, value => theme.Typography.H6.FontSize = value, theme.Typography.H6, options);
        Set(options.Subtitle1, value => theme.Typography.Subtitle1.FontSize = value, theme.Typography.Subtitle1, options);
        Set(options.Subtitle2, value => theme.Typography.Subtitle2.FontSize = value, theme.Typography.Subtitle2, options);
        Set(options.Body1, value => theme.Typography.Body1.FontSize = value, theme.Typography.Body1, options);
        Set(options.Body2, value => theme.Typography.Body2.FontSize = value, theme.Typography.Body2, options);
        Set(options.Button, value => theme.Typography.Button.FontSize = value, theme.Typography.Button, options);
        Set(options.Caption, value => theme.Typography.Caption.FontSize = value, theme.Typography.Caption, options);
        Set(options.Overline, value => theme.Typography.Overline.FontSize = value, theme.Typography.Overline, options);

        return theme;
    }

    private static void Set(
        string? value,
        Action<string> setFontSize,
        BaseTypography typography,
        ResponsiveTypographyOptions options)
    {
        if (value is null)
        {
            return;
        }

        setFontSize(value);
        if (options.LineHeight is not null)
        {
            typography.LineHeight = options.LineHeight;
        }

        if (options.LetterSpacing is not null)
        {
            typography.LetterSpacing = options.LetterSpacing;
        }

        if (options.FontWeight is not null)
        {
            typography.FontWeight = options.FontWeight;
        }
    }
}
