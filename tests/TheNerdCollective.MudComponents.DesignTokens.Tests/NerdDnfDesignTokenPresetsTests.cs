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
