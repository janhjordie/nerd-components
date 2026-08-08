using TheNerdCollective.Brand.Tnc;
using TheNerdCollective.MudComponents.DesignTokens;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdMudThemeMappingToolsTests
{
    public NerdMudThemeMappingToolsTests() => NerdBrandPackTestBootstrap.EnsureRegistered();

    [Fact]
    public void BuildPaletteMappings_includes_primary_slot_with_alias_and_color()
    {
        var options = new NerdDesignTokenOptions();
        NerdTncDesignTokenPresets.Apply(options);

        var mappings = NerdMudThemeMappingTools.BuildPaletteMappings(options);
        var primary = Assert.Single(mappings, entry => entry.MudThemeProperty == "Palette.Primary");

        Assert.Equal(NerdDesignSystemUi.PrimaryAction, primary.BindingAlias);
        Assert.False(string.IsNullOrWhiteSpace(primary.ColorToken));
        Assert.Equal("--mud-palette-primary", primary.CssVariable);
        Assert.False(string.IsNullOrWhiteSpace(primary.LightValue));
        Assert.False(string.IsNullOrWhiteSpace(primary.DarkValue));
    }

    [Fact]
    public void BuildAll_covers_palette_layout_shadows_and_zindex_categories()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc" };
        options.Add("navy", new NerdColorToken { Value = "#0B1F33", ContrastText = "#FFFFFF" });
        options.Alias(NerdDesignSystemUi.PrimaryAction, "navy");
        options.AddRadius("default", "8px");
        options.AddShadow("sm", "0 1px 2px rgba(0,0,0,.2)");
        options.AddZIndex("drawer", "1200");
        options.AddSpacing("drawer-width", "260px");
        options.FrameworkDefaults = new NerdFrameworkDefaults
        {
            MudBlazor = new NerdMudBlazorFrameworkDefaults
            {
                Palette = NerdMudBrandPaletteMap.CreateConventionBindings()
            }
        };

        var mappings = NerdMudThemeMappingTools.BuildAll(options);

        Assert.Contains(mappings, entry => entry.Category == NerdMudThemeMappingTools.CategoryPalette);
        Assert.Contains(mappings, entry => entry.MudThemeProperty == "LayoutProperties.DefaultBorderRadius");
        Assert.Contains(mappings, entry => entry.MudThemeProperty == "LayoutProperties.DrawerWidthLeft");
        Assert.Contains(mappings, entry => entry.MudThemeProperty.StartsWith("Shadows.Elevation", StringComparison.Ordinal));
        Assert.Contains(mappings, entry => entry.MudThemeProperty == "ZIndex.Drawer");
    }

    [Fact]
    public void ResolveColorTokenName_walks_alias_chain()
    {
        var options = new NerdDesignTokenOptions();
        options.Add("coral", new NerdColorToken { Value = "#FF6B4A" });
        options.Alias("primary-action", "action");
        options.Alias("action", "coral");

        Assert.Equal("coral", NerdMudThemeMappingTools.ResolveColorTokenName(options, "primary-action"));
    }

    [Fact]
    public void ThemeCatalogRoute_defaults_to_nerd_theme()
    {
        var options = new NerdDesignTokenOptions();
        Assert.Equal("/nerd-theme", options.ThemeCatalogRoute);
    }
}
