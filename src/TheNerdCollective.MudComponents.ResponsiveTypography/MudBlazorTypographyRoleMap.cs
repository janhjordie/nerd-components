using MudBlazor;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

internal static class MudBlazorTypographyRoleMap
{
    internal static readonly IReadOnlyList<(string Role, string CssSlug, Func<Typography, BaseTypography> Selector)> Roles =
    [
        ("Default", "default", t => t.Default),
        ("H1", "h1", t => t.H1),
        ("H2", "h2", t => t.H2),
        ("H3", "h3", t => t.H3),
        ("H4", "h4", t => t.H4),
        ("H5", "h5", t => t.H5),
        ("H6", "h6", t => t.H6),
        ("Subtitle1", "subtitle1", t => t.Subtitle1),
        ("Subtitle2", "subtitle2", t => t.Subtitle2),
        ("Body1", "body1", t => t.Body1),
        ("Body2", "body2", t => t.Body2),
        ("Button", "button", t => t.Button),
        ("Caption", "caption", t => t.Caption),
        ("Overline", "overline", t => t.Overline)
    ];
}
