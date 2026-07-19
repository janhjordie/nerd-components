using Microsoft.Extensions.DependencyInjection;
using TheNerdCollective.MudComponents.ResponsiveTypography;

namespace TheNerdCollective.Brand.Dnf;

public static class NerdDnfBrandServiceCollectionExtensions
{
    public static IServiceCollection AddNerdDnfBrand(
        this IServiceCollection services,
        Action<NerdDesignTokenOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.RegisterBrandPack(NerdDnfBrandPack.Instance);
        services.RegisterBrandTypography(NerdDnfBrandTypographyPack.Instance);
        return services.AddNerdDesignTokensFromBrand("dnf", configure);
    }

    public static IServiceCollection AddNerdDnfTypography(
        this IServiceCollection services,
        Action<NerdResponsiveTypographyOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddNerdResponsiveTypography(options =>
        {
            NerdDnfTypographyPresets.Apply(options.Typography);
            configure?.Invoke(options);
        });
    }

    public static IServiceCollection AddNerdDnfDesignSystem(
        this IServiceCollection services,
        Action<NerdDesignTokenOptions>? configureTokens = null,
        Action<NerdResponsiveTypographyOptions>? configureTypography = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddNerdDnfBrand(configureTokens);
        return services.AddNerdDnfTypography(configureTypography);
    }
}
