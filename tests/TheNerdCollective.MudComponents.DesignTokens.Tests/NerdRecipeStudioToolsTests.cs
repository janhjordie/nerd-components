using TheNerdCollective.Brand.Tnc;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdRecipeStudioToolsTests
{
    [Fact]
    public void FindRecipeForToken_prefers_content_role()
    {
        var options = CreateTncOptions();

        var recipe = NerdRecipeStudioTools.FindRecipeForToken("chalk", options);

        Assert.NotNull(recipe);
        Assert.Equal("hero", recipe.Value.Key);
        Assert.Equal("navy", recipe.Value.Value.Surface);
        Assert.Equal("chalk", recipe.Value.Value.Content);
    }

    [Fact]
    public void FindRecipeForSurface_returns_first_matching_recipe()
    {
        var options = CreateTncOptions();

        var recipe = NerdRecipeStudioTools.FindRecipeForSurface("snow", options);

        Assert.NotNull(recipe);
        Assert.Equal("header", recipe.Value.Key);
        Assert.Equal("ink", recipe.Value.Value.Content);
    }

    [Fact]
    public void GetStudioOptions_only_lists_recipe_pairings()
    {
        var options = CreateTncOptions();

        var surfaces = NerdRecipeStudioTools.GetStudioSurfaceOptions(options);
        var contentsOnNavy = NerdRecipeStudioTools.GetStudioContentOptions("navy", options);
        var actions = NerdRecipeStudioTools.GetStudioActionOptions("navy", "chalk", options);

        Assert.Equal(["coral", "navy", "snow"], surfaces);
        Assert.Equal(["chalk", "coral"], contentsOnNavy);
        Assert.Equal(["coral"], actions);
    }

    [Fact]
    public void GetDefaultRecipe_prefers_hero()
    {
        var options = CreateTncOptions();

        var recipe = NerdRecipeStudioTools.GetDefaultRecipe(options);

        Assert.Equal("hero", recipe.Key);
        Assert.Equal("navy", recipe.Value.Surface);
    }

    private static NerdDesignTokenOptions CreateTncOptions()
    {
        var options = new NerdDesignTokenOptions();
        NerdTncDesignTokenPresets.Apply(options);
        options.PairingPolicy = new NerdTncPairingPolicy();
        return options;
    }
}
