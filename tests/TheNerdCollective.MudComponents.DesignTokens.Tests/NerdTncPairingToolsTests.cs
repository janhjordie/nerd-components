using TheNerdCollective.Brand.Tnc;
using TheNerdCollective.MudComponents.DesignTokens;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public class NerdTncPairingToolsTests
{
    private static readonly NerdTncPairingPolicy Policy = new();

    [Fact]
    public void GetAllApprovedPairings_lists_four_tnc_recipe_combinations() =>
        Assert.Equal(4, Policy.GetApprovedPairings().Count);

    [Theory]
    [InlineData("navy", "chalk")]
    [InlineData("snow", "ink")]
    [InlineData("coral", "chalk")]
    public void SuggestContentToken_follows_tnc_recipes(string surface, string expected)
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc" };
        NerdTncDesignTokenPresets.Apply(options);
        Assert.Equal(expected, Policy.SuggestContentToken(surface, options));
    }

    [Theory]
    [InlineData("chalk", "navy", true)]
    [InlineData("ink", "snow", true)]
    [InlineData("chalk", "snow", false)]
    public void IsBrandApprovedPairing_matches_recipes(string content, string surface, bool expected) =>
        Assert.Equal(expected, Policy.IsBrandApprovedPairing(content, surface));

    [Fact]
    public void ValidatePairing_marks_chalk_on_navy_as_approved()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc" };
        NerdTncDesignTokenPresets.Apply(options);
        options.PairingPolicy = Policy;

        var validation = NerdTokenPairingTools.ValidatePairing("chalk", "navy", options);

        Assert.True(validation.IsBrandApproved);
        Assert.True(validation.MeetsAa);
    }
}
