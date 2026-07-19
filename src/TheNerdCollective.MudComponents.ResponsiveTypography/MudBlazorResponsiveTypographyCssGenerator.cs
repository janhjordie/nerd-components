using System.Text;
using MudBlazor;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

/// <summary>Generates MudBlazor typography CSS variable overrides from responsive typography options.</summary>
public static class MudBlazorResponsiveTypographyCssGenerator
{
    public static string Generate(ResponsiveTypographyOptions options, bool useImportant = true, string? scopeSelector = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        var theme = new MudTheme();
        theme.UseResponsiveTypography(options);
        return Generate(theme.Typography, useImportant, scopeSelector);
    }

    public static string Generate(Typography typography, bool useImportant = true, string? scopeSelector = null)
    {
        ArgumentNullException.ThrowIfNull(typography);
        if (scopeSelector is not null &&
            (scopeSelector.Contains('{') || scopeSelector.Contains('}')))
        {
            throw new ArgumentException("Scope selector cannot contain CSS declarations.", nameof(scopeSelector));
        }

        var important = useImportant ? " !important" : string.Empty;
        var variableRoot = string.IsNullOrWhiteSpace(scopeSelector) ? ":root" : scopeSelector;
        var css = new StringBuilder();
        css.AppendLine($"{variableRoot} {{");

        foreach (var (_, slug, selector) in MudBlazorTypographyRoleMap.Roles)
        {
            var roleTypography = selector(typography);
            AppendVariable(css, slug, "size", roleTypography.FontSize);
            AppendVariable(css, slug, "lineheight", roleTypography.LineHeight);
            AppendVariable(css, slug, "letterspacing", roleTypography.LetterSpacing);
            AppendVariable(css, slug, "weight", roleTypography.FontWeight);
        }

        css.AppendLine("}");

        foreach (var (_, slug, selector) in MudBlazorTypographyRoleMap.Roles)
        {
            var roleTypography = selector(typography);
            if (string.IsNullOrWhiteSpace(roleTypography.FontSize))
            {
                continue;
            }

            var roleSelector = string.IsNullOrWhiteSpace(scopeSelector)
                ? $".mud-typography-{slug}"
                : $"{scopeSelector} .mud-typography-{slug}";
            css.AppendLine($"{roleSelector} {{");
            css.AppendLine($"  font-size: {roleTypography.FontSize}{important};");

            if (!string.IsNullOrWhiteSpace(roleTypography.LineHeight))
            {
                css.AppendLine($"  line-height: {roleTypography.LineHeight}{important};");
            }

            if (!string.IsNullOrWhiteSpace(roleTypography.LetterSpacing))
            {
                css.AppendLine($"  letter-spacing: {roleTypography.LetterSpacing}{important};");
            }

            if (!string.IsNullOrWhiteSpace(roleTypography.FontWeight))
            {
                css.AppendLine($"  font-weight: {roleTypography.FontWeight}{important};");
            }

            css.AppendLine("}");
        }

        return css.ToString();
    }

    private static void AppendVariable(StringBuilder css, string slug, string suffix, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        css.AppendLine($"  --mud-typography-{slug}-{suffix}: {value};");
    }
}
