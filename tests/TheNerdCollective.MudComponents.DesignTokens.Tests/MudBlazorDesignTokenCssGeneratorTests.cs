using TheNerdCollective.MudComponents.DesignTokens;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public class MudBlazorDesignTokenCssGeneratorTests
{
    [Fact]
    public void Generate_emits_variables_and_mudblazor_component_variants()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf", UseCssLayer = true }
            .Add("forest", new NerdColorToken
            {
                Value = "#365C3A",
                ContrastText = "#FFFFFF",
                Hover = "#2D4D30"
            });

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(".dnf-forest", css);
        Assert.Contains("--dnf-color-forest: #365C3A", css);
        Assert.Contains(".dnf-forest.mud-button-filled", css);
        Assert.Contains(".dnf-forest.mud-button-outlined", css);
        Assert.Contains(".dnf-forest.mud-button-text", css);
        Assert.Contains(".dnf-forest.mud-typography", css);
        Assert.Contains(".dnf-forest.mud-disabled", css);
        Assert.Contains("!important", css);
    }

    [Fact]
    public void Generate_supports_customer_specific_token_sets()
    {
        var options = new NerdDesignTokenOptions { Prefix = "kunde" }
            .Add("sand", new NerdColorToken { Value = "#E8D8AD", ContrastText = "#2D2D2D" })
            .Add("sea-2", new NerdColorToken { Value = "#287A9E", ContrastText = "#FFFFFF" });

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(".kunde-sand", css);
        Assert.Contains(".kunde-sea-2", css);
        Assert.DoesNotContain(".kunde-forest", css);
    }

    [Fact]
    public void Generate_emits_dark_mode_and_semantic_role_variables()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf" }
            .Add("forest", new NerdColorToken
            {
                Value = "#365C3A",
                Light = "#4D7A50",
                Dark = "#203B25",
                Surface = "#F0F7F0",
                Content = "#19301D",
                Interactive = "#2D4D30"
            });

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains("[data-theme=\"dark\"] .dnf-forest", css);
        Assert.Contains("--dnf-color-forest-surface: #F0F7F0", css);
        Assert.Contains("--dnf-color-forest-content: #19301D", css);
        Assert.Contains("--dnf-color-forest-interactive: #2D4D30", css);
        Assert.Contains(".dnf-forest.mud-selected", css);
        Assert.Contains("[aria-pressed=\"true\"]", css);
    }

    [Fact]
    public void Generate_computes_contrast_text_when_omitted()
    {
        var options = new NerdDesignTokenOptions { Prefix = "test" }
            .Add("sun", new NerdColorToken { Value = "#FFFFFF" });

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains("--test-color-sun-text: #1F2937", css);
    }

    [Fact]
    public void Generate_isolates_css_and_emits_design_system_helpers()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf", UseCssLayer = true }
            .Add("forest", new NerdColorToken { Value = "#365C3A" })
            .Alias("primary-action", "forest")
            .AddRadius("card", "12px")
            .AddShadow("elevated", "0 4px 16px rgba(0,0,0,.16)");

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains("@layer nerd-design-tokens", css);
        Assert.Contains(".dnf-primary-action", css);
        Assert.Contains(".dnf-radius-card", css);
        Assert.Contains(".dnf-shadow-elevated", css);
    }

    [Fact]
    public void Accessibility_check_reports_wcag_aa_status()
    {
        var options = new NerdDesignTokenOptions { Prefix = "test" }
            .Add("light", new NerdColorToken { Value = "#FFFFFF", ContrastText = "#000000" });

        var result = NerdDesignTokenTools.CheckAccessibility(options).Single();

        Assert.True(result.MeetsAa);
        Assert.True(result.Light.ContrastRatio >= 4.5);
        Assert.Equal("2.1", result.WcagVersion);
    }

    [Fact]
    public void Accessibility_warnings_include_mode_ratio_and_recommendation()
    {
        var options = new NerdDesignTokenOptions { Prefix = "test", WcagVersion = "2.1" }
            .Add("sand", new NerdColorToken { Value = "#E8D8AD", ContrastText = "#D8C58E" });

        var warnings = NerdDesignTokenTools.GetAccessibilityWarnings(options);

        Assert.NotEmpty(warnings);
        Assert.Contains(warnings, warning => warning.TokenName == "sand");
        Assert.Contains(warnings, warning => warning.Mode is "light" or "dark");
        Assert.False(string.IsNullOrWhiteSpace(warnings[0].RecommendedForeground));
    }

    [Fact]
    public void Stitch_export_contains_customer_tokens()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf" }
            .Add("forest", new NerdColorToken { Value = "#365C3A" });

        var design = NerdDesignTokenTools.ExportStitchDesignMd(options);

        Assert.Contains("# Design tokens", design);
        Assert.Contains("forest", design);
        Assert.Contains("#365C3A", design);
    }

    [Fact]
    public void Catalog_options_have_sensible_defaults()
    {
        var options = new NerdDesignTokenOptions();

        Assert.True(options.EnableCatalogPage);
        Assert.Equal("/nerd-design-tokens", options.CatalogRoute);
        Assert.True(options.RestrictCatalogToDevelopment);
    }

    [Theory]
    [InlineData("Sand")]
    [InlineData("sand color")]
    [InlineData("sand;")]
    public void Add_rejects_invalid_token_names(string name)
    {
        var options = new NerdDesignTokenOptions();

        Assert.Throws<ArgumentException>(() =>
            options.Add(name, new NerdColorToken { Value = "red", ContrastText = "white" }));
    }
}
