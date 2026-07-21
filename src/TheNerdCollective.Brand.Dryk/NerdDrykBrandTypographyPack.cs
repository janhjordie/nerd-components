namespace TheNerdCollective.Brand.Dryk;

public sealed class NerdDrykBrandTypographyPack : INerdBrandTypographyPack
{
    public static NerdDrykBrandTypographyPack Instance { get; } = new();

    private static readonly NerdEmbeddedBrandTypographyPack Embedded =
        NerdEmbeddedBrandTypographyPack.FromBrandJson("dryk");

    public string Id => Embedded.Id;

    public void Configure(ResponsiveTypographyOptions options) => Embedded.Configure(options);
}
