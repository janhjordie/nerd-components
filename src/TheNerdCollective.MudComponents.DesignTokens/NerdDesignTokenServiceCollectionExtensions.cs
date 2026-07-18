using Microsoft.Extensions.DependencyInjection;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

public static class NerdDesignTokenServiceCollectionExtensions
{
    public static IServiceCollection AddNerdTokenPackStore(
        this IServiceCollection services,
        string directory = "App_Data/token-packs")
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<INerdTokenPackStore>(
            _ => new FileNerdTokenPackStore(directory));
        return services;
    }

    public static IServiceCollection AddNerdDesignTokens(
        this IServiceCollection services,
        Action<NerdDesignTokenOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new NerdDesignTokenOptions();
        configure(options);

        services.AddNerdDesignSystem(hub => hub.DesignTokensRoute = options.CatalogRoute);
        services.AddSingleton(options);
        services.AddSingleton(sp => new NerdDesignTokenCss(
            MudBlazorDesignTokenCssGenerator.Generate(sp.GetRequiredService<NerdDesignTokenOptions>())));

        if (options.Colors.Count > 0 && options.WarnOnAccessibilityFailuresAtStartup)
        {
            services.AddHostedService<NerdDesignTokenAccessibilityStartupValidator>();
        }

        return services;
    }
}
