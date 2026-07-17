using Microsoft.Extensions.DependencyInjection;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public static class NerdResponsiveTypographyServiceCollectionExtensions
{
    public static IServiceCollection AddNerdResponsiveTypography(
        this IServiceCollection services,
        Action<NerdResponsiveTypographyOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new NerdResponsiveTypographyOptions();
        configure(options);
        services.AddSingleton(options);
        services.AddHostedService<NerdTypographyAccessibilityStartupValidator>();

        return services;
    }
}
