using TheNerdCollective.Brand.Dnf;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdCoreCssGeneratorTests
{
    [Fact]
    public void Generate_dnf_preset_emits_brand_root_and_intent_variables()
    {
        var options = new NerdDesignTokenOptions();
        NerdDnfDesignTokenPresets.Apply(options);

        var css = NerdCoreCssGenerator.Generate(options);

        Assert.Contains(".dnf-nerd-brand", css);
        Assert.Contains("--nerd-intent-primary-action-surface", css);
        Assert.Contains("--dnf-color-skov", css);
        Assert.DoesNotContain(".mud-button-filled", css);
    }

    [Fact]
    public void ExportCoreCss_matches_generator()
    {
        var options = new NerdDesignTokenOptions { Prefix = "test" };
        options.Add("ink", new NerdColorToken { Value = "#111111", ContrastText = "#FFFFFF" });

        Assert.Equal(NerdCoreCssGenerator.Generate(options), NerdDesignTokenTools.ExportCoreCss(options));
    }
}
