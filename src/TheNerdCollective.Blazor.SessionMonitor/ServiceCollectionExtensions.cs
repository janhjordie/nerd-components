using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TheNerdCollective.Blazor.SessionMonitor;

/// <summary>
/// Extension methods for registering session monitoring services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds session monitoring to your Blazor Server application.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional options configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSessionMonitoring(
        this IServiceCollection services,
        Action<SessionMonitorOptions>? configure = null)
    {
        if (configure is not null)
        {
            services.Configure(configure);
        }
        else
        {
            services.AddOptions<SessionMonitorOptions>();
        }

        services.AddSingleton<SessionMonitorService>();
        services.AddSingleton<ISessionMonitorService>(sp => sp.GetRequiredService<SessionMonitorService>());

        services.AddHttpContextAccessor();
        services.AddSingleton<IStartupFilter, SessionMonitorStartupFilter>();
        services.AddScoped<SessionMonitorCircuitContext>();
        services.AddScoped<CircuitHandler, SessionMonitorCircuitHandler>();

        return services;
    }

    /// <summary>
    /// Adds session monitoring with configuration from an <see cref="IConfiguration"/> section.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configurationSection">Configuration section (e.g. "SessionMonitor").</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSessionMonitoring(
        this IServiceCollection services,
        IConfiguration configurationSection)
    {
        services.Configure<SessionMonitorOptions>(configurationSection);
        return services.AddSessionMonitoring();
    }
}
