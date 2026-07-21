using TheNerdCollective.Brand.Dnf;
using TheNerdCollective.MudComponents.DesignTokens;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public class NerdDnfDesignTokenPresetsTests
{
    [Fact]
    public void Apply_registers_all_dnf_brand_tokens()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf" };
        NerdDnfDesignTokenPresets.Apply(options);

        Assert.Equal(12, options.Colors.Count);
        Assert.Contains("skov", options.Colors.Keys);
        Assert.Contains("kridt", options.Colors.Keys);
        Assert.Contains("jord", options.Colors.Keys);
        Assert.Contains("graes", options.Colors.Keys);
    }

    [Fact]
    public void Apply_uses_kridt_and_skov_contrast_pairings()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf" };
        NerdDnfDesignTokenPresets.Apply(options);

        Assert.Equal(NerdDnfDesignTokenPresets.KridtText, options.Colors["skov"].ContrastText);
        Assert.Equal(NerdDnfDesignTokenPresets.SkovText, options.Colors["kridt"].ContrastText);
    }

    [Fact]
    public void Generate_maps_dark_token_content_to_brand_color()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf" };
        NerdDnfDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains("--dnf-color-skov-content: #FDFAF3", css);
        Assert.Contains("--dnf-color-skov-surface: #002D26", css);
        Assert.Contains("--dnf-color-kridt-content: #002D26", css);
    }

    [Fact]
    public void Dnf_preset_matches_the_12_token_baseline_and_recipe()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf" };
        NerdDnfDesignTokenPresets.Apply(options);

        Assert.Equal(
            [
                "blad", "flod", "graes", "hav", "himmel", "jord",
                "kridt", "kridt-lys", "ler", "morgenrode", "skov", "sol"
            ],
            options.Colors.Keys.OrderBy(name => name, StringComparer.Ordinal));
        Assert.Equal(13, options.Recipes.Count);
        Assert.Equal(17, options.Aliases.Count);
        Assert.Equal(2, options.Opacities.Count);
        Assert.Contains("watermark", options.Opacities.Keys);
        Assert.Contains("hero-overlay", options.Opacities.Keys);
        Assert.Contains("kridt-himmel", options.Recipes.Keys);
        Assert.Contains("hero", options.Recipes.Keys);
        Assert.Contains("sidebar", options.Recipes.Keys);
        Assert.Contains("hero-photo", options.Recipes.Keys);
        Assert.Contains("partner-row", options.Recipes.Keys);
        Assert.Contains("cta-strip", options.Recipes.Keys);
        Assert.Contains("link-card", options.Recipes.Keys);
        Assert.Contains("footer", options.Recipes.Keys);
        Assert.Equal("graes", options.Aliases["nav-item-active"]);
        Assert.Equal("kridt", options.Recipes["kridt-himmel"].Surface);
        Assert.Equal("skov", options.Recipes["kridt-himmel"].Content);
        Assert.Equal("graes", options.Recipes["kridt-himmel"].Action);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        foreach (var tokenName in options.Colors.Keys)
        {
            Assert.Contains($"--dnf-color-{tokenName}:", css);
            Assert.Contains($".dnf-{tokenName}", css);
        }
        Assert.Contains(".dnf-recipe-kridt-himmel", css);
        Assert.Contains(".dnf-recipe-hero", css);
        Assert.Contains(".dnf-recipe-footer", css);
        Assert.Contains(".dnf-recipe-sidebar .mud-nav-link:hover", css);
    }

    [Fact]
    public void Dnf_preset_passes_wcag_aa_accessibility_gate()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf" };
        NerdDnfDesignTokenPresets.Apply(options);

        var exception = Record.Exception(() => NerdDesignTokenTools.AssertAccessibilityCompliance(options));
        Assert.Null(exception);
    }

    [Fact]
    public void AssertAccessibilityCompliance_throws_for_failing_token()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf" };
        options.Add("bad", new NerdColorToken { Value = "#777777", ContrastText = "#888888" });

        var exception = Assert.Throws<InvalidOperationException>(
            () => NerdDesignTokenTools.AssertAccessibilityCompliance(options));

        Assert.Contains("accessibility gate failed", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("#002D26", "#FDFAF3", "#002D26")]
    [InlineData("#E8E0D3", "#002D26", "#002D26")]
    [InlineData("#FF5E63", "#002D26", "#002D26")]
    public void ContentText_uses_skov_on_light_and_brand_on_dark(
        string brand,
        string contrast,
        string expected) =>
        Assert.Equal(expected, NerdColorParser.ContentText(brand, contrast));
}
