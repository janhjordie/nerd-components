namespace TheNerdCollective.Brand.Demo;

public sealed class NerdDemoBrandPack : INerdBrandPack
{
    public const string LogoPath = "_content/TheNerdCollective.Brand.Demo/brand/demo-logo.svg";
    public const string MobileLogoPath = "_content/TheNerdCollective.Brand.Demo/brand/demo-logo-mobile.svg";

    public static NerdDemoBrandPack Instance { get; } = new();

    private static readonly NerdEmbeddedBrandPack Embedded = NerdEmbeddedBrandPack.FromBrandJson("demo");

    public string Id => Embedded.Id;

    public string DisplayName => Embedded.DisplayName;

    public string IdentityVersion => Embedded.IdentityVersion;

    public INerdPairingPolicy? PairingPolicy => Embedded.PairingPolicy;

    public void Configure(NerdDesignTokenOptions options) => Embedded.Configure(options);
}
