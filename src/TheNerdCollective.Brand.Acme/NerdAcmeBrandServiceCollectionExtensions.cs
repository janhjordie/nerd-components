using Microsoft.Extensions.DependencyInjection;
using TheNerdCollective.MudComponents.ResponsiveTypography;

namespace TheNerdCollective.Brand.Acme;

public static class NerdAcmeBrandServiceCollectionExtensions
{
    public static IServiceCollection AddNerdAcmeBrand(
        this IServiceCollection services,
        Action<NerdDesignTokenOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.RegisterBrandPack(NerdAcmeBrandPack.Instance);
        services.RegisterBrandTypography(NerdAcmeBrandTypographyPack.Instance);
        return services.AddNerdDesignTokensFromBrand("acme", configure);
    }
}
