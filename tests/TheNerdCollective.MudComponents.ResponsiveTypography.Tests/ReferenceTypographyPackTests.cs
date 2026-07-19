using TheNerdCollective.Brand.Acme;
using TheNerdCollective.Brand.Demo;
using TheNerdCollective.Brand.Dnf;
using TheNerdCollective.Brand.Tnc;
using TheNerdCollective.MudComponents.ResponsiveTypography;

namespace TheNerdCollective.MudComponents.ResponsiveTypography.Tests;

public sealed class ReferenceTypographyPackTests
{
    public ReferenceTypographyPackTests() => NerdBrandTypographyTestBootstrap.EnsureRegistered();

    [Fact]
    public void Embedded_tnc_typography_pack_loads_json()
    {
        var embedded = NerdEmbeddedBrandTypographyPack.FromBrandJson("tnc");
        var options = new ResponsiveTypographyOptions();
        embedded.Configure(options);

        Assert.Equal("tnc", embedded.Id);
        Assert.Equal("1rem", options.Default);
        Assert.Contains("clamp(", options.H1, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("tnc")]
    [InlineData("dnf")]
    [InlineData("acme")]
    [InlineData("demo")]
    public void Embedded_typography_pack_roundtrips_from_registered_brand(string brandId)
    {
        var embedded = NerdEmbeddedBrandTypographyPack.FromBrandJson(brandId);
        var nerdOptions = new NerdResponsiveTypographyOptions();
        embedded.Configure(nerdOptions.Typography);
        var pack = NerdTypographyPack.FromOptions(nerdOptions, brandId);

        Assert.Equal(brandId, embedded.Id);
        Assert.NotEmpty(pack.Roles);
        Assert.Empty(NerdTypographyAccessibilityTools.GetAccessibilityWarnings(nerdOptions));
    }

    [Fact]
    public void Export_reference_typography_pack_json_files()
    {
        var outputDir = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "TheNerdCollective.MudComponents.ResponsiveTypography", "reference", "brands"));
        Directory.CreateDirectory(outputDir);

        foreach (var (brandId, configure) in new (string, Action<ResponsiveTypographyOptions>)[]
        {
            ("tnc", NerdTncTypographyPresets.Apply),
            ("dnf", NerdDnfTypographyPresets.Apply),
            ("acme", NerdAcmeTypographyPresets.Apply),
            ("demo", NerdDemoTypographyPresets.Apply)
        })
        {
            var responsive = new NerdResponsiveTypographyOptions();
            configure(responsive.Typography);
            var pack = NerdTypographyPack.FromOptions(responsive, brandId);
            pack = new NerdTypographyPack
            {
                ClientId = brandId,
                BrandId = brandId,
                DisplayName = brandId.ToUpperInvariant(),
                BrandIdentityVersion = "1.0.0",
                Roles = new(pack.Roles, StringComparer.OrdinalIgnoreCase),
                LineHeight = pack.LineHeight,
                LetterSpacing = pack.LetterSpacing,
                FontWeight = pack.FontWeight
            };

            var path = Path.Combine(outputDir, $"{brandId}.typography-pack.json");
            File.WriteAllText(path, NerdEmbeddedTypographyPackLoader.Serialize(pack));
            Assert.True(File.Exists(path));
        }
    }
}
