using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

        services.AddNerdDesignSystem(hub =>
        {
            hub.TypographyRoute = options.CatalogRoute;
            hub.TypographyRoleCount = options.Typography.ConfiguredRoles.Count;
        });
        services.AddSingleton(options);
        services.AddSingleton(sp => new NerdResponsiveTypographyCss(
            MudBlazorResponsiveTypographyCssGenerator.Generate(options.Typography)));
        services.AddSingleton<MudTheme>(options.CreatePreviewTheme());
        services.TryAddSingleton<INerdTypographyPackStore>(
            _ => new FileNerdTypographyPackStore("App_Data/typography-packs"));

        if (options.Typography.ConfiguredRoles.Count > 0 && options.WarnOnAccessibilityFailuresAtStartup)
        {
            services.AddHostedService<NerdTypographyAccessibilityStartupValidator>();
        }

        services.AddNerdBrandPackIntegration();

        return services;
    }
}
