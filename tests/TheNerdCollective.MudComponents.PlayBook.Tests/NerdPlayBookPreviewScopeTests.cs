using TheNerdCollective.Brand.Tnc;
using TheNerdCollective.MudComponents.DesignTokens;
using Xunit;

namespace TheNerdCollective.MudComponents.PlayBook.Tests;

public sealed class NerdPlayBookPreviewScopeTests
{
    [Fact]
    public void Chalk_palette_preview_uses_navy_backdrop()
    {
        var options = new NerdDesignTokenOptions();
        NerdTncDesignTokenPresets.Apply(options);
        options.PairingPolicy = new NerdTncPairingPolicy();

        Assert.True(NerdPlayBookPreviewScope.NeedsContrastBackdrop(options, "tnc-chalk"));
        Assert.Equal("tnc-navy", NerdPlayBookPreviewScope.ResolveBackdropClass(options, "tnc-chalk"));
    }

    [Fact]
    public void Primary_action_intent_does_not_need_backdrop()
    {
        var options = new NerdDesignTokenOptions();
        NerdTncDesignTokenPresets.Apply(options);

        Assert.False(NerdPlayBookPreviewScope.NeedsContrastBackdrop(options, "tnc-primary-action"));
    }
}
