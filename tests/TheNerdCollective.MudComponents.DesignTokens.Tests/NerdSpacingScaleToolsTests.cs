using TheNerdCollective.MudComponents.DesignTokens;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdSpacingScaleToolsTests
{
    [Fact]
    public void ApplyDefaultScale_adds_expected_steps_without_overwriting()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc" };
        options.AddSpacing("4", "18px");

        NerdSpacingScaleTools.ApplyDefaultScale(options);

        Assert.Equal("18px", options.Spacing["4"]);
        Assert.Equal("4px", options.Spacing["1"]);
        Assert.Equal(NerdSpacingScaleTools.DefaultScale.Count, options.Spacing.Count);
    }

    [Fact]
    public void GenerateScale_linear_uses_base_unit_multiplier()
    {
        var scale = NerdSpacingScaleTools.GenerateScale(8, curve: NerdSpacingScaleCurve.Linear);

        Assert.Equal("8px", scale["1"]);
        Assert.Equal("32px", scale["4"]);
    }

    [Fact]
    public void GenerateScale_geometric_applies_ratio()
    {
        var scale = NerdSpacingScaleTools.GenerateScale(4, 2d, NerdSpacingScaleCurve.Geometric);

        Assert.Equal("4px", scale["1"]);
        Assert.Equal("8px", scale["2"]);
        Assert.Equal("16px", scale["3"]);
    }
}
