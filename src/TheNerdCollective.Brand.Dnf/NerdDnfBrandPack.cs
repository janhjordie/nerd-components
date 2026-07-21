namespace TheNerdCollective.Brand.Dnf;

public sealed class NerdDnfBrandPack : INerdBrandPack
{
    public const string LogoPath = "_content/TheNerdCollective.Brand.Dnf/brand/dnf-logo.png";
    public const string MobileLogoPath = "_content/TheNerdCollective.Brand.Dnf/brand/dnf-logo-mobile.png";

    public static NerdDnfBrandPack Instance { get; } = new();

    private static readonly NerdEmbeddedBrandPack Embedded = NerdEmbeddedBrandPack.FromBrandJson("dnf");

    public string Id => Embedded.Id;

    public string DisplayName => Embedded.DisplayName;

    public string IdentityVersion => Embedded.IdentityVersion;

    public INerdPairingPolicy? PairingPolicy => Embedded.PairingPolicy;

    public void Configure(NerdDesignTokenOptions options) => Embedded.Configure(options);
}
