using MudBlazor;
using MudBlazor.Utilities;
using TheNerdCollective.Blazor.ThemeKit;
using TheNerdCollective.MudComponents.ResponsiveTypography;
using Xunit;

namespace TheNerdCollective.Blazor.ThemeKit.Tests;

public class MudThemeJsonSerializerTests
{
    [Fact]
    public void ResponsiveFontSize_Clamp_returns_css_clamp_expression()
    {
        Assert.Equal("clamp(1.75rem, 3vw, 2.5rem)", ResponsiveFontSize.Clamp("1.75rem", "3vw", "2.5rem"));
    }

    [Fact]
    public void ResponsiveTypography_applies_configured_roles_and_preserves_omitted_roles()
    {
        var theme = new MudTheme();
        var originalH2 = theme.Typography.H2.FontSize;

        theme.UseResponsiveTypography(options =>
        {
            options.H1 = ResponsiveFontSize.Clamp("2rem", "4vw", "4rem");
            options.H3 = ResponsiveFontSize.Clamp("1.75rem", "3vw", "2.5rem");
        });

        Assert.Equal("clamp(2rem, 4vw, 4rem)", theme.Typography.H1.FontSize);
        Assert.Equal("clamp(1.75rem, 3vw, 2.5rem)", theme.Typography.H3.FontSize);
        Assert.Equal(originalH2, theme.Typography.H2.FontSize);
    }

    [Fact]
    public void ExtractTokens_and_apply_roundtrip_preserves_primary()
    {
        var original = CreateSampleTheme();
        var tokens = MudThemeJsonSerializer.ExtractTokens(original);

        var clone = new MudTheme();
        clone.Typography.Default.FontFamily = ["Roboto"];
        MudThemeJsonSerializer.ApplyTokens(clone, tokens);

        Assert.Equal(
            original.PaletteLight.Primary.ToString(MudColorOutputFormats.Hex),
            clone.PaletteLight.Primary.ToString(MudColorOutputFormats.Hex));
    }

    [Fact]
    public void JsonFileMudThemeCatalog_loads_index_and_sandbox_theme()
    {
        var themesDir = Path.Combine(AppContext.BaseDirectory, "TestThemes");
        Directory.CreateDirectory(themesDir);

        File.WriteAllText(Path.Combine(themesDir, "themes.index.json"), """
            {
              "schemaVersion": "1.0",
              "defaultThemeId": "playbook-sandbox",
              "themes": [
                {
                  "id": "billetsalg-default",
                  "displayName": "Default",
                  "version": "1.0.0",
                  "updatedAt": "2026-06-16",
                  "file": "billetsalg-default.theme.json"
                },
                {
                  "id": "playbook-sandbox",
                  "displayName": "Sandbox",
                  "version": "0.1.0",
                  "updatedAt": "2026-06-16",
                  "file": "playbook-sandbox.theme.json"
                }
              ]
            }
            """);

        File.WriteAllText(Path.Combine(themesDir, "playbook-sandbox.theme.json"), """
            {
              "schemaVersion": "1.0",
              "id": "playbook-sandbox",
              "version": "0.1.0",
              "tokens": { "light.primary": "#112233" }
            }
            """);

        var fallback = new TestFallbackCatalog();
        var catalog = new JsonFileMudThemeCatalog(themesDir, fallback);

        Assert.Equal("playbook-sandbox", catalog.DefaultThemeId);
        Assert.Equal(2, catalog.All.Count);
        Assert.Equal("#112233", catalog.GetTheme("playbook-sandbox").PaletteLight.Primary.ToString(MudColorOutputFormats.Hex));

        try
        {
            Directory.Delete(themesDir, recursive: true);
        }
        catch
        {
            // Best-effort cleanup.
        }
    }

    [Fact]
    public void ExportThemeJson_includes_schema_and_tokens()
    {
        var theme = CreateSampleTheme();
        var json = MudThemeJsonSerializer.ExportThemeJson(theme, "test-id", "2.0.0", "Test", "2026-06-16");

        Assert.Contains("\"id\": \"test-id\"", json);
        Assert.Contains("\"version\": \"2.0.0\"", json);
        Assert.Contains("light.primary", json);
    }

    private static MudTheme CreateSampleTheme()
    {
        var theme = new MudTheme
        {
            PaletteLight = new PaletteLight { Primary = "#0B7285", Secondary = "#2AA198" },
            LayoutProperties = new LayoutProperties { DefaultBorderRadius = "6px" },
        };
        theme.Typography.Default.FontFamily = ["Roboto"];
        return theme;
    }

    private sealed class TestFallbackCatalog : IMudThemeCatalog
    {
        public string DefaultThemeId => "billetsalg-default";

        public IReadOnlyList<MudThemeDescriptor> All { get; } =
        [
            new MudThemeDescriptor("billetsalg-default", "Default", "1.0.0", "#0B7285"),
        ];

        public MudTheme GetTheme(string themeId) => CreateSampleTheme();
    }
}
