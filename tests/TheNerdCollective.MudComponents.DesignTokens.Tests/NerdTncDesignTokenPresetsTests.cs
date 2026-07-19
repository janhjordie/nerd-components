using TheNerdCollective.Brand.Tnc;
using TheNerdCollective.MudComponents.DesignTokens;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdTncDesignTokenPresetsTests
{
    public NerdTncDesignTokenPresetsTests() => NerdBrandPackTestBootstrap.EnsureRegistered();
    [Fact]
    public void Apply_registers_tnc_brand_colors_and_recipes()
    {
        var options = new NerdDesignTokenOptions();
        NerdTncDesignTokenPresets.Apply(options);

        Assert.Equal("tnc", options.Prefix);
        Assert.Equal(NerdTncDesignTokenPresets.Navy, options.Colors["navy"].Value);
        Assert.Equal(NerdTncDesignTokenPresets.Coral, options.Colors["coral"].Value);
        Assert.Equal(NerdTncDesignTokenPresets.Snow, options.Colors["snow"].Value);
        Assert.Equal(NerdTncDesignTokenPresets.Ink, options.Colors["ink"].Value);
        Assert.Equal(NerdTncDesignTokenPresets.Snow, options.Colors["chalk"].Content);
        Assert.Equal("coral", options.Aliases["primary-action"]);
        Assert.Equal("navy", options.Aliases["brand-chrome"]);
        Assert.Equal("chalk", options.Recipes["hero"].Content);
        Assert.Equal("coral", options.Recipes["hero"].Action);
        Assert.Equal("snow", options.Recipes["header"].Surface);
        Assert.Equal("coral", options.Recipes["tagline"].Content);
    }

    [Fact]
    public void FromPreset_loads_tnc_pack()
    {
        var pack = NerdTokenPack.FromPreset("tnc", "tnc");

        Assert.Equal("tnc", pack.ClientId);
        Assert.Equal("tnc", pack.Prefix);
        Assert.Equal("1.0.0", pack.BrandIdentityVersion);
        Assert.Equal(5, pack.Colors.Count);
        Assert.Contains("hero", pack.Recipes.Keys);
        Assert.Contains("tagline", pack.Recipes.Keys);
        pack.Validate();
    }

    [Fact]
    public void Hero_recipe_uses_navy_surface_without_lightening()
    {
        var options = new NerdDesignTokenOptions();
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);
        var heroStart = css.IndexOf(".tnc-recipe-hero", StringComparison.Ordinal);
        var headerStart = css.IndexOf(".tnc-recipe-header", StringComparison.Ordinal);

        Assert.True(heroStart >= 0);
        Assert.True(headerStart >= 0);
        Assert.Contains(
            NerdTncDesignTokenPresets.Navy,
            css.AsSpan(heroStart, Math.Min(200, css.Length - heroStart)).ToString(),
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            NerdTncDesignTokenPresets.Snow,
            css.AsSpan(headerStart, Math.Min(200, css.Length - headerStart)).ToString(),
            StringComparison.OrdinalIgnoreCase);
    }
}
