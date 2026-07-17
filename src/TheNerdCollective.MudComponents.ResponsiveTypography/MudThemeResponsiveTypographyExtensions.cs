using MudBlazor;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

/// <summary>
/// Applies responsive typography options to a <see cref="MudTheme"/>.
/// </summary>
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

        Apply(theme.Typography.Default, nameof(ResponsiveTypographyOptions.Default), options);
        Apply(theme.Typography.H1, nameof(ResponsiveTypographyOptions.H1), options);
        Apply(theme.Typography.H2, nameof(ResponsiveTypographyOptions.H2), options);
        Apply(theme.Typography.H3, nameof(ResponsiveTypographyOptions.H3), options);
        Apply(theme.Typography.H4, nameof(ResponsiveTypographyOptions.H4), options);
        Apply(theme.Typography.H5, nameof(ResponsiveTypographyOptions.H5), options);
        Apply(theme.Typography.H6, nameof(ResponsiveTypographyOptions.H6), options);
        Apply(theme.Typography.Subtitle1, nameof(ResponsiveTypographyOptions.Subtitle1), options);
        Apply(theme.Typography.Subtitle2, nameof(ResponsiveTypographyOptions.Subtitle2), options);
        Apply(theme.Typography.Body1, nameof(ResponsiveTypographyOptions.Body1), options);
        Apply(theme.Typography.Body2, nameof(ResponsiveTypographyOptions.Body2), options);
        Apply(theme.Typography.Button, nameof(ResponsiveTypographyOptions.Button), options);
        Apply(theme.Typography.Caption, nameof(ResponsiveTypographyOptions.Caption), options);
        Apply(theme.Typography.Overline, nameof(ResponsiveTypographyOptions.Overline), options);

        return theme;
    }

    private static void Apply(BaseTypography typography, string role, ResponsiveTypographyOptions options)
    {
        var fontSize = GetFontSize(options, role);
        if (fontSize is null)
        {
            return;
        }

        typography.FontSize = fontSize;

        var roleStyle = options.Roles.TryGet(role, out var style) ? style : null;
        typography.LineHeight = roleStyle?.LineHeight ?? options.LineHeight ?? typography.LineHeight;
        typography.LetterSpacing = roleStyle?.LetterSpacing ?? options.LetterSpacing ?? typography.LetterSpacing;
        typography.FontWeight = roleStyle?.FontWeight ?? options.FontWeight ?? typography.FontWeight;
    }

    private static string? GetFontSize(ResponsiveTypographyOptions options, string role) => role switch
    {
        nameof(ResponsiveTypographyOptions.Default) => options.Default,
        nameof(ResponsiveTypographyOptions.H1) => options.H1,
        nameof(ResponsiveTypographyOptions.H2) => options.H2,
        nameof(ResponsiveTypographyOptions.H3) => options.H3,
        nameof(ResponsiveTypographyOptions.H4) => options.H4,
        nameof(ResponsiveTypographyOptions.H5) => options.H5,
        nameof(ResponsiveTypographyOptions.H6) => options.H6,
        nameof(ResponsiveTypographyOptions.Subtitle1) => options.Subtitle1,
        nameof(ResponsiveTypographyOptions.Subtitle2) => options.Subtitle2,
        nameof(ResponsiveTypographyOptions.Body1) => options.Body1,
        nameof(ResponsiveTypographyOptions.Body2) => options.Body2,
        nameof(ResponsiveTypographyOptions.Button) => options.Button,
        nameof(ResponsiveTypographyOptions.Caption) => options.Caption,
        nameof(ResponsiveTypographyOptions.Overline) => options.Overline,
        _ => null
    };
}
