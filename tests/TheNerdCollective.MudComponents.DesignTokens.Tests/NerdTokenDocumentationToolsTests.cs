using TheNerdCollective.Brand.Tnc;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdTokenDocumentationToolsTests
{
    [Fact]
    public void Resolve_returns_color_usage_for_palette_token()
    {
        var options = new NerdDesignTokenOptions();
        NerdTncDesignTokenPresets.Apply(options);
        options.Add("brand-navy", new NerdColorToken
        {
            Value = "#001B3A",
            Description = "Primary brand navy"
        });

        var doc = NerdTokenDocumentationTools.Resolve(
            options,
            new NerdTokenTreeNavigation(NerdTokenTreeTargetKind.Color, "brand-navy"));

        Assert.NotNull(doc);
        Assert.Equal("Primary brand navy", doc.Summary);
        Assert.Contains("--tnc-color-brand-navy", doc.Usage);
    }

    [Fact]
    public void Resolve_returns_recipe_role_documentation()
    {
        var options = new NerdDesignTokenOptions();
        NerdTncDesignTokenPresets.Apply(options);
        var recipe = options.Recipes.Values.First();

        var doc = NerdTokenDocumentationTools.Resolve(
            options,
            new NerdTokenTreeNavigation(NerdTokenTreeTargetKind.RecipeRole, recipe.Surface));

        Assert.NotNull(doc);
        Assert.Equal(recipe.Surface, doc.Title);
    }
}
