using System.Text.Json;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.Shared.Tests;

public class NerdBrandPackToolsTests
{
    [Fact]
    public void Create_and_unbundle_round_trips_token_and_typography_json()
    {
        const string tokens = """{"clientId":"acme","prefix":"acme","colors":{"forest":{"value":"#1F6B3A"}}}""";
        const string typography = """{"clientId":"acme","roles":{"H1":"clamp(1rem, 2vw, 2rem)"}}""";

        var pack = NerdBrandPackTools.Create("acme", tokens, typography);
        var json = NerdBrandPackTools.ToJson(pack);
        var restored = NerdBrandPackTools.FromJson(json);
        var (restoredTokens, restoredTypography) = NerdBrandPackTools.Unbundle(restored);

        Assert.Equal("acme", restored.ClientId);
        Assert.Contains("forest", restoredTokens, StringComparison.Ordinal);
        Assert.Contains("H1", restoredTypography, StringComparison.Ordinal);
    }

    [Fact]
    public void ToZip_contains_bundle_and_section_files()
    {
        const string tokens = """{"clientId":"acme","prefix":"acme","colors":{"forest":{"value":"#1F6B3A"}}}""";
        const string typography = """{"clientId":"acme","roles":{"H1":"clamp(1rem, 2vw, 2rem)"}}""";
        var pack = NerdBrandPackTools.Create("acme", tokens, typography);

        var zipBytes = NerdBrandPackTools.ToZip(pack);

        using var memory = new MemoryStream(zipBytes);
        var restored = NerdBrandPackTools.FromZip(memory);
        Assert.Equal("acme", restored.ClientId);
        Assert.NotNull(restored.DesignTokens);
        Assert.NotNull(restored.Typography);
    }
}
