using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace TheNerdCollective.MudComponents.Shared;

public static class NerdDesignSystemServiceCollectionExtensions
{
    public static IServiceCollection AddNerdDesignSystem(
        this IServiceCollection services,
        Action<NerdDesignSystemOptions>? configure = null)
    {
        var options = new NerdDesignSystemOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);
        services.AddScoped<NerdClipboardService>();
        services.AddScoped<NerdDownloadService>();
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

        return builder.AddAdditionalAssemblies(typeof(NerdDesignSystemWebApplicationExtensions).Assembly);
    }

    public static RazorComponentsEndpointConventionBuilder AddNerdDesignSystemAssets(
        this RazorComponentsEndpointConventionBuilder builder)
    {
        return builder.AddAdditionalAssemblies(typeof(NerdDesignSystemWebApplicationExtensions).Assembly);
    }
}
