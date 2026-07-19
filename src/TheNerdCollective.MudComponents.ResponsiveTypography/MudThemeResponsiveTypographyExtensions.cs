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

        foreach (var (role, _, selector) in MudBlazorTypographyRoleMap.Roles)
        {
            Apply(selector(theme.Typography), role, options);
        }

        return theme;
    }

    public static MudTheme UseResponsiveTypography(
        this MudTheme theme,
        ResponsiveTypographyOptions options)
    {
        ArgumentNullException.ThrowIfNull(theme);
        ArgumentNullException.ThrowIfNull(options);

        foreach (var (role, _, selector) in MudBlazorTypographyRoleMap.Roles)
        {
            Apply(selector(theme.Typography), role, options);
        }

        return theme;
    }

    internal static string? ResolveFontSize(ResponsiveTypographyOptions options, string role)
    {
        var explicitSize = GetFontSize(options, role);
        if (explicitSize is not null)
        {
            return explicitSize;
        }

        if (!string.Equals(role, nameof(ResponsiveTypographyOptions.Default), StringComparison.Ordinal) &&
            options.Default is not null)
        {
            return options.Default;
        }

        return null;
    }

    private static void Apply(BaseTypography typography, string role, ResponsiveTypographyOptions options)
    {
        var fontSize = ResolveFontSize(options, role);
        if (fontSize is not null)
        {
            typography.FontSize = fontSize;
        }

        var roleStyle = options.Roles.TryGet(role, out var style) ? style : null;

        if (roleStyle?.LineHeight is not null)
        {
            typography.LineHeight = roleStyle.LineHeight;
        }
        else if (options.LineHeight is not null)
        {
            typography.LineHeight = options.LineHeight;
        }

        if (roleStyle?.LetterSpacing is not null)
        {
            typography.LetterSpacing = roleStyle.LetterSpacing;
        }
        else if (options.LetterSpacing is not null)
        {
            typography.LetterSpacing = options.LetterSpacing;
        }

        if (roleStyle?.FontWeight is not null)
        {
            typography.FontWeight = roleStyle.FontWeight;
        }
        else if (options.FontWeight is not null)
        {
            typography.FontWeight = options.FontWeight;
        }
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
