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
        IConfiguration configuration)
    {
        services.AddObservabilityDashboard(configuration);
        services.AddSigNozObservabilityBackend(configuration);
        return services;
    }

    /// <summary>Registers the SigNoz backend as <see cref="IObservabilityBackend"/>.</summary>
    public static IServiceCollection AddSigNozObservabilityBackend(
        this IServiceCollection services,
        IConfiguration configuration)
        => services.AddSigNozObservabilityBackend(
            configuration.GetSection($"{ObservabilityDashboardOptions.SectionName}:{SigNozBackendOptions.SectionName}"));

    /// <summary>Registers the SigNoz backend using a configuration section.</summary>
    public static IServiceCollection AddSigNozObservabilityBackend(
        this IServiceCollection services,
        IConfigurationSection section)
        => services.AddSigNozObservabilityBackend(options => section.Bind(options));

    /// <summary>Registers the SigNoz backend using a configure delegate.</summary>
    public static IServiceCollection AddSigNozObservabilityBackend(
        this IServiceCollection services,
        Action<SigNozBackendOptions> configure)
    {
        services.AddOptions<SigNozBackendOptions>().Configure(configure);

        services.AddHttpClient(SigNozObservabilityBackend.HttpClientName, (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<SigNozBackendOptions>>().Value;
            client.Timeout = options.HttpTimeout;
        });

        services.TryAddSingleton<SigNozObservabilityBackend>();
        services.AddSingleton<IObservabilityBackend>(sp => sp.GetRequiredService<SigNozObservabilityBackend>());

        return services;
    }
}
