using TheNerdCollective.Brand.Acme;
using TheNerdCollective.MudComponents.DesignTokens;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public class NerdAcmePairingToolsTests
{
    private static readonly NerdAcmePairingPolicy Policy = new();

    [Fact]
    public void GetAllApprovedPairings_lists_three_acme_combinations() =>
        Assert.Equal(3, Policy.GetApprovedPairings().Count);

    [Theory]
    [InlineData("cloud", "ink")]
    [InlineData("forest", "cloud")]
    [InlineData("ink", "cloud")]
    public void SuggestContentToken_prefers_light_on_dark_and_ink_on_light(
        string surface,
        string expected)
    {
        var options = new NerdDesignTokenOptions { Prefix = "acme" };
        NerdAcmeDesignTokenPresets.Apply(options);
        Assert.Equal(expected, Policy.SuggestContentToken(surface, options));
    }

    [Fact]
    public void ValidatePairing_marks_ink_on_cloud_as_approved()
    {
        var options = new NerdDesignTokenOptions { Prefix = "acme" };
        NerdAcmeDesignTokenPresets.Apply(options);
        options.PairingPolicy = Policy;

        var validation = NerdTokenPairingTools.ValidatePairing("ink", "cloud", options);

        Assert.True(validation.IsBrandApproved);
        Assert.True(validation.MeetsAa);
    }
}
