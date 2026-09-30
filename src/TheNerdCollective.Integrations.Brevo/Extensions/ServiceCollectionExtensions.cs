// Licensed under the Apache License, Version 2.0.
// See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace TheNerdCollective.Integrations.Brevo.Extensions;

/// <summary>
/// Registers the Brevo API client.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="BrevoService"/> from the "Brevo" configuration section.
    /// </summary>
    /// <remarks>
    /// Configure in appsettings.json:
    /// <code>
    /// {
    ///   "Brevo": {
    ///     "ApiKey": "your_api_key"
    ///   }
    /// }
    /// </code>
    /// </remarks>
    public static IServiceCollection AddBrevoIntegration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<BrevoOptions>(configuration.GetSection("Brevo"));
        services
            .AddHttpClient<BrevoService>()
            .AddPolicyHandler(GetRetryPolicy());

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(response => response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}
