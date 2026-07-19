using TheNerdCollective.MudComponents.ResponsiveTypography;

namespace TheNerdCollective.Brand.Tnc;

public static class NerdTncTypographyPresets
{
    private static readonly NerdEmbeddedBrandTypographyPack Embedded =
        NerdEmbeddedBrandTypographyPack.FromBrandJson("tnc");

    public static void Apply(ResponsiveTypographyOptions options) =>
        Embedded.Configure(options);
}
