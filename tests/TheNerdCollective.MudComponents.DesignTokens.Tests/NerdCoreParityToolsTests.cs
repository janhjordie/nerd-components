using TheNerdCollective.Brand.Dnf;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdCoreParityToolsTests
{
    [Fact]
    public void Evaluate_tnc_pack_has_high_core_parity_score()
    {
        var options = NerdEmbeddedBrandPack.FromBrandJson("tnc").TokenPack.ToOptions();
        var result = NerdCoreParityTools.Evaluate(options);

        Assert.True(result.Score >= 80, $"Expected >= 80, got {result.Score}");
        Assert.Contains(result.Metrics, metric => metric.Name == "Shell bindings");
        Assert.DoesNotContain(result.Metrics, metric => metric.Name == "Mud palette map");
    }

    [Fact]
    public void Evaluate_dnf_pack_includes_extended_recipes_metric()
    {
        var options = NerdEmbeddedBrandPack.FromBrandJson("dnf").TokenPack.ToOptions();
        var result = NerdCoreParityTools.Evaluate(options, includeExtendedRecipes: true);

        var recipes = result.Metrics.First(metric => metric.Name == "Shell recipes");
        Assert.DoesNotContain("Missing recipes: sidebar", recipes.Detail, StringComparison.Ordinal);
    }
}
