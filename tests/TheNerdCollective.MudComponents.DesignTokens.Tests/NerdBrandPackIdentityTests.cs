using TheNerdCollective.Brand.Acme;
using TheNerdCollective.Brand.Demo;
using TheNerdCollective.Brand.Dnf;
using TheNerdCollective.Brand.Dryk;
using TheNerdCollective.Brand.Tnc;
using TheNerdCollective.MudComponents.DesignTokens;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public class NerdBrandPackIdentityTests
{
    [Theory]
    [InlineData(typeof(NerdDnfBrandPack), "dnf", "1.0.0")]
    [InlineData(typeof(NerdDrykBrandPack), "dryk", "1.0.0")]
    [InlineData(typeof(NerdTncBrandPack), "tnc", "1.0.0")]
    [InlineData(typeof(NerdAcmeBrandPack), "acme", "1.0.0")]
    [InlineData(typeof(NerdDemoBrandPack), "demo", "1.0.0")]
    public void Brand_packs_expose_identity_version(Type packType, string id, string identityVersion)
    {
        var pack = (INerdBrandPack)packType.GetProperty("Instance")!.GetValue(null)!;
        Assert.Equal(id, pack.Id);
        Assert.Equal(identityVersion, pack.IdentityVersion);
        Assert.Equal($"{pack.DisplayName} ({identityVersion})", NerdBrandPackLabels.Format(pack));
    }

    [Fact]
    public void Configure_sets_active_brand_identity_on_options()
    {
        NerdBrandPackTestBootstrap.EnsureRegistered();
        var options = new NerdDesignTokenOptions();
        NerdBrandPackRegistry.Instance.Configure("dnf", options);

        Assert.Equal("dnf", options.ActiveBrandPackId);
        Assert.Equal("1.0.0", options.ActiveBrandIdentityVersion);

        var pack = NerdTokenPack.FromOptions(options, "dnf");
        Assert.Equal("1.0.0", pack.BrandIdentityVersion);
    }

    [Fact]
    public void Configure_replaces_previous_brand_recipes_when_switching()
    {
        NerdBrandPackTestBootstrap.EnsureRegistered();
        var options = new NerdDesignTokenOptions();
        NerdBrandPackRegistry.Instance.Configure("tnc", options);
        Assert.Contains("cta", options.Recipes.Keys);
        Assert.DoesNotContain("kridt-himmel", options.Recipes.Keys);

        NerdBrandPackRegistry.Instance.Configure("dnf", options);

        Assert.Equal("dnf", options.Prefix);
        Assert.DoesNotContain("cta", options.Recipes.Keys);
        Assert.Contains("kridt-himmel", options.Recipes.Keys);
        Assert.DoesNotContain("navy", options.Colors.Keys);
    }
}
