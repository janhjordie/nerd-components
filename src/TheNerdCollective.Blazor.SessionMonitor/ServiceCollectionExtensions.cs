using Microsoft.AspNetCore.Components.Server.Circuits;
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
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSessionMonitoring(this IServiceCollection services)
    {
        // Register the monitoring service as singleton to maintain state across app lifetime
        services.AddSingleton<SessionMonitorService>();
        services.AddSingleton<ISessionMonitorService>(sp => sp.GetRequiredService<SessionMonitorService>());
        
        // Register the circuit handler
        services.AddScoped<CircuitHandler, SessionMonitorCircuitHandler>();

        return services;
    }
}
