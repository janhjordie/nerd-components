using MudBlazor;
using MudBlazor.Utilities;
using TheNerdCollective.Blazor.ThemeKit;
using Xunit;

namespace TheNerdCollective.Blazor.ThemeKit.Tests;

public class MudThemeExportWriterTests
{
    [Theory]
    [InlineData("billetsalg-default", "BilletsalgDefaultTheme")]
    [InlineData("playbook-sandbox", "PlaybookSandboxTheme")]
    public void ToCatalogClassName_maps_theme_id(string themeId, string expectedClassName)
    {
        Assert.Equal(expectedClassName, MudThemeExportWriter.ToCatalogClassName(themeId));
    }

    [Fact]
    public void WriteCatalogThemeClass_matches_shared_ui_catalog_shape()
    {
        var theme = CreateBilletsalgDefaultTheme();
        var options = new MudThemeCatalogExportOptions(
            Id: "billetsalg-default",
            DisplayName: "BilletSalg Default",
            Version: "1.0.0",
            UpdatedAt: "2026-06-16",
            SourceNotes: "Baseline BilletSalg V2 shell theme");

        var export = MudThemeExportWriter.WriteCatalogThemeClass(theme, options);

        Assert.Contains("namespace SharedUI.Themes.Catalog;", export);
        Assert.Contains("public static class BilletsalgDefaultTheme", export);
        Assert.Contains("public const string Id = \"billetsalg-default\";", export);
        Assert.Contains("public const string DisplayName = \"BilletSalg Default\";", export);
        Assert.Contains("public const string Version = \"1.0.0\";", export);
        Assert.Contains("public static MudTheme CreateTheme()", export);
        Assert.Contains("var theme = new MudTheme", export);
        Assert.Contains("Primary = \"#0B7285\",", export);
        Assert.Contains("DefaultBorderRadius = \"6px\",", export);
        Assert.Contains("theme.Typography.Default.FontFamily = [\"Roboto\", \"Helvetica\", \"Arial\", \"sans-serif\"];", export);
        Assert.DoesNotContain("Typography = new Typography", export);
        Assert.DoesNotContain("public static MudTheme ExportedTheme", export);
        Assert.EndsWith("}" + Environment.NewLine, export);
    }

    [Fact]
    public void WriteThemeManifest_matches_shared_ui_manifest_shape()
    {
        var options = new MudThemeCatalogExportOptions(
            Id: "billetsalg-default",
            DisplayName: "BilletSalg Default",
            Version: "1.0.1",
            UpdatedAt: "2026-06-16",
            MudBlazorVersion: "9.5",
            SourceNotes: "Baseline BilletSalg V2 shell theme");

        var manifest = MudThemeExportWriter.WriteThemeManifest(options);

        Assert.Contains("\"id\": \"billetsalg-default\"", manifest);
        Assert.Contains("\"version\": \"1.0.1\"", manifest);
        Assert.Contains("\"displayName\": \"BilletSalg Default\"", manifest);
        Assert.Contains("\"mudBlazorVersion\": \"9.5\"", manifest);
        Assert.Contains("\"type\": \"custom\"", manifest);
        Assert.Contains("Baseline BilletSalg V2 shell theme", manifest);
        Assert.DoesNotContain("\"updatedAt\"", manifest);
    }

    [Fact]
    public void WriteProductionExport_includes_relative_paths()
    {
        var theme = CreateBilletsalgDefaultTheme();
        var options = new MudThemeCatalogExportOptions(
            Id: "billetsalg-default",
            DisplayName: "BilletSalg Default",
            Version: "1.0.0",
            UpdatedAt: "2026-06-16");

        var bundle = MudThemeExportWriter.WriteProductionExport(theme, options);

        Assert.Equal("BilletsalgDefaultTheme", bundle.ClassName);
        Assert.Equal(
            "src/SharedUI/Themes/catalog/billetsalg-default/BilletsalgDefaultTheme.cs",
            bundle.RelativeCsPath);
        Assert.Equal(
            "src/SharedUI/Themes/catalog/billetsalg-default/theme.manifest.json",
            bundle.RelativeManifestPath);
        Assert.NotEmpty(bundle.ThemeClassFile);
        Assert.NotEmpty(bundle.ThemeManifestFile);
    }

    private static MudTheme CreateBilletsalgDefaultTheme()
    {
        var theme = new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#0B7285",
                PrimaryContrastText = "#FFFFFF",
                Secondary = "#2AA198",
                SecondaryContrastText = "#FFFFFF",
                Tertiary = "#F59F00",
                TertiaryContrastText = "#FFFFFF",
                Background = "#FAFBFB",
                Surface = "#FFFFFF",
                AppbarBackground = "#FFFFFF",
                AppbarText = "#0B7285",
                DrawerBackground = "#FFFFFF",
                DrawerText = "#23303A",
                TextPrimary = "#222222",
            },
            PaletteDark = new PaletteDark
            {
                Primary = "#9FD356",
                PrimaryContrastText = "#000000",
                Secondary = "#A4D65E",
                SecondaryContrastText = "#000000",
                Tertiary = "#FFB74D",
                TertiaryContrastText = "#000000",
                Background = "#14181E",
                Surface = "#191D24",
                AppbarBackground = "#14181E",
                AppbarText = "#FFFFFF",
                DrawerBackground = "#14181E",
                DrawerText = "#E6E6E9",
                TextPrimary = "#FFFFFF",
                TextSecondary = "#B0B3B8",
                ActionDefault = "#FFFFFF",
                ActionDisabled = "#6C757D",
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "6px",
            },
        };

        theme.Typography.Default.FontFamily = ["Roboto", "Helvetica", "Arial", "sans-serif"];
        return theme;
    }
}
