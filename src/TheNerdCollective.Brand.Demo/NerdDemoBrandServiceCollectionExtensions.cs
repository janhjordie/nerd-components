using Microsoft.Extensions.DependencyInjection;
using TheNerdCollective.MudComponents.ResponsiveTypography;

namespace TheNerdCollective.Brand.Demo;

public static class NerdDemoBrandServiceCollectionExtensions
{
    public static IServiceCollection AddNerdDemoBrand(
        this IServiceCollection services,
        Action<NerdDesignTokenOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.RegisterBrandPack(NerdDemoBrandPack.Instance);
        services.RegisterBrandTypography(NerdDemoBrandTypographyPack.Instance);
        return services.AddNerdDesignTokensFromBrand("demo", configure);
    }
}
