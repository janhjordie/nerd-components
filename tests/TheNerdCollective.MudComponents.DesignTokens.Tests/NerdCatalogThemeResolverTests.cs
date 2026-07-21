using TheNerdCollective.Brand.Tnc;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdCatalogThemeResolverTests
{
    [Fact]
    public void CreateForCatalog_uses_controller_theme_when_present()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc" };
        NerdTncDesignTokenPresets.Apply(options);
        var tokenCss = new NerdDesignTokenCss(string.Empty);
        var controller = new NerdMudThemeController(options, tokenCss, new NerdDesignSystemOptions());

        var theme = NerdCatalogThemeResolver.CreateForCatalog(options, controller);

        Assert.Same(controller.CurrentTheme, theme);
    }

    [Fact]
    public void CreateForCatalog_builds_brand_theme_when_standalone()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc" };
        NerdTncDesignTokenPresets.Apply(options);
        NerdFoundationTaxonomyTools.ApplyDefaults(options);

        var theme = NerdCatalogThemeResolver.CreateForCatalog(
            options,
            themeController: null,
            configure: theme => theme.LayoutProperties.DefaultBorderRadius = "12px");

        Assert.Equal("12px", theme.LayoutProperties.DefaultBorderRadius);
        Assert.NotNull(theme.PaletteLight.Primary);
    }
}
