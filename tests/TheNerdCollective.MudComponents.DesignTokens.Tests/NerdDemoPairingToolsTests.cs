using TheNerdCollective.Brand.Demo;
using TheNerdCollective.MudComponents.DesignTokens;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public class NerdDemoPairingToolsTests
{
    private static readonly NerdDemoPairingPolicy Policy = new();

    [Fact]
    public void GetAllApprovedPairings_lists_four_demo_combinations() =>
        Assert.Equal(4, Policy.GetApprovedPairings().Count);

    [Theory]
    [InlineData("slate", "paper")]
    [InlineData("violet", "paper")]
    [InlineData("paper", "slate")]
    public void SuggestContentToken_follows_demo_cta_strip(string surface, string expected)
    {
        var options = new NerdDesignTokenOptions { Prefix = "demo" };
        NerdDemoDesignTokenPresets.Apply(options);
        Assert.Equal(expected, Policy.SuggestContentToken(surface, options));
    }

    [Fact]
    public void ValidatePairing_marks_paper_on_slate_as_approved()
    {
        var options = new NerdDesignTokenOptions { Prefix = "demo" };
        NerdDemoDesignTokenPresets.Apply(options);
        options.PairingPolicy = Policy;

        var validation = NerdTokenPairingTools.ValidatePairing("paper", "slate", options);

        Assert.True(validation.IsBrandApproved);
        Assert.True(validation.MeetsAa);
    }
}
