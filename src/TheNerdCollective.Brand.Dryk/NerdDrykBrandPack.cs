namespace TheNerdCollective.Brand.Dryk;

public sealed class NerdDrykBrandPack : INerdBrandPack
{
    public const string LogoPath = "_content/TheNerdCollective.Brand.Dryk/brand/dryk-logo.png";
    public const string MobileLogoPath = "_content/TheNerdCollective.Brand.Dryk/brand/dryk-logo.png";

    public static NerdDrykBrandPack Instance { get; } = new();

    private static readonly NerdEmbeddedBrandPack Embedded = NerdEmbeddedBrandPack.FromBrandJson("dryk");

    public string Id => Embedded.Id;

    public string DisplayName => Embedded.DisplayName;

    public string IdentityVersion => Embedded.IdentityVersion;

    public INerdPairingPolicy? PairingPolicy => Embedded.PairingPolicy;

    public void Configure(NerdDesignTokenOptions options) => Embedded.Configure(options);
}
