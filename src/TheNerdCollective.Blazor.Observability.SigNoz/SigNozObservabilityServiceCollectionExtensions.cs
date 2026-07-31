using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using TheNerdCollective.Blazor.Observability;

namespace TheNerdCollective.Blazor.Observability.SigNoz;

/// <summary>Dependency injection for the SigNoz observability backend adapter.</summary>
public static class SigNozObservabilityServiceCollectionExtensions
{
    /// <summary>
    /// Registers core dashboard services and the SigNoz backend adapter from
    /// <see cref="ObservabilityDashboardOptions.SectionName"/> configuration.
    /// </summary>
    public static IServiceCollection AddObservabilityDashboardWithSigNoz(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<SigNozObservabilityBuilder>? configure = null)
    {
        services.AddObservabilityDashboard(configuration);
        services.AddSigNozObservabilityBackend(configuration, configure);
        return services;
    }

    /// <summary>Registers the SigNoz backend as <see cref="IObservabilityBackend"/>.</summary>
    public static IServiceCollection AddSigNozObservabilityBackend(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<SigNozObservabilityBuilder>? configure = null)
        => services.AddSigNozObservabilityBackend(
            configuration.GetSection($"{ObservabilityDashboardOptions.SectionName}:{SigNozBackendOptions.SectionName}"),
            configure);

    /// <summary>Registers the SigNoz backend using a configuration section.</summary>
    public static IServiceCollection AddSigNozObservabilityBackend(
        this IServiceCollection services,
        IConfigurationSection section,
        Action<SigNozObservabilityBuilder>? configure = null)
        => services.AddSigNozObservabilityBackend(options => section.Bind(options), configure);

    /// <summary>Registers the SigNoz backend using a configure delegate.</summary>
    public static IServiceCollection AddSigNozObservabilityBackend(
        this IServiceCollection services,
        Action<SigNozBackendOptions> configure,
        Action<SigNozObservabilityBuilder>? builderConfigure = null)
    {
        services.AddOptions<SigNozBackendOptions>().Configure(configure);

        var builder = new SigNozObservabilityBuilder(services);
        builderConfigure?.Invoke(builder);
        builder.Apply();

        services.AddHttpClient(SigNozObservabilityBackend.HttpClientName, (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<SigNozBackendOptions>>().Value;
            client.Timeout = options.HttpTimeout;
        });

        RegisterCoreServices(services);

        return services;
    }

    private static void RegisterCoreServices(IServiceCollection services)
    {
        services.TryAddSingleton<ISigNozRuntimeProfileProvider, SigNozRuntimeProfileProvider>();
        services.TryAddSingleton<ISigNozResponseParser, BuiltInSigNozResponseParser>();
        services.TryAddSingleton<ISigNozResponseParser, DeepWalkSigNozResponseParser>();
        services.TryAddSingleton<SigNozResponseParserCoordinator>();
        services.TryAddSingleton<ISigNozCapabilityDiscovery, SigNozCapabilityDiscovery>();
        services.TryAddSingleton<SigNozQueryClient>();
        services.TryAddSingleton<ISigNozQueryClient>(sp => sp.GetRequiredService<SigNozQueryClient>());
        services.TryAddSingleton<SigNozObservabilityBackend>();
        services.AddSingleton<IObservabilityBackend>(sp => sp.GetRequiredService<SigNozObservabilityBackend>());
        services.AddHostedService<SigNozCapabilityDiscoveryHostedService>();
    }
}
