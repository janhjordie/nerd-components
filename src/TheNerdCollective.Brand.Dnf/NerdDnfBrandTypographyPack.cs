using TheNerdCollective.MudComponents.ResponsiveTypography;

namespace TheNerdCollective.Brand.Dnf;

public sealed class NerdDnfBrandTypographyPack : INerdBrandTypographyPack
{
    public static NerdDnfBrandTypographyPack Instance { get; } = new();

    private static readonly NerdEmbeddedBrandTypographyPack Embedded =
        NerdEmbeddedBrandTypographyPack.FromBrandJson("dnf");

    public string Id => Embedded.Id;

    public void Configure(ResponsiveTypographyOptions options) => Embedded.Configure(options);
}
