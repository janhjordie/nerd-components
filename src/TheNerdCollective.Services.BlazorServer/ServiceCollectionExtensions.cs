namespace TheNerdCollective.Services.BlazorServer;

using Microsoft.AspNetCore.Components.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

/// <summary>
/// Extension methods for registering Blazor Server circuit services and configuration.
/// This enables shared circuit configuration across multiple Blazor Server applications.
/// 
/// Usage in Program.cs:
///   builder.Services.AddBlazorServerCircuitServices(builder.Configuration, builder.Environment);
///   builder.Host.ConfigureBlazorServerCircuitShutdown();
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Blazor Server circuit configuration to the dependency injection container.
    /// Handles long-running sessions and graceful reconnection.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <param name="environment">Host environment</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddBlazorServerCircuitServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Configure Circuit Options to handle long-running sessions and prevent abrupt disconnections
        services.Configure<CircuitOptions>(options =>
        {
            // Dev: 5s for fast teardown (CircuitDefaults default).
            // Production: 3 minutes so users survive brief network blips,
            // LB re-routes, and Container Apps health-probe failovers.
            options.DisconnectedCircuitRetentionPeriod = environment.IsDevelopment()
                ? CircuitDefaults.DisconnectedCircuitRetentionPeriod
                : TimeSpan.FromMinutes(3);

            // Maximum number of circuits to retain per session
            options.DisconnectedCircuitMaxRetained = CircuitDefaults.DisconnectedCircuitMaxRetained;

            // Timeout for JS interop calls (default: 1 minute)
            options.JSInteropDefaultCallTimeout = CircuitDefaults.JSInteropDefaultCallTimeout;

            // Increase detailed errors in development for debugging
            options.DetailedErrors = environment.IsDevelopment();
        });

        return services;
    }

    /// <summary>
    /// Configures graceful shutdown timeout for Blazor Server circuits.
    /// Reduces default shutdown timeout from 30 seconds to 5 seconds for faster development cycles.
    /// </summary>
    /// <param name="builder">The host builder</param>
    /// <returns>The host builder for chaining</returns>
    public static IHostBuilder ConfigureBlazorServerCircuitShutdown(this IHostBuilder builder)
    {
        builder.ConfigureHostOptions(options =>
        {
            options.ShutdownTimeout = TimeSpan.FromSeconds(5);
        });

        return builder;
    }
}
