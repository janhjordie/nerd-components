using Microsoft.Extensions.DependencyInjection;

namespace TheNerdCollective.Brand.Dryk;

public static class NerdDrykBrandServiceCollectionExtensions
{
    public static IServiceCollection AddNerdDrykBrand(
        this IServiceCollection services,
        Action<NerdDesignTokenOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.RegisterBrandPack(NerdDrykBrandPack.Instance);
        services.RegisterBrandTypography(NerdDrykBrandTypographyPack.Instance);
        return services.AddNerdDesignTokensFromBrand("dryk", configure);
    }

    public static IServiceCollection AddNerdDrykTypography(
        this IServiceCollection services,
        Action<NerdResponsiveTypographyOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddNerdResponsiveTypography(options =>
        {
            NerdDrykTypographyPresets.Apply(options.Typography);
            configure?.Invoke(options);
        });
    }

    public static IServiceCollection AddNerdDrykDesignSystem(
        this IServiceCollection services,
        Action<NerdDesignTokenOptions>? configureTokens = null,
        Action<NerdResponsiveTypographyOptions>? configureTypography = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddNerdDrykBrand(configureTokens);
        return services.AddNerdDrykTypography(configureTypography);
    }
}
