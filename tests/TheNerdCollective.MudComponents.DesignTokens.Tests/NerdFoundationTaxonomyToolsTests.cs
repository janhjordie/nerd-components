using TheNerdCollective.MudComponents.DesignTokens;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdFoundationTaxonomyToolsTests
{
    [Fact]
    public void ApplyDefaults_adds_breakpoints_motion_and_z_index()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc" };

        NerdFoundationTaxonomyTools.ApplyDefaults(options);

        Assert.Equal("600px", options.Breakpoints["sm"]);
        Assert.Equal("250ms", options.MotionDurations["normal"]);
        Assert.Equal("cubic-bezier(0.4, 0, 0.2, 1)", options.MotionEasings["standard"]);
        Assert.Equal("1300", options.ZIndex["modal"]);
    }
}
