using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TheNerdCollective.Blazor.Observability.Backends;

namespace TheNerdCollective.Blazor.Observability;

/// <summary>Dependency injection extensions for observability dashboard services.</summary>
public static class ObservabilityDashboardServiceCollectionExtensions
{
    /// <summary>Registers observability dashboard services using configuration section <see cref="ObservabilityDashboardOptions.SectionName"/>.</summary>
    public static IServiceCollection AddObservabilityDashboard(
        this IServiceCollection services,
        IConfiguration configuration)
        => services.AddObservabilityDashboard(configuration.GetSection(ObservabilityDashboardOptions.SectionName));

    /// <summary>Registers observability dashboard services using a configuration section.</summary>
    public static IServiceCollection AddObservabilityDashboard(
        this IServiceCollection services,
        IConfigurationSection section)
        => services.AddObservabilityDashboard(options => section.Bind(options));

    /// <summary>Registers observability dashboard services using a configure delegate.</summary>
    public static IServiceCollection AddObservabilityDashboard(
        this IServiceCollection services,
        Action<ObservabilityDashboardOptions> configure)
    {
        services.AddOptions<ObservabilityDashboardOptions>()
            .Configure(configure);

        services.AddHttpClient(SigNozObservabilityBackend.HttpClientName, (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ObservabilityDashboardOptions>>().Value.SigNoz;
            client.Timeout = options.HttpTimeout;
        });

        services.AddSingleton<IObservabilityBackend>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ObservabilityDashboardOptions>>().Value;
            return options.Backend switch
            {
                ObservabilityBackendKind.SigNoz => sp.GetRequiredService<SigNozObservabilityBackend>(),
                ObservabilityBackendKind.InProcess => throw new NotSupportedException(
                    "In-process observability backend is not implemented yet. Use ObservabilityBackendKind.SigNoz."),
                _ => sp.GetRequiredService<SigNozObservabilityBackend>()
            };
        });

        services.AddSingleton<SigNozObservabilityBackend>();
        services.AddSingleton<IObservabilityDashboardService, ObservabilityDashboardService>();

        return services;
    }
}
