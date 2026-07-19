using System.Text.Json;
using TheNerdCollective.MudComponents.DesignTokens;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class ReferenceBrandPackTests
{
    public ReferenceBrandPackTests() => NerdBrandPackTestBootstrap.EnsureRegistered();

    [Theory]
    [InlineData("tnc")]
    [InlineData("dnf")]
    [InlineData("acme")]
    [InlineData("demo")]
    public void Reference_pack_roundtrips_from_registered_brand(string brandId)
    {
        var pack = NerdTokenPackEnricher.FromBrandPack(NerdBrandPackRegistry.Instance.GetRequired(brandId));
        pack = new NerdTokenPack
        {
            ClientId = pack.ClientId,
            BrandId = brandId,
            DisplayName = NerdBrandPackRegistry.Instance.GetRequired(brandId).DisplayName,
            Prefix = pack.Prefix,
            Version = 2,
            BrandIdentityVersion = pack.BrandIdentityVersion,
            PairingGuideName = pack.PairingGuideName,
            Colors = new(pack.Colors, StringComparer.OrdinalIgnoreCase),
            Aliases = new(pack.Aliases, StringComparer.OrdinalIgnoreCase),
            Radii = new(pack.Radii, StringComparer.OrdinalIgnoreCase),
            Shadows = new(pack.Shadows, StringComparer.OrdinalIgnoreCase),
            Recipes = new(pack.Recipes, StringComparer.OrdinalIgnoreCase),
            Opacities = new(pack.Opacities, StringComparer.OrdinalIgnoreCase),
            ApprovedPairings = [..pack.ApprovedPairings],
            LockedTokens = [..pack.LockedTokens]
        };

        var json = NerdEmbeddedTokenPackLoader.Serialize(pack);
        var loaded = NerdTokenPack.FromJson(json);

        Assert.Equal(pack.Prefix, loaded.Prefix);
        Assert.Equal(pack.Colors.Count, loaded.Colors.Count);
        Assert.Equal(pack.Recipes.Count, loaded.Recipes.Count);
        if (pack.ApprovedPairings.Count > 0)
        {
            Assert.NotNull(loaded.PairingGuideName);
            Assert.Equal(pack.ApprovedPairings.Count, loaded.ApprovedPairings.Count);
            Assert.True(NerdJsonPairingPolicy.TryCreate(loaded, out _));
        }
    }

    [Fact]
    public void Export_reference_brand_pack_json_files()
    {
        var outputDir = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "TheNerdCollective.MudComponents.DesignTokens", "reference", "brands"));
        Directory.CreateDirectory(outputDir);

        foreach (var brandId in new[] { "tnc", "dnf", "acme", "demo" })
        {
            var brandPack = NerdBrandPackRegistry.Instance.GetRequired(brandId);
            var pack = NerdTokenPackEnricher.EnrichRecipeMetadata(
                NerdTokenPackEnricher.EnrichPairingColors(
                    NerdTokenPackEnricher.FromBrandPack(brandPack),
                    NerdBrandPackTestBootstrap.CreateReferenceOptions(brandId)),
                brandId);
            pack = new NerdTokenPack
            {
                ClientId = brandId,
                BrandId = brandId,
                DisplayName = brandPack.DisplayName,
                Prefix = pack.Prefix,
                Version = 2,
                BrandIdentityVersion = brandPack.IdentityVersion,
                PairingGuideName = pack.PairingGuideName,
                Colors = new(pack.Colors, StringComparer.OrdinalIgnoreCase),
                Aliases = new(pack.Aliases, StringComparer.OrdinalIgnoreCase),
                Radii = new(pack.Radii, StringComparer.OrdinalIgnoreCase),
                Shadows = new(pack.Shadows, StringComparer.OrdinalIgnoreCase),
                Recipes = new(pack.Recipes, StringComparer.OrdinalIgnoreCase),
                Opacities = new(pack.Opacities, StringComparer.OrdinalIgnoreCase),
                ApprovedPairings = [..pack.ApprovedPairings],
                LockedTokens = brandId switch
                {
                    "tnc" => ["navy", "coral"],
                    "dnf" => ["skov", "kridt"],
                    _ => []
                }
            };

            var path = Path.Combine(outputDir, $"{brandId}.token-pack.json");
            File.WriteAllText(path, NerdEmbeddedTokenPackLoader.Serialize(pack));
            Assert.True(File.Exists(path));
        }
    }

    [Theory]
    [InlineData("tnc")]
    [InlineData("dnf")]
    [InlineData("acme")]
    [InlineData("demo")]
    public void Embedded_brand_pack_loads_json(string brandId)
    {
        var embedded = NerdEmbeddedBrandPack.FromBrandJson(brandId);
        var options = new NerdDesignTokenOptions();
        embedded.Configure(options);

        Assert.Equal(brandId, options.ActiveBrandPackId);
        Assert.NotEmpty(options.Colors);
        if (embedded.PairingPolicy is not null)
        {
            Assert.IsType<NerdJsonPairingPolicy>(options.PairingPolicy);
            Assert.NotEmpty(options.PairingPolicy!.GetApprovedPairings());
        }
    }
}
