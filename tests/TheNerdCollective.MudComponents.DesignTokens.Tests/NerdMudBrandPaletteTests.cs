using MudBlazor.Utilities;
using TheNerdCollective.Brand.Tnc;
using TheNerdCollective.MudComponents.DesignTokens;

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
