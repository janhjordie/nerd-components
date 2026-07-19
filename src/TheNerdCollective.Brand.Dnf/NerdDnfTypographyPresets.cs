using TheNerdCollective.MudComponents.ResponsiveTypography;

namespace TheNerdCollective.Brand.Dnf;

public static class NerdDnfTypographyPresets
{
    private static readonly NerdEmbeddedBrandTypographyPack Embedded =
        NerdEmbeddedBrandTypographyPack.FromBrandJson("dnf");

    public static void Apply(ResponsiveTypographyOptions options) =>
        Embedded.Configure(options);
}
