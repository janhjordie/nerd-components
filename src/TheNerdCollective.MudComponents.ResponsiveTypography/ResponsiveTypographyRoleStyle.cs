namespace TheNerdCollective.MudComponents.ResponsiveTypography;

/// <summary>
/// Optional per-role typography spacing and weight overrides.
/// </summary>
public sealed class ResponsiveTypographyRoleStyle
{
    public string? LineHeight { get; set; }
    public string? LetterSpacing { get; set; }
    public string? FontWeight { get; set; }
}

/// <summary>
/// Per-role typography style settings keyed by MudBlazor role name.
/// </summary>
public sealed class ResponsiveTypographyRoleStyles
{
    private readonly Dictionary<string, ResponsiveTypographyRoleStyle> _roles = new(StringComparer.OrdinalIgnoreCase);

    public ResponsiveTypographyRoleStyle Default => For(nameof(Default));
    public ResponsiveTypographyRoleStyle H1 => For(nameof(H1));
    public ResponsiveTypographyRoleStyle H2 => For(nameof(H2));
    public ResponsiveTypographyRoleStyle H3 => For(nameof(H3));
    public ResponsiveTypographyRoleStyle H4 => For(nameof(H4));
    public ResponsiveTypographyRoleStyle H5 => For(nameof(H5));
    public ResponsiveTypographyRoleStyle H6 => For(nameof(H6));
    public ResponsiveTypographyRoleStyle Subtitle1 => For(nameof(Subtitle1));
    public ResponsiveTypographyRoleStyle Subtitle2 => For(nameof(Subtitle2));
    public ResponsiveTypographyRoleStyle Body1 => For(nameof(Body1));
    public ResponsiveTypographyRoleStyle Body2 => For(nameof(Body2));
    public ResponsiveTypographyRoleStyle Button => For(nameof(Button));
    public ResponsiveTypographyRoleStyle Caption => For(nameof(Caption));
    public ResponsiveTypographyRoleStyle Overline => For(nameof(Overline));

    public ResponsiveTypographyRoleStyle For(string role)
    {
        if (!_roles.TryGetValue(role, out var style))
        {
            style = new ResponsiveTypographyRoleStyle();
            _roles[role] = style;
        }

        return style;
    }

    internal bool TryGet(string role, out ResponsiveTypographyRoleStyle style) =>
        _roles.TryGetValue(role, out style!);

    internal void CopyFrom(ResponsiveTypographyRoleStyles source)
    {
        ArgumentNullException.ThrowIfNull(source);
        foreach (var role in RoleNames)
        {
            if (!source.TryGet(role, out var style))
            {
                continue;
            }

            var target = For(role);
            target.LineHeight = style.LineHeight;
            target.LetterSpacing = style.LetterSpacing;
            target.FontWeight = style.FontWeight;
        }
    }

    internal static readonly string[] RoleNames =
    [
        nameof(Default), nameof(H1), nameof(H2), nameof(H3), nameof(H4), nameof(H5), nameof(H6),
        nameof(Subtitle1), nameof(Subtitle2), nameof(Body1), nameof(Body2),
        nameof(Button), nameof(Caption), nameof(Overline)
    ];
}
