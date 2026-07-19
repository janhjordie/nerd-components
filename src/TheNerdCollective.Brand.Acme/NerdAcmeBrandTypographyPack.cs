using TheNerdCollective.MudComponents.ResponsiveTypography;

namespace TheNerdCollective.Brand.Acme;

public sealed class NerdAcmeBrandTypographyPack : INerdBrandTypographyPack
{
    public static NerdAcmeBrandTypographyPack Instance { get; } = new();

    private static readonly NerdEmbeddedBrandTypographyPack Embedded =
        NerdEmbeddedBrandTypographyPack.FromBrandJson("acme");

    public string Id => Embedded.Id;

    public void Configure(ResponsiveTypographyOptions options) => Embedded.Configure(options);
}
