using TheNerdCollective.MudComponents.DesignTokens;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdFoundationTaxonomyToolsTests
{
    [Fact]
    public void ApplyDefaults_fills_missing_radii_and_shadows()
    {
        var options = new NerdDesignTokenOptions { Prefix = "test" };

        NerdFoundationTaxonomyTools.ApplyDefaults(options);

        Assert.Equal("8px", options.Radii["default"]);
        Assert.Equal("4px", options.Radii["sm"]);
        Assert.Contains("md", options.Shadows.Keys);
    }

    [Fact]
    public void ToOptions_applies_foundation_defaults()
    {
        var pack = new NerdTokenPack
        {
            ClientId = "test",
            Prefix = "test",
            Colors = new Dictionary<string, NerdColorToken>
            {
                ["primary"] = new() { Value = "#336699" }
            }
        };

        var options = pack.ToOptions();

        Assert.Equal("8px", options.Radii["default"]);
        Assert.True(options.Breakpoints.ContainsKey("md"));
    }
}
