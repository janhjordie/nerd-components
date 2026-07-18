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

        Assert.Contains("--dnf-color-skov-content: #002D26", css);
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
        Assert.Single(options.Recipes);
        Assert.Equal("kridt", options.Recipes["kridt-himmel"].Surface);
        Assert.Equal("skov", options.Recipes["kridt-himmel"].Content);
        Assert.Equal("himmel", options.Recipes["kridt-himmel"].Action);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        foreach (var tokenName in options.Colors.Keys)
        {
            Assert.Contains($"--dnf-color-{tokenName}:", css);
            Assert.Contains($".dnf-{tokenName}", css);
        }
        Assert.Contains(".dnf-recipe-kridt-himmel", css);
    }

    [Theory]
    [InlineData("#002D26", "#FDFAF3", "#002D26")]
    [InlineData("#E8E0D3", "#002D26", "#002D26")]
    [InlineData("#FE993F", "#002D26", "#002D26")]
    public void ContentText_uses_skov_on_light_and_brand_on_dark(
        string brand,
        string contrast,
        string expected) =>
        Assert.Equal(expected, NerdColorParser.ContentText(brand, contrast));
}
