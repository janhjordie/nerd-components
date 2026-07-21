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

    [Fact]
    public void On_primary_action_intent_uses_primary_action_backdrop()
    {
        var options = new NerdDesignTokenOptions();
        NerdTncDesignTokenPresets.Apply(options);

        Assert.True(NerdPlayBookPreviewScope.NeedsContrastBackdrop(options, "tnc-on-primary-action"));
        Assert.Equal("tnc-primary-action", NerdPlayBookPreviewScope.ResolveBackdropClass(options, "tnc-on-primary-action"));
    }

    [Fact]
    public void On_brand_chrome_intent_uses_brand_chrome_backdrop()
    {
        var options = new NerdDesignTokenOptions();
        NerdTncDesignTokenPresets.Apply(options);

        Assert.True(NerdPlayBookPreviewScope.NeedsContrastBackdrop(options, "tnc-on-brand-chrome"));
        Assert.Equal("tnc-brand-chrome", NerdPlayBookPreviewScope.ResolveBackdropClass(options, "tnc-on-brand-chrome"));
        Assert.Null(NerdPlayBookPreviewScope.ResolveBackdropStyle(options, "tnc-on-brand-chrome"));
    }

    [Fact]
    public void On_primary_action_backdrop_paints_parent_surface()
    {
        var options = new NerdDesignTokenOptions();
        NerdTncDesignTokenPresets.Apply(options);

        Assert.Equal(
            "background-color: var(--tnc-color-primary-action); color: var(--tnc-color-on-primary-action);",
            NerdPlayBookPreviewScope.ResolveBackdropStyle(options, "tnc-on-primary-action"));
    }

    [Fact]
    public void Chalk_palette_backdrop_paints_navy_surface()
    {
        var options = new NerdDesignTokenOptions();
        NerdTncDesignTokenPresets.Apply(options);
        options.PairingPolicy = new NerdTncPairingPolicy();

        Assert.Equal(
            "background-color: var(--tnc-color-navy); color: var(--tnc-color-chalk);",
            NerdPlayBookPreviewScope.ResolveBackdropStyle(options, "tnc-chalk"));
    }
}
