using TheNerdCollective.Brand.Acme;
using TheNerdCollective.Brand.Demo;
using TheNerdCollective.Brand.Dnf;
using TheNerdCollective.Brand.Tnc;
using TheNerdCollective.MudComponents.DesignTokens;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.ResponsiveTypography.Tests;

public sealed class NerdBrandRuntimeTests
{
    public NerdBrandRuntimeTests()
    {
        NerdBrandTypographyTestBootstrap.EnsureRegistered();
        EnsureBrandPacksRegistered();
    }

    private static void EnsureBrandPacksRegistered()
    {
        var registry = NerdBrandPackRegistry.Instance;
        if (registry.Packs.Count > 0)
        {
            return;
        }

        registry.Register(NerdDnfBrandPack.Instance);
        registry.Register(NerdAcmeBrandPack.Instance);
        registry.Register(NerdDemoBrandPack.Instance);
        registry.Register(NerdTncBrandPack.Instance);
    }

    [Fact]
    public void RestoreDefaultBrand_resets_mutated_token_pack()
    {
        var tokenOptions = new NerdDesignTokenOptions();
        NerdBrandPackRegistry.Instance.Configure("tnc", tokenOptions);
        tokenOptions.DefaultBrandPackId = "tnc";

        var hubOptions = new NerdDesignSystemOptions
        {
            ActiveTokenPackId = tokenOptions.ActiveBrandPackId,
            ActiveBrandIdentityVersion = tokenOptions.ActiveBrandIdentityVersion,
            TokenPrefix = tokenOptions.Prefix,
        };
        var typographyOptions = new NerdResponsiveTypographyOptions();
        NerdBrandTypographyRegistry.Instance.Configure("tnc", typographyOptions);
        var tokenCss = new NerdDesignTokenCss(string.Empty);
        var typographyCss = new NerdResponsiveTypographyCss(string.Empty);

        NerdBrandPackRegistry.Instance.Configure("acme", tokenOptions);
        hubOptions.ActiveTokenPackId = tokenOptions.ActiveBrandPackId;
        hubOptions.TokenPrefix = tokenOptions.Prefix;

        NerdBrandRuntime.RestoreDefaultBrand(
            tokenOptions,
            hubOptions,
            typographyOptions,
            tokenCss,
            typographyCss);

        Assert.Equal("tnc", tokenOptions.ActiveBrandPackId);
        Assert.Equal("tnc", tokenOptions.Prefix);
        Assert.Equal("tnc", hubOptions.ActiveTokenPackId);
        Assert.Equal("tnc", hubOptions.ActiveTypographyPackId);
        Assert.Contains("tnc-", tokenCss.Content, StringComparison.Ordinal);
    }
}
