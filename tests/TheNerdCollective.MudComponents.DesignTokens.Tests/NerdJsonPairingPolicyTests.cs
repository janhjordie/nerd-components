using TheNerdCollective.Brand.Dnf;
using TheNerdCollective.MudComponents.DesignTokens;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdJsonPairingPolicyTests
{
    [Fact]
    public void TryCreate_returns_false_when_pairings_missing()
    {
        var pack = new NerdTokenPack
        {
            ClientId = "test",
            Prefix = "test",
            PairingGuideName = "Guide"
        };

        Assert.False(NerdJsonPairingPolicy.TryCreate(pack, out var policy));
        Assert.Null(policy);
    }

    [Fact]
    public void IsBrandApprovedPairing_respects_json_list()
    {
        var pack = new NerdTokenPack
        {
            ClientId = "test",
            Prefix = "test",
            PairingGuideName = "Test guide",
            ApprovedPairings =
            [
                new NerdApprovedPairing("chalk", "navy"),
                new NerdApprovedPairing("ink", "snow")
            ]
        };

        Assert.True(NerdJsonPairingPolicy.TryCreate(pack, out var policy));
        Assert.NotNull(policy);
        Assert.True(policy.IsBrandApprovedPairing("chalk", "navy"));
        Assert.False(policy.IsBrandApprovedPairing("coral", "navy"));
        Assert.Equal(2, policy.GetApprovedPairings().Count);
    }

    [Fact]
    public void GetApprovedContentTokens_filters_by_surface()
    {
        var policy = new NerdJsonPairingPolicy(
            "Guide",
            [
                new NerdApprovedPairing("chalk", "navy"),
                new NerdApprovedPairing("coral", "navy"),
                new NerdApprovedPairing("ink", "snow")
            ]);
        var options = new NerdDesignTokenOptions()
            .Add("navy", new NerdColorToken { Value = "#001" })
            .Add("snow", new NerdColorToken { Value = "#fff" })
            .Add("chalk", new NerdColorToken { Value = "#eee" })
            .Add("coral", new NerdColorToken { Value = "#f55" })
            .Add("ink", new NerdColorToken { Value = "#111" });

        var navyContents = policy.GetApprovedContentTokens("navy", options);
        Assert.Equal(2, navyContents.Count);
        Assert.Contains("chalk", navyContents, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("coral", navyContents, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void TokenPackImporter_rejects_invalid_json()
    {
        var result = NerdTokenPackImporter.TryImport("{not-json");
        Assert.False(result.Success);
        Assert.Null(result.Pack);
    }

    [Fact]
    public void TokenPackImporter_accepts_minimal_pack()
    {
        var result = NerdTokenPackImporter.TryImport(
            """{"clientId":"x","prefix":"x","version":2,"colors":{"ink":{"value":"#111"}}}""");
        Assert.True(result.Success);
        Assert.NotNull(result.Pack);
        Assert.Single(result.Pack.Colors);
    }

    [Fact]
    public void ResolveForegroundColor_uses_skov_green_not_contrast_text_on_light_surfaces()
    {
        NerdBrandPackTestBootstrap.EnsureRegistered();
        var options = new NerdDesignTokenOptions();
        NerdBrandPackRegistry.Instance.Configure("dnf", options);
        var policy = Assert.IsType<NerdJsonPairingPolicy>(options.PairingPolicy);

        Assert.Equal(NerdDnfDesignTokenPresets.SkovText, policy.ResolveForegroundColor("skov", options));
        Assert.Equal(NerdDnfDesignTokenPresets.KridtText, policy.ResolveForegroundColor("kridt", options));
    }

    [Theory]
    [InlineData("skov", "kridt")]
    [InlineData("skov", "sol")]
    [InlineData("kridt", "jord")]
    public void ValidatePairing_resolves_dnf_approved_pairings_from_json_policy(string content, string surface)
    {
        NerdBrandPackTestBootstrap.EnsureRegistered();
        var options = new NerdDesignTokenOptions();
        NerdBrandPackRegistry.Instance.Configure("dnf", options);

        var validation = NerdTokenPairingTools.ValidatePairing(content, surface, options);

        Assert.True(validation.IsBrandApproved);
        Assert.True(validation.MeetsAa);
        Assert.Equal(
            string.Equals(content, "kridt", StringComparison.OrdinalIgnoreCase)
                ? NerdDnfDesignTokenPresets.KridtText
                : NerdDnfDesignTokenPresets.SkovText,
            validation.ContentColor);
    }
}
