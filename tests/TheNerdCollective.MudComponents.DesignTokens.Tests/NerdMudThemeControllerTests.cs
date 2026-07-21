using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public class NerdMudThemeControllerTests
{
    [Fact]
    public void ApplyBrandPack_syncs_hub_token_prefix()
    {
        NerdBrandPackTestBootstrap.EnsureRegistered();

        var tokenOptions = new NerdDesignTokenOptions();
        NerdBrandPackRegistry.Instance.Configure("tnc", tokenOptions);
        var hubOptions = new NerdDesignSystemOptions { TokenPrefix = "tnc" };
        var tokenCss = new NerdDesignTokenCss(MudBlazorDesignTokenCssGenerator.Generate(tokenOptions));
        var controller = new NerdMudThemeController(tokenOptions, tokenCss, hubOptions);

        controller.ApplyBrandPack("dnf");

        Assert.Equal("dnf", tokenOptions.Prefix);
        Assert.Equal("dnf", hubOptions.TokenPrefix);
        Assert.Equal("dnf", hubOptions.ActiveTokenPackId);
    }
}
