// Licensed under the Apache License, Version 2.0.
// See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TheNerdCollective.Integrations.Mollie.Extensions;

/// <summary>
/// Dependency-injection registration for the Mollie API client.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Mollie API client and binds its API key from configuration.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="sectionName">The configuration section containing <c>ApiKey</c>.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddMollieIntegration(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = MollieOptions.DefaultSectionName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);

        services
            .AddOptions<MollieOptions>()
            .Bind(configuration.GetSection(sectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.ApiKey), "A Mollie API key is required.")
            .ValidateOnStart();

        services.AddHttpClient<MollieApiClient>();
        return services;
    }
}
