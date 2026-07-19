using TheNerdCollective.Brand.Tnc;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdBlazoriseDesignTokenCssGeneratorTests
{
    [Fact]
    public void AppendBlazoriseBrandPalette_emits_bootstrap_and_blazorise_roots()
    {
        var options = new NerdDesignTokenOptions();
        NerdTncDesignTokenPresets.Apply(options);

        var css = NerdDesignTokenMudTools.ExportMudCss(options);

        Assert.Contains(".tnc-blazorise-brand", css);
        Assert.Contains(".tnc-bootstrap-brand", css);
        Assert.Contains("--bs-primary: var(--nerd-intent-primary-action-surface)", css);
    }
}
