using TheNerdCollective.Brand.Dnf;
using TheNerdCollective.MudComponents.DesignTokens;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public class NerdDnfPairingToolsTests
{
    private static readonly NerdDnfPairingPolicy Policy = new();

    [Fact]
    public void GetAllApprovedPairings_lists_twelve_dnf_combinations() =>
        Assert.Equal(12, Policy.GetApprovedPairings().Count);

    [Theory]
    [InlineData("kridt", NerdDnfPairingPolicy.SkovToken)]
    [InlineData("kridt-lys", NerdDnfPairingPolicy.SkovToken)]
    [InlineData("sol", NerdDnfPairingPolicy.SkovToken)]
    [InlineData("skov", NerdDnfPairingPolicy.KridtToken)]
    [InlineData("jord", NerdDnfPairingPolicy.KridtToken)]
    [InlineData("blad", NerdDnfPairingPolicy.KridtToken)]
    public void SuggestContentToken_follows_dnf_guide(string surface, string expected)
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf" };
        NerdDnfDesignTokenPresets.Apply(options);
        Assert.Equal(expected, Policy.SuggestContentToken(surface, options));
    }

    [Theory]
    [InlineData(NerdDnfPairingPolicy.SkovToken, "kridt", true)]
    [InlineData(NerdDnfPairingPolicy.KridtToken, "skov", true)]
    [InlineData(NerdDnfPairingPolicy.SkovToken, "jord", false)]
    [InlineData(NerdDnfPairingPolicy.KridtToken, "kridt", false)]
    public void IsBrandApprovedPairing_matches_identity_guide(
        string content,
        string surface,
        bool expected) =>
        Assert.Equal(expected, Policy.IsBrandApprovedPairing(content, surface));

    [Fact]
    public void ValidatePairing_marks_skov_on_kridt_as_approved_and_aa()
    {
        var options = CreateDnfOptions();
        var validation = NerdTokenPairingTools.ValidatePairing(
            NerdDnfPairingPolicy.SkovToken,
            "kridt",
            options);

        Assert.True(validation.IsBrandApproved);
        Assert.True(validation.MeetsAa);
        Assert.Equal(NerdDnfDesignTokenPresets.SkovText, validation.ContentColor);
        Assert.Equal(options.Colors["kridt"].Value, validation.SurfaceColor);
    }

    [Theory]
    [InlineData("kridt", "jord")]
    [InlineData("kridt", "ler")]
    [InlineData("kridt", "hav")]
    [InlineData("kridt", "skov")]
    [InlineData("kridt", "blad")]
    public void ValidatePairing_uses_kridt_text_on_dark_dnf_surfaces(string content, string surface)
    {
        var options = CreateDnfOptions();
        var validation = NerdTokenPairingTools.ValidatePairing(content, surface, options);

        Assert.Equal(NerdDnfDesignTokenPresets.KridtText, validation.ContentColor);
        Assert.Equal(options.Colors[surface].Value, validation.SurfaceColor);
        Assert.True(validation.MeetsAa);
        Assert.True(validation.IsBrandApproved);
    }

    [Theory]
    [InlineData("skov", "kridt")]
    [InlineData("skov", "kridt-lys")]
    [InlineData("skov", "sol")]
    [InlineData("skov", "morgenrode")]
    [InlineData("skov", "himmel")]
    [InlineData("skov", "flod")]
    [InlineData("skov", "graes")]
    public void ValidatePairing_uses_skov_text_on_light_dnf_surfaces(string content, string surface)
    {
        var options = CreateDnfOptions();
        var validation = NerdTokenPairingTools.ValidatePairing(content, surface, options);

        Assert.Equal(NerdDnfDesignTokenPresets.SkovText, validation.ContentColor);
        Assert.Equal(options.Colors[surface].Value, validation.SurfaceColor);
        Assert.True(validation.MeetsAa);
        Assert.True(validation.IsBrandApproved);
    }

    private static NerdDesignTokenOptions CreateDnfOptions()
    {
        NerdBrandPackTestBootstrap.EnsureRegistered();
        var options = new NerdDesignTokenOptions();
        NerdBrandPackRegistry.Instance.Configure("dnf", options);
        return options;
    }
}
