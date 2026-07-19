using TheNerdCollective.MudComponents.ResponsiveTypography;

namespace TheNerdCollective.Brand.Demo;

public static class NerdDemoTypographyPresets
{
    private static readonly NerdEmbeddedBrandTypographyPack Embedded =
        NerdEmbeddedBrandTypographyPack.FromBrandJson("demo");

    public static void Apply(ResponsiveTypographyOptions options) =>
        Embedded.Configure(options);
}
