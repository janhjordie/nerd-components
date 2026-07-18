using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public static class NerdResponsiveTypographyServiceCollectionExtensions
{
    public static IServiceCollection AddNerdTypographyPackStore(
        this IServiceCollection services,
        string directory = "App_Data/typography-packs")
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<INerdTypographyPackStore>(
            _ => new FileNerdTypographyPackStore(directory));
        return services;
    }

    public static IServiceCollection AddNerdResponsiveTypography(
        this IServiceCollection services,
        Action<NerdResponsiveTypographyOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new NerdResponsiveTypographyOptions();
        configure(options);

        services.AddNerdDesignSystem(hub => hub.TypographyRoute = options.CatalogRoute);
        services.AddSingleton(options);
        services.AddSingleton<MudTheme>(options.CreatePreviewTheme());

        if (options.Typography.ConfiguredRoles.Count > 0 && options.WarnOnAccessibilityFailuresAtStartup)
        {
            services.AddHostedService<NerdTypographyAccessibilityStartupValidator>();
        }

        return services;
    }
}
