namespace TheNerdCollective.Brand.Acme;

public sealed class NerdAcmeBrandPack : INerdBrandPack
{
    public const string LogoPath = "_content/TheNerdCollective.Brand.Acme/brand/acme-logo.svg";
    public const string MobileLogoPath = "_content/TheNerdCollective.Brand.Acme/brand/acme-logo-mobile.svg";

    public static NerdAcmeBrandPack Instance { get; } = new();

    private static readonly NerdEmbeddedBrandPack Embedded = NerdEmbeddedBrandPack.FromBrandJson("acme");

    public string Id => Embedded.Id;

    public string DisplayName => Embedded.DisplayName;

    public string IdentityVersion => Embedded.IdentityVersion;

    public INerdPairingPolicy? PairingPolicy => Embedded.PairingPolicy;

    public void Configure(NerdDesignTokenOptions options) => Embedded.Configure(options);
}
