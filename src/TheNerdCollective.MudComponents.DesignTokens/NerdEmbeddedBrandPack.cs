namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Brand pack backed by an embedded or loaded <see cref="NerdTokenPack"/> JSON document.</summary>
public sealed class NerdEmbeddedBrandPack : INerdBrandPack
{
    private readonly NerdTokenPack _pack;
    private readonly NerdJsonPairingPolicy? _pairingPolicy;

    public NerdEmbeddedBrandPack(NerdTokenPack pack)
    {
        ArgumentNullException.ThrowIfNull(pack);
        _pack = pack;
        if (NerdJsonPairingPolicy.TryCreate(pack, out var policy))
        {
            _pairingPolicy = policy;
        }
    }

    public static NerdEmbeddedBrandPack FromBrandJson(string brandId) =>
        new(NerdEmbeddedTokenPackLoader.LoadBrand(brandId));

    public string Id => _pack.BrandId ?? _pack.Prefix;

    public string DisplayName => _pack.DisplayName ?? Id.ToUpperInvariant();

    public string IdentityVersion => _pack.BrandIdentityVersion ?? "1.0.0";

    public INerdPairingPolicy? PairingPolicy => _pairingPolicy;

    public NerdTokenPack TokenPack => _pack;

    public void Configure(NerdDesignTokenOptions options) => _pack.ApplyTo(options);
}
