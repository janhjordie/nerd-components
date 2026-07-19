using TheNerdCollective.Brand.Acme;
using TheNerdCollective.Brand.Demo;
using TheNerdCollective.Brand.Dnf;
using TheNerdCollective.Brand.Tnc;
using TheNerdCollective.MudComponents.ResponsiveTypography;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.ResponsiveTypography.Tests;

public sealed class NerdWcagTypographyTests
{
    [Theory]
    [InlineData("Caption", "0.625rem", "0.75rem")]
    [InlineData("Body1", "0.875rem", "1rem")]
    [InlineData("Button", "0.8125rem", "0.875rem")]
    public void NormalizeFontSize_uses_role_specific_floors(string role, string input, string expected) =>
        Assert.Equal(expected, NerdWcagTypography.NormalizeFontSize(role, input));

    [Theory]
    [InlineData("Caption", 12)]
    [InlineData("Button", 14)]
    [InlineData("Body1", 16)]
    [InlineData("H1", 16)]
    public void WcagStandards_returns_role_specific_minimums(string role, double expected) =>
        Assert.Equal(expected, WcagStandards.GetTypographyRoleMinimumPixels(role));

    [Theory]
    [InlineData("tnc", typeof(NerdTncBrandTypographyPack))]
    [InlineData("dnf", typeof(NerdDnfBrandTypographyPack))]
    [InlineData("acme", typeof(NerdAcmeBrandTypographyPack))]
    [InlineData("demo", typeof(NerdDemoBrandTypographyPack))]
    public void Embedded_brand_typography_packs_pass_wcag(string brandId, Type packType)
    {
        var pack = (INerdBrandTypographyPack)packType.GetProperty("Instance")!.GetValue(null)!;
        var options = new NerdResponsiveTypographyOptions();
        pack.Configure(options.Typography);

        Assert.Equal(brandId, pack.Id);
        Assert.Empty(NerdTypographyAccessibilityTools.GetAccessibilityWarnings(options));
    }
}
