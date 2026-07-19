using TheNerdCollective.Brand.Dnf;
using TheNerdCollective.MudComponents.DesignTokens;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public class NerdBrandHealthToolsTests
{
    [Fact]
    public void Evaluate_returns_metrics_and_overall_score()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf" };
        NerdDnfDesignTokenPresets.Apply(options);

        var result = NerdBrandHealthTools.Evaluate(options);

        Assert.InRange(result.Score, 0, 100);
        Assert.Equal(4, result.Metrics.Count);
        Assert.Contains(result.Metrics, metric => metric.Name == "Contrast");
        Assert.Contains(result.Metrics, metric => metric.Name == "Naming");
        Assert.Contains(result.Metrics, metric => metric.Name == "Recipe coverage");
        Assert.Contains(result.Metrics, metric => metric.Name == "Unused tokens");
    }

    [Fact]
    public void Evaluate_counts_opacity_base_tokens_as_used()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf" }
            .Add("skov", new NerdColorToken { Value = "#002D26", ContrastText = "#FDFAF3" })
            .AddOpacity("watermark", new NerdOpacityToken("skov", 0.12));

        var unusedMetric = NerdBrandHealthTools.Evaluate(options).Metrics
            .Single(metric => metric.Name == "Unused tokens");

        Assert.Equal(100, unusedMetric.Score);
    }
}
