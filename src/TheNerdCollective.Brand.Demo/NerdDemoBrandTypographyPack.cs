using TheNerdCollective.MudComponents.ResponsiveTypography;

namespace TheNerdCollective.Brand.Demo;

public sealed class NerdDemoBrandTypographyPack : INerdBrandTypographyPack
{
    public static NerdDemoBrandTypographyPack Instance { get; } = new();

    private static readonly NerdEmbeddedBrandTypographyPack Embedded =
        NerdEmbeddedBrandTypographyPack.FromBrandJson("demo");

    public string Id => Embedded.Id;

    public void Configure(ResponsiveTypographyOptions options) => Embedded.Configure(options);
}
