using Microsoft.Extensions.DependencyInjection;
using TheNerdCollective.MudComponents.ResponsiveTypography;

namespace TheNerdCollective.Brand.Tnc;

public static class NerdTncBrandServiceCollectionExtensions
{
    public static IServiceCollection AddNerdTncBrand(
        this IServiceCollection services,
        Action<NerdDesignTokenOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.RegisterBrandPack(NerdTncBrandPack.Instance);
        services.RegisterBrandTypography(NerdTncBrandTypographyPack.Instance);
        return services.AddNerdDesignTokensFromBrand("tnc", configure);
    }
}
