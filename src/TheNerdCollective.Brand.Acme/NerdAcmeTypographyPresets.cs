using TheNerdCollective.MudComponents.ResponsiveTypography;

namespace TheNerdCollective.Brand.Acme;

public static class NerdAcmeTypographyPresets
{
    private static readonly NerdEmbeddedBrandTypographyPack Embedded =
        NerdEmbeddedBrandTypographyPack.FromBrandJson("acme");

    public static void Apply(ResponsiveTypographyOptions options) =>
        Embedded.Configure(options);
}
