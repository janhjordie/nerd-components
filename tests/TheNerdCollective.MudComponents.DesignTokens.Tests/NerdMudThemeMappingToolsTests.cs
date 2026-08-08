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
        Assert.Equal(NerdMudThemeMappingStatus.Mapped, primary.Status);
        Assert.False(string.IsNullOrWhiteSpace(primary.LightValue));
        Assert.False(string.IsNullOrWhiteSpace(primary.DarkValue));
    }

    [Fact]
    public void BuildAll_lists_complete_mud_theme_inventory_with_honest_coverage()
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

        var inventory = NerdMudThemeMappingTools.EnumerateMudThemeInventory();
        var mappings = NerdMudThemeMappingTools.BuildAll(options);
        var coverage = NerdMudThemeMappingTools.EvaluateCoverage(mappings);

        Assert.Equal(inventory.Count, mappings.Count);
        Assert.Equal(inventory.Count, coverage.Total);
        Assert.Equal(coverage.Covered + coverage.Unmapped, coverage.InScope);
        Assert.Equal(coverage.InScope + coverage.AcceptedDefault + coverage.External, coverage.Total);
        Assert.Equal(0, coverage.Unmapped);
        Assert.Equal(100, coverage.CoveredPercent);
        Assert.True(coverage.AcceptedDefault > 0);
        Assert.Equal(14 * 6, coverage.External);
        Assert.Contains(mappings, entry => entry.MudThemeProperty == "LayoutProperties.DefaultBorderRadius");
        Assert.Contains(mappings, entry => entry.MudThemeProperty == "LayoutProperties.DrawerWidthLeft");
        Assert.Contains(mappings, entry => entry.MudThemeProperty == "LayoutProperties.AppbarHeight" &&
                                           entry.Status == NerdMudThemeMappingStatus.AcceptedDefault);
        Assert.Contains(mappings, entry => entry.MudThemeProperty == "Shadows.Elevation[1]");
        Assert.Contains(mappings, entry => entry.MudThemeProperty == "Shadows.Elevation[25]" &&
                                           entry.Status == NerdMudThemeMappingStatus.AcceptedDefault);
        Assert.Contains(mappings, entry => entry.MudThemeProperty == "ZIndex.Drawer");
        Assert.Contains(mappings, entry => entry.MudThemeProperty == "Typography.H1.FontSize" &&
                                           entry.Status == NerdMudThemeMappingStatus.External);
        Assert.Contains(mappings, entry => entry.MudThemeProperty == "PseudoCss.Scope" &&
                                           entry.Status == NerdMudThemeMappingStatus.AcceptedDefault);
        Assert.Contains(mappings, entry => entry.MudThemeProperty == "Palette.PrimaryDarken" &&
                                           entry.Status == NerdMudThemeMappingStatus.Derived);
        Assert.Contains(mappings, entry => entry.MudThemeProperty == "Palette.RippleOpacity" &&
                                           entry.Status == NerdMudThemeMappingStatus.AcceptedDefault);
    }

    [Fact]
    public void EvaluateCoverage_excludes_accepted_and_external_from_percent()
    {
        var entries = new[]
        {
            new NerdMudThemeMappingEntry("Palette", "Palette.Primary", "", null, null, "#000", "#000", "Color", NerdMudThemeMappingStatus.Mapped, ""),
            new NerdMudThemeMappingEntry("Typography", "Typography.H1.FontSize", "", null, null, "2rem", "2rem", "External", NerdMudThemeMappingStatus.External, ""),
            new NerdMudThemeMappingEntry("Shadows", "Shadows.Elevation[25]", "", null, null, "none", "none", "AcceptedDefault", NerdMudThemeMappingStatus.AcceptedDefault, ""),
            new NerdMudThemeMappingEntry("Layout", "LayoutProperties.AppbarHeight", "", null, null, "64px", "64px", "Unmapped", NerdMudThemeMappingStatus.Unmapped, "")
        };

        var coverage = NerdMudThemeMappingTools.EvaluateCoverage(entries);

        Assert.Equal(4, coverage.Total);
        Assert.Equal(1, coverage.Mapped);
        Assert.Equal(1, coverage.Unmapped);
        Assert.Equal(1, coverage.AcceptedDefault);
        Assert.Equal(1, coverage.External);
        Assert.Equal(2, coverage.InScope);
        Assert.Equal(50, coverage.CoveredPercent);
    }

    [Fact]
    public void EnumerateMudThemeInventory_matches_mudblazor_palette_surface()
    {
        var inventory = NerdMudThemeMappingTools.EnumerateMudThemeInventory();
        var paletteCount = typeof(MudBlazor.Palette)
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Length;

        Assert.Equal(paletteCount, inventory.Count(slot => slot.Category == NerdMudThemeMappingTools.CategoryPalette));
        Assert.Equal(26, inventory.Count(slot => slot.Category == NerdMudThemeMappingTools.CategoryShadows));
        Assert.Equal(14 * 6, inventory.Count(slot => slot.Category == NerdMudThemeMappingTools.CategoryTypography));
        Assert.Contains(inventory, slot => slot.Property == "PseudoCss.Scope");
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
