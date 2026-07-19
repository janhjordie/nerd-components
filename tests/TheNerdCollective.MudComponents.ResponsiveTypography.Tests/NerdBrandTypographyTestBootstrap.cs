using TheNerdCollective.Brand.Acme;
using TheNerdCollective.Brand.Demo;
using TheNerdCollective.Brand.Dnf;
using TheNerdCollective.Brand.Tnc;

namespace TheNerdCollective.MudComponents.ResponsiveTypography.Tests;

public static class NerdBrandTypographyTestBootstrap
{
    private static bool _initialized;

    public static void EnsureRegistered()
    {
        var registry = NerdBrandTypographyRegistry.Instance;
        if (_initialized && registry.Packs.Count > 0)
        {
            return;
        }

        registry.Reset();
        registry.Register(NerdDnfBrandTypographyPack.Instance);
        registry.Register(NerdAcmeBrandTypographyPack.Instance);
        registry.Register(NerdDemoBrandTypographyPack.Instance);
        registry.Register(NerdTncBrandTypographyPack.Instance);
        _initialized = true;
    }
}
