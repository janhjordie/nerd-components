using System.Text;
using System.Text.Json;
using MudBlazor;
using MudBlazor.Utilities;

namespace TheNerdCollective.Blazor.ThemeKit;

public static class MudThemeExportWriter
{
    private static readonly JsonSerializerOptions ManifestJsonOptions = new()
    {
        WriteIndented = true,
    };

    public static string ToCatalogClassName(string themeId)
    {
        var parts = themeId.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            return "CustomTheme";
        }

        return string.Concat(parts.Select(ToPascalCaseSegment)) + "Theme";
    }

    public static MudThemeProductionExport WriteProductionExport(MudTheme theme, MudThemeCatalogExportOptions options)
    {
        var className = options.ClassName ?? ToCatalogClassName(options.Id);
        var folderName = options.Id;
        return new MudThemeProductionExport(
            ThemeClassFile: WriteCatalogThemeClass(theme, options with { ClassName = className }),
            ThemeManifestFile: WriteThemeManifest(options),
            Version: options.Version,
            UpdatedAt: options.UpdatedAt,
            ClassName: className,
            RelativeCsPath: $"src/SharedUI/Themes/catalog/{folderName}/{className}.cs",
            RelativeManifestPath: $"src/SharedUI/Themes/catalog/{folderName}/theme.manifest.json");
    }

    public static string WriteCatalogThemeClass(MudTheme theme, MudThemeCatalogExportOptions options)
    {
        var className = options.ClassName ?? ToCatalogClassName(options.Id);
        var builder = new StringBuilder();

        builder.AppendLine("using MudBlazor;");
        builder.AppendLine();
        builder.AppendLine($"namespace {options.Namespace};");
        builder.AppendLine();
        builder.AppendLine($"public static class {className}");
        builder.AppendLine("{");
        builder.AppendLine($"    public const string Id = \"{EscapeString(options.Id)}\";");
        builder.AppendLine($"    public const string DisplayName = \"{EscapeString(options.DisplayName)}\";");
        builder.AppendLine($"    public const string Version = \"{EscapeString(options.Version)}\";");
        builder.AppendLine();
        builder.AppendLine("    public static MudTheme CreateTheme()");
        builder.AppendLine("    {");
        builder.AppendLine("        var theme = new MudTheme");
        builder.AppendLine("        {");

        AppendPaletteLight(builder, theme.PaletteLight);
        AppendPaletteDark(builder, theme.PaletteDark);
        AppendLayout(builder, theme.LayoutProperties);

        builder.AppendLine("        };");
        builder.AppendLine();

        AppendTypographyAssignment(builder, theme.Typography);

        builder.AppendLine("        return theme;");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        return builder.ToString();
    }

    public static string WriteThemeManifest(MudThemeCatalogExportOptions options)
    {
        var manifest = new Dictionary<string, object?>
        {
            ["id"] = options.Id,
            ["version"] = options.Version,
            ["displayName"] = options.DisplayName,
            ["mudBlazorVersion"] = options.MudBlazorVersion,
            ["source"] = new Dictionary<string, object?>
            {
                ["type"] = "custom",
                ["notes"] = options.SourceNotes ?? $"ThemeKit export {options.UpdatedAt}",
            },
        };

        return JsonSerializer.Serialize(manifest, ManifestJsonOptions) + Environment.NewLine;
    }

    private static void AppendPaletteLight(StringBuilder builder, Palette palette)
    {
        builder.AppendLine("            PaletteLight = new PaletteLight");
        builder.AppendLine("            {");
        AppendColor(builder, "Primary", palette.Primary, indent: 16);
        AppendColor(builder, "PrimaryContrastText", palette.PrimaryContrastText, indent: 16);
        AppendColor(builder, "Secondary", palette.Secondary, indent: 16);
        AppendColor(builder, "SecondaryContrastText", palette.SecondaryContrastText, indent: 16);
        AppendColor(builder, "Tertiary", palette.Tertiary, indent: 16);
        AppendColor(builder, "TertiaryContrastText", palette.TertiaryContrastText, indent: 16);
        AppendColor(builder, "Background", palette.Background, indent: 16);
        AppendColor(builder, "Surface", palette.Surface, indent: 16);
        AppendColor(builder, "AppbarBackground", palette.AppbarBackground, indent: 16);
        AppendColor(builder, "AppbarText", palette.AppbarText, indent: 16);
        AppendColor(builder, "DrawerBackground", palette.DrawerBackground, indent: 16);
        AppendColor(builder, "DrawerText", palette.DrawerText, indent: 16);
        AppendColor(builder, "TextPrimary", palette.TextPrimary, indent: 16);
        builder.AppendLine("            },");
    }

    private static void AppendPaletteDark(StringBuilder builder, Palette palette)
    {
        builder.AppendLine("            PaletteDark = new PaletteDark");
        builder.AppendLine("            {");
        AppendColor(builder, "Primary", palette.Primary, indent: 16);
        AppendColor(builder, "PrimaryContrastText", palette.PrimaryContrastText, indent: 16);
        AppendColor(builder, "Secondary", palette.Secondary, indent: 16);
        AppendColor(builder, "SecondaryContrastText", palette.SecondaryContrastText, indent: 16);
        AppendColor(builder, "Tertiary", palette.Tertiary, indent: 16);
        AppendColor(builder, "TertiaryContrastText", palette.TertiaryContrastText, indent: 16);
        AppendColor(builder, "Background", palette.Background, indent: 16);
        AppendColor(builder, "Surface", palette.Surface, indent: 16);
        AppendColor(builder, "AppbarBackground", palette.AppbarBackground, indent: 16);
        AppendColor(builder, "AppbarText", palette.AppbarText, indent: 16);
        AppendColor(builder, "DrawerBackground", palette.DrawerBackground, indent: 16);
        AppendColor(builder, "DrawerText", palette.DrawerText, indent: 16);
        AppendColor(builder, "TextPrimary", palette.TextPrimary, indent: 16);
        AppendColor(builder, "TextSecondary", palette.TextSecondary, indent: 16);
        AppendColor(builder, "ActionDefault", palette.ActionDefault, indent: 16);
        AppendColor(builder, "ActionDisabled", palette.ActionDisabled, indent: 16);
        builder.AppendLine("            },");
    }

    private static void AppendLayout(StringBuilder builder, LayoutProperties layout)
    {
        builder.AppendLine("            LayoutProperties = new LayoutProperties");
        builder.AppendLine("            {");
        AppendText(builder, "DefaultBorderRadius", layout.DefaultBorderRadius, indent: 16);
        builder.AppendLine("            },");
    }

    private static void AppendTypographyAssignment(StringBuilder builder, Typography typography)
    {
        var fontFamily = typography.Default.FontFamily;
        if (fontFamily is not { Length: > 0 })
        {
            return;
        }

        builder.Append("        theme.Typography.Default.FontFamily = [");
        builder.Append(string.Join(", ", fontFamily.Select(f => $"\"{EscapeString(f)}\"")));
        builder.AppendLine("];");
    }

    private static void AppendColor(StringBuilder builder, string name, MudColor color, int indent)
    {
        var hex = color.ToString(MudColorOutputFormats.Hex);
        if (string.IsNullOrWhiteSpace(hex))
        {
            return;
        }

        builder.AppendLine($"{new string(' ', indent)}{name} = \"{hex.ToUpperInvariant()}\",");
    }

    private static void AppendText(StringBuilder builder, string name, string? value, int indent)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        builder.AppendLine($"{new string(' ', indent)}{name} = \"{EscapeString(value)}\",");
    }

    private static string ToPascalCaseSegment(string segment)
    {
        if (string.IsNullOrWhiteSpace(segment))
        {
            return string.Empty;
        }

        return char.ToUpperInvariant(segment[0]) + segment[1..];
    }

    private static string EscapeString(string value)
        => value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
}
