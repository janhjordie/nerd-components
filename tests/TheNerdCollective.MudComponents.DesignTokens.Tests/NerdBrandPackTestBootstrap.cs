using TheNerdCollective.Brand.Acme;
using TheNerdCollective.Brand.Demo;
using TheNerdCollective.Brand.Dnf;
using TheNerdCollective.Brand.Tnc;
using TheNerdCollective.MudComponents.DesignTokens;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public static class NerdBrandPackTestBootstrap
{
    private static bool _initialized;

    public static void EnsureRegistered()
    {
        var registry = NerdBrandPackRegistry.Instance;
        if (_initialized && registry.Packs.Count > 0)
        {
            return;
        }

        registry.Reset();
        registry.Register(NerdDnfBrandPack.Instance);
        registry.Register(NerdAcmeBrandPack.Instance);
        registry.Register(NerdDemoBrandPack.Instance);
        registry.Register(NerdTncBrandPack.Instance);
        _initialized = true;
    }

    public static NerdDesignTokenOptions CreateReferenceOptions(string brandId)
    {
        var options = new NerdDesignTokenOptions();
        switch (brandId)
        {
            case "dnf":
                NerdDnfDesignTokenPresets.Apply(options);
                options.PairingPolicy = new NerdDnfPairingPolicy();
                break;
            case "tnc":
                NerdTncDesignTokenPresets.Apply(options);
                options.PairingPolicy = new NerdTncPairingPolicy();
                break;
            case "acme":
                NerdAcmeDesignTokenPresets.Apply(options);
                options.PairingPolicy = new NerdAcmePairingPolicy();
                break;
            case "demo":
                NerdDemoDesignTokenPresets.Apply(options);
                options.PairingPolicy = new NerdDemoPairingPolicy();
                break;
            default:
                throw new ArgumentException($"Unknown brand '{brandId}'.", nameof(brandId));
        }

        return options;
    }
}
