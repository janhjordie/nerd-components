using TheNerdCollective.Brand.Acme;
using TheNerdCollective.Brand.Demo;
using TheNerdCollective.Brand.Dnf;
using TheNerdCollective.Brand.Tnc;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.ResponsiveTypography.Tests;

public sealed class NerdBrandTypographyRegistryTests
{
    public NerdBrandTypographyRegistryTests() => NerdBrandTypographyTestBootstrap.EnsureRegistered();

    [Fact]
    public void Configure_applies_dnf_editorial_scale()
    {
        var options = new NerdResponsiveTypographyOptions();
        NerdBrandTypographyRegistry.Instance.Configure("dnf", options);

        Assert.Contains("clamp(", options.Typography.H1, StringComparison.Ordinal);
        Assert.Equal("1.5", options.Typography.LineHeight);
        Assert.Equal("0.12em", options.Typography.LetterSpacing);
        Assert.Empty(NerdTypographyAccessibilityTools.GetAccessibilityWarnings(options));
    }

    [Fact]
    public void Configure_replaces_previous_brand_values()
    {
        var options = new NerdResponsiveTypographyOptions();
        NerdBrandTypographyRegistry.Instance.Configure("dnf", options);
        NerdBrandTypographyRegistry.Instance.Configure("acme", options);

        Assert.Equal("1rem", options.Typography.Body1);
        Assert.Equal("clamp(1.25rem, 1.6vw, 1.5rem)", options.Typography.H3);
        Assert.Equal("1.5", options.Typography.LineHeight);
        Assert.Equal("clamp(1.5rem, 2.5vw, 2rem)", options.Typography.H1);
    }

    [Fact]
    public void SwitchBrand_updates_hub_typography_pack_id()
    {
        var options = new NerdResponsiveTypographyOptions();
        var hub = new NerdDesignSystemOptions();

        NerdBrandTypographySwitcher.SwitchBrand("tnc", options, hub);

        Assert.Equal("tnc", hub.ActiveTypographyPackId);
        Assert.Contains("clamp(", options.Typography.H3, StringComparison.Ordinal);
    }
}
