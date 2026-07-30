using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TheNerdCollective.Blazor.Observability;

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

    /// <summary>
    /// Registers backend-neutral observability dashboard services.
    /// Pair with an adapter package such as
    /// <c>TheNerdCollective.Blazor.Observability.SigNoz</c> that registers <see cref="IObservabilityBackend"/>.
    /// </summary>
    public static IServiceCollection AddObservabilityDashboard(
        this IServiceCollection services,
        Action<ObservabilityDashboardOptions> configure)
    {
        services.AddOptions<ObservabilityDashboardOptions>()
            .Configure(configure);

        services.AddSingleton<IObservabilityDashboardService, ObservabilityDashboardService>();

        return services;
    }
}
