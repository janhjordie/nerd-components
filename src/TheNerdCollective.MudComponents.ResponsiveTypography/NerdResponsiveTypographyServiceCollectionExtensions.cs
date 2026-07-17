using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public static class NerdResponsiveTypographyServiceCollectionExtensions
{
    public static IServiceCollection AddNerdResponsiveTypography(
        this IServiceCollection services,
        Action<NerdResponsiveTypographyOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddNerdDesignSystem(options =>
        {
            options.TypographyRoute = "/nerd-typography";
        });

        var options = new NerdResponsiveTypographyOptions();
        configure(options);
        services.AddSingleton(options);
        services.AddSingleton<MudTheme>(options.CreatePreviewTheme());

        if (options.Typography.ConfiguredRoles.Count > 0 && options.WarnOnAccessibilityFailuresAtStartup)
        {
            services.AddHostedService<NerdTypographyAccessibilityStartupValidator>();
        }

        return services;
    }
}
