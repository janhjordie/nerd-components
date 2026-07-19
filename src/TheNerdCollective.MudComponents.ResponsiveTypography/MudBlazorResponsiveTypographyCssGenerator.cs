using System.Text;
using MudBlazor;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

/// <summary>Generates MudBlazor typography CSS variable overrides from responsive typography options.</summary>
public static class MudBlazorResponsiveTypographyCssGenerator
{
    public static string Generate(ResponsiveTypographyOptions options, bool useImportant = true)
    {
        ArgumentNullException.ThrowIfNull(options);
        var theme = new MudTheme();
        theme.UseResponsiveTypography(options);
        return Generate(theme.Typography, useImportant);
    }

    public static string Generate(Typography typography, bool useImportant = true)
    {
        ArgumentNullException.ThrowIfNull(typography);
        var important = useImportant ? " !important" : string.Empty;
        var css = new StringBuilder();
        css.AppendLine(":root {");

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

            css.AppendLine($".mud-typography-{slug} {{");
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
