namespace TheNerdCollective.MudComponents.ResponsiveTypography;

/// <summary>Brand typography pack backed by an embedded <see cref="NerdTypographyPack"/> JSON document.</summary>
public sealed class NerdEmbeddedBrandTypographyPack : INerdBrandTypographyPack
{
    private readonly NerdTypographyPack _pack;

    public NerdEmbeddedBrandTypographyPack(NerdTypographyPack pack)
    {
        ArgumentNullException.ThrowIfNull(pack);
        _pack = pack;
    }

    public static NerdEmbeddedBrandTypographyPack FromBrandJson(string brandId) =>
        new(NerdEmbeddedTypographyPackLoader.LoadBrand(brandId));

    public string Id => _pack.BrandId ?? _pack.ClientId;

    public NerdTypographyPack TypographyPack => _pack;

    public void Configure(ResponsiveTypographyOptions options)
    {
        _pack.ApplyTo(options);
        NerdWcagTypography.EnsureCompliance(options);
    }
}
