using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TheNerdCollective.MudComponents.DesignTokens;

public static class NerdBrandPackServiceCollectionExtensions
{
    public static IServiceCollection AddNerdDesignTokenBrandPacks(
        this IServiceCollection services,
        params INerdBrandPack[] packs)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(packs);

        foreach (var pack in packs)
        {
            RegisterBrandPack(services, pack);
        }

        services.TryAddSingleton(NerdBrandPackRegistry.Instance);
        return services;
    }

    public static IServiceCollection RegisterBrandPack(
        this IServiceCollection services,
        INerdBrandPack pack)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(pack);
        NerdBrandPackRegistry.Instance.Register(pack);
        services.TryAddEnumerable(ServiceDescriptor.Singleton(pack));
        services.TryAddSingleton(NerdBrandPackRegistry.Instance);
        return services;
    }

    public static IServiceCollection AddNerdDesignTokensFromBrand(
        this IServiceCollection services,
        string brandId,
        Action<NerdDesignTokenOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddNerdDesignTokens(options =>
        {
            NerdBrandPackRegistry.Instance.Configure(brandId, options);
            configure?.Invoke(options);
        });
    }
}
