namespace TheNerdCollective.Brand.Tnc;

public sealed class NerdTncBrandPack : INerdBrandPack
{
    public const string LogoOnChromePath = "_content/TheNerdCollective.Brand.Tnc/brand/logo-dark.svg";
    public const string LogoOnSurfacePath = "_content/TheNerdCollective.Brand.Tnc/brand/logo-light.svg";

    public static NerdTncBrandPack Instance { get; } = new();

    private static readonly NerdEmbeddedBrandPack Embedded = NerdEmbeddedBrandPack.FromBrandJson("tnc");

    public string Id => Embedded.Id;

    public string DisplayName => Embedded.DisplayName;

    public string IdentityVersion => Embedded.IdentityVersion;

    public INerdPairingPolicy? PairingPolicy => Embedded.PairingPolicy;

    public void Configure(NerdDesignTokenOptions options) => Embedded.Configure(options);
}
