using TheNerdCollective.Brand.Dnf;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdLayoutCompositionCatalogTests
{
    [Fact]
    public void Describe_dnf_preset_includes_hero_photo_with_overlay_class()
    {
        var options = new NerdDesignTokenOptions();
        NerdDnfDesignTokenPresets.Apply(options);

        var blocks = NerdLayoutCompositionCatalog.Describe(options);

        var heroPhoto = Assert.Single(blocks, block => block.RecipeName == "hero-photo");
        Assert.Equal("Hero with photo", heroPhoto.Title);
        Assert.Equal("dnf-opacity-hero-overlay", heroPhoto.OverlayClass);
    }

    [Fact]
    public void Stitch_export_includes_layout_compositions_and_opacity_tokens()
    {
        var options = new NerdDesignTokenOptions();
        NerdDnfDesignTokenPresets.Apply(options);

        var design = NerdDesignTokenTools.ExportStitchDesignMd(options);

        Assert.Contains("## Layout compositions", design);
        Assert.Contains("### Hero with photo", design);
        Assert.Contains("dnf-opacity-hero-overlay", design);
        Assert.Contains("## Opacity overlays", design);
        Assert.Contains("hero-overlay", design);
    }
}
