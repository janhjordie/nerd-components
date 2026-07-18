using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TheNerdCollective.MudComponents.Shared;

public static class NerdDesignSystemServiceCollectionExtensions
{
    public static IServiceCollection AddNerdDesignSystem(
        this IServiceCollection services,
        Action<NerdDesignSystemOptions>? configure = null)
    {
        services.TryAddScoped<NerdClipboardService>();
        services.TryAddScoped<NerdDownloadService>();
        services.TryAddSingleton<INerdCatalogEntitlements, NerdOpenCatalogEntitlements>();
        services.TryAddSingleton<INerdCatalogUpgradeUi, NerdDefaultCatalogUpgradeUi>();

        services.TryAddSingleton<NerdDesignSystemOptions>(sp =>
        {
            var options = new NerdDesignSystemOptions();
            foreach (var action in sp.GetServices<NerdDesignSystemConfigureAction>())
            {
                action.Configure(options);
            }

            return options;
        });

        if (configure is not null)
        {
            services.AddSingleton(new NerdDesignSystemConfigureAction(configure));
        }

        return services;
    }
}

public static class NerdDesignSystemWebApplicationExtensions
{
    public static RazorComponentsEndpointConventionBuilder AddNerdDesignSystemHub(
        this RazorComponentsEndpointConventionBuilder builder,
        IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(services);

        var options = services.GetService<NerdDesignSystemOptions>();
        if (options is not null && !options.EnableHubPage)
        {
            return builder;
        }

        return builder.AddNerdDesignSystemAssets();
    }

    public static RazorComponentsEndpointConventionBuilder AddNerdDesignSystemAssets(
        this RazorComponentsEndpointConventionBuilder builder)
    {
        return builder.AddAdditionalAssemblies(typeof(NerdDesignSystemWebApplicationExtensions).Assembly);
    }
}
