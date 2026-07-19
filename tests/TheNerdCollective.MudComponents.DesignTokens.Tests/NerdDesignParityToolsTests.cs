namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdDesignParityToolsTests
{
    [Fact]
    public void Evaluate_tnc_pack_has_high_parity_score()
    {
        var options = NerdEmbeddedBrandPack.FromBrandJson("tnc").TokenPack.ToOptions();
        var result = NerdDesignParityTools.Evaluate(options);

        Assert.True(result.Score >= 80, $"Expected >= 80, got {result.Score}");
        Assert.Contains(result.Metrics, metric => metric.Name == "Shell bindings");
        Assert.Contains(result.Metrics, metric => metric.Name == "Framework defaults");
    }

    [Fact]
    public void Evaluate_dnf_pack_includes_extended_recipes_metric()
    {
        var options = NerdEmbeddedBrandPack.FromBrandJson("dnf").TokenPack.ToOptions();
        var result = NerdDesignParityTools.Evaluate(options, includeExtendedRecipes: true);

        Assert.True(result.Score >= 70, $"Expected >= 70, got {result.Score}");
        var recipes = result.Metrics.First(metric => metric.Name == "Shell recipes");
        Assert.DoesNotContain("Missing recipes: sidebar", recipes.Detail, StringComparison.Ordinal);
    }
}
