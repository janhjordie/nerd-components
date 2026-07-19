using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public static class NerdBrandTypographyServiceCollectionExtensions
{
    public static IServiceCollection AddNerdBrandTypographyPacks(
        this IServiceCollection services,
        params INerdBrandTypographyPack[] packs)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(packs);

        foreach (var pack in packs)
        {
            RegisterBrandTypography(services, pack);
        }

        services.TryAddSingleton(NerdBrandTypographyRegistry.Instance);
        return services;
    }

    public static IServiceCollection RegisterBrandTypography(
        this IServiceCollection services,
        INerdBrandTypographyPack pack)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(pack);
        NerdBrandTypographyRegistry.Instance.Register(pack);
        services.TryAddEnumerable(ServiceDescriptor.Singleton(pack));
        services.TryAddSingleton(NerdBrandTypographyRegistry.Instance);
        return services;
    }
}
