using TheNerdCollective.Brand.Tnc;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdBootstrapDesignTokenCssGeneratorTests
{
    [Fact]
    public void AppendBootstrapBrandPalette_emits_bs_primary_from_intents()
    {
        var options = new NerdDesignTokenOptions();
        NerdTncDesignTokenPresets.Apply(options);

        var css = NerdDesignTokenMudTools.ExportMudCss(options);

        Assert.Contains(".tnc-bootstrap-brand", css);
        Assert.Contains("--bs-primary: var(--nerd-intent-primary-action-surface)", css);
        Assert.Contains("--bs-body-bg: var(--nerd-intent-page-surface-surface)", css);
    }
}
