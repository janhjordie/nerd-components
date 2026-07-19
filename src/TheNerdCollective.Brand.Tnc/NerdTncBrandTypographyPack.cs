using TheNerdCollective.MudComponents.ResponsiveTypography;

namespace TheNerdCollective.Brand.Tnc;

public sealed class NerdTncBrandTypographyPack : INerdBrandTypographyPack
{
    public static NerdTncBrandTypographyPack Instance { get; } = new();

    private static readonly NerdEmbeddedBrandTypographyPack Embedded =
        NerdEmbeddedBrandTypographyPack.FromBrandJson("tnc");

    public string Id => Embedded.Id;

    public void Configure(ResponsiveTypographyOptions options) => Embedded.Configure(options);
}
