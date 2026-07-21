namespace TheNerdCollective.Brand.Dryk;

public static class NerdDrykTypographyPresets
{
    private static readonly NerdEmbeddedBrandTypographyPack Embedded =
        NerdEmbeddedBrandTypographyPack.FromBrandJson("dryk");

    public static void Apply(ResponsiveTypographyOptions options) =>
        Embedded.Configure(options);
}
