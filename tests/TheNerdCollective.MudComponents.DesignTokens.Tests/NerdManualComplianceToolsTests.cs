using TheNerdCollective.Brand.Dnf;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdManualComplianceToolsTests
{
    [Fact]
    public void Evaluate_dnf_preset_scores_high_on_shell_and_overlay()
    {
        var options = new NerdDesignTokenOptions();
        NerdDnfDesignTokenPresets.Apply(options);

        var result = NerdManualComplianceTools.Evaluate(options);

        Assert.True(result.Score >= 80);
        Assert.Contains(result.Metrics, metric => metric.Name == "Shell recipes" && metric.Score >= 80);
        Assert.Contains(result.Metrics, metric => metric.Name == "Hero overlays" && metric.Score == 100);
    }

    [Fact]
    public void Evaluate_flags_missing_hero_overlay_when_photo_recipe_exists()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc" };
        options.Add("navy", new NerdColorToken { Value = "#001B3A" });
        options.Add("snow", new NerdColorToken { Value = "#FFFFFF" });
        options.AddRecipe("hero-photo", new NerdDesignTokenRecipe("navy", "snow"));

        var overlayMetric = NerdManualComplianceTools.Evaluate(options).Metrics
            .First(metric => metric.Name == "Hero overlays");

        Assert.Equal(0, overlayMetric.Score);
    }
}
