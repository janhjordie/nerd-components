using MudBlazor.Utilities;
using TheNerdCollective.Brand.Tnc;
using TheNerdCollective.MudComponents.DesignTokens;
using TheNerdCollective.MudComponents.ResponsiveTypography;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdMudBrandPaletteTests
{
    [Fact]
    public void Generate_does_not_duplicate_global_palette_at_brand_root()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.DoesNotContain(".tnc-mud-brand, .mud-theme-provider {", css);
        Assert.DoesNotContain(".tnc-primary-action {  --mud-palette-secondary: var(--tnc-color-primary-action)", css);
    }

    [Fact]
    public void Generate_palette_first_does_not_flatten_all_channels_on_primary_action()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(".tnc-primary-action {", css);
        Assert.Contains("--mud-palette-primary: var(--tnc-color-primary-action)", css);
        Assert.DoesNotContain(".tnc-primary-action {  --mud-palette-secondary: var(--tnc-color-primary-action)", css);
    }

    [Fact]
    public void NerdMudThemeFactory_matches_tnc_primary_and_secondary()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc" };
        NerdTncDesignTokenPresets.Apply(options);

        var theme = NerdMudThemeFactory.Create(options);

        Assert.Equal(new MudColor(NerdTncDesignTokenPresets.Coral), theme.PaletteLight.Primary);
        Assert.Equal(new MudColor(NerdTncDesignTokenPresets.Navy), theme.PaletteLight.Secondary);
        Assert.Equal(new MudColor(NerdTncDesignTokenPresets.Snow), theme.PaletteLight.Surface);
        Assert.NotEqual(default(MudColor), theme.PaletteDark.Primary);
        Assert.NotEqual(default(MudColor), theme.PaletteDark.Background);
    }

    [Fact]
    public void NerdMudThemeFactory_maps_content_intents_to_paint_colors_for_dnf()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf" };
        TheNerdCollective.Brand.Dnf.NerdDnfDesignTokenPresets.Apply(options);

        var theme = NerdMudThemeFactory.Create(options);

        // on-brand-chrome → kridt (paint), not contrast-of-kridt (skov)
        Assert.Equal(new MudColor(TheNerdCollective.Brand.Dnf.NerdDnfDesignTokenPresets.Kridt), theme.PaletteLight.AppbarText);
        // brand-chrome → skov
        Assert.Equal(new MudColor(TheNerdCollective.Brand.Dnf.NerdDnfDesignTokenPresets.Skov), theme.PaletteLight.AppbarBackground);
        // nav-item → skov (paint), not content-on-skov (kridt)
        Assert.Equal(new MudColor(TheNerdCollective.Brand.Dnf.NerdDnfDesignTokenPresets.Skov), theme.PaletteLight.DrawerText);
        Assert.Equal(new MudColor(TheNerdCollective.Brand.Dnf.NerdDnfDesignTokenPresets.Kridt), theme.PaletteLight.DrawerBackground);
    }

    [Fact]
    public void NerdMudThemeFactory_applies_typography_and_layout_from_pack()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc" };
        NerdTncDesignTokenPresets.Apply(options);
        options.AddRadius("default", "12px");
        options.AddShadow("md", "0 4px 12px rgba(0,0,0,0.15)");
        options.AddSpacing("drawer-width", "280px");
        options.AddZIndex("modal", "1400");
        options.AddZIndex("tooltip", "1600");

        var typography = new NerdResponsiveTypographyOptions();
        typography.Typography.H3 = "2.5rem";

        var theme = NerdMudThemeFactory.Create(options, mudTheme => mudTheme.UseResponsiveTypography(typography.Typography));

        Assert.Equal("12px", theme.LayoutProperties.DefaultBorderRadius);
        Assert.Equal("280px", theme.LayoutProperties.DrawerWidthLeft);
        Assert.Equal("2.5rem", theme.Typography.H3.FontSize);
        Assert.Equal("0 4px 12px rgba(0,0,0,0.15)", theme.Shadows.Elevation[2]);
        Assert.Equal(1400, theme.ZIndex.Dialog);
        Assert.Equal(1600, theme.ZIndex.Tooltip);
    }

    [Fact]
    public void Palette_manifest_includes_all_semantic_keys()
    {
        Assert.Contains("--mud-palette-action-default", MudBlazorPaletteManifest.AllPaletteVariables);
        Assert.Contains("--mud-palette-primary-hover", MudBlazorPaletteManifest.AllPaletteVariables);
        Assert.True(MudBlazorPaletteManifest.AllPaletteVariables.Count() >= 80);
    }

    [Fact]
    public void Generate_secondary_action_overrides_secondary_palette_channel()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(".tnc-secondary-action {", css);
        Assert.Contains("--mud-palette-secondary: var(--tnc-color-secondary-action)", css);
        Assert.DoesNotContain(".tnc-secondary-action[class*=\"mud-button-filled\"]", css);
    }
}
