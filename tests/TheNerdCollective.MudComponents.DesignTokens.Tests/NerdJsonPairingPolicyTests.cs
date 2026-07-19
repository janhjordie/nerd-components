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
}
