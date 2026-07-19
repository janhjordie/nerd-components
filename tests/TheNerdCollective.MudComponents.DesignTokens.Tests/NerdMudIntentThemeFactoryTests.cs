using MudBlazor.Utilities;
using TheNerdCollective.Brand.Tnc;
using TheNerdCollective.MudComponents.DesignTokens;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdMudIntentThemeFactoryTests
{
    [Fact]
    public void GetPseudoCssScope_uses_root_prefix_class()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc" };
        NerdTncDesignTokenPresets.Apply(options);

        var scope = NerdMudIntentPaletteMap.GetPseudoCssScope(options, NerdDesignSystemUi.PrimaryAction);

        Assert.Equal(":root .tnc-primary-action", scope);
    }

    [Fact]
    public void CreateIntentTheme_overrides_primary_for_primary_action()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc" };
        NerdTncDesignTokenPresets.Apply(options);
        var brand = NerdMudThemeFactory.Create(options);

        var intent = NerdMudIntentThemeFactory.CreateIntentTheme(
            options,
            NerdDesignSystemUi.PrimaryAction,
            brand,
            isDarkMode: false);

        Assert.Equal(new MudColor(NerdTncDesignTokenPresets.Coral), intent.PaletteLight.Primary);
    }

    [Fact]
    public void Generate_skips_intent_palette_css_when_pseudo_css_enabled()
    {
        var options = new NerdDesignTokenOptions
        {
            Prefix = "tnc",
            UseImportantOverrides = false,
            UseIntentPseudoCssThemes = true
        };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.DoesNotContain("--mud-palette-primary: var(--tnc-color-primary-action)", css);
        Assert.DoesNotContain("--mud-palette-surface: var(--tnc-color-page-surface)", css);
    }

    [Fact]
    public void Palette_first_aliases_do_not_emit_bulk_button_patterns()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.DoesNotContain(".tnc-muted-content[class*=\"mud-button-filled\"]", css);
        Assert.Contains(".tnc-muted-content {", css);
    }

    [Fact]
    public void Recipe_theme_scope_uses_recipe_prefix()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc" };
        NerdTncDesignTokenPresets.Apply(options);

        var scope = NerdMudRecipeThemeFactory.GetPseudoCssScope(options, NerdDesignSystemUi.SidebarRecipe);

        Assert.Equal(":root .tnc-recipe-sidebar", scope);
    }
}
