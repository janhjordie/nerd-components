using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TheNerdCollective.MudComponents.DesignTokens;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public static class NerdBrandPackServiceCollectionExtensions
{
    public static IServiceCollection AddNerdBrandPackIntegration(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<INerdBrandPackSource>(sp =>
        {
            var tokenOptions = sp.GetService<NerdDesignTokenOptions>();
            var typographyOptions = sp.GetService<NerdResponsiveTypographyOptions>();
            var tokenCss = sp.GetService<NerdDesignTokenCss>();
            var hubOptions = sp.GetService<NerdDesignSystemOptions>();
            if (tokenOptions is null || typographyOptions is null || tokenCss is null || hubOptions is null)
            {
                return NullNerdBrandPackSource.Instance;
            }

            return new NerdBrandPackSource(tokenOptions, typographyOptions, tokenCss, hubOptions);
        });
        return services;
    }
}
