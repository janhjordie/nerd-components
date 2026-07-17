using TheNerdCollective.MudComponents.DesignTokens;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public class MudBlazorDesignTokenCssGeneratorTests
{
    [Fact]
    public void Generate_emits_variables_and_mudblazor_component_variants()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf" }
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
