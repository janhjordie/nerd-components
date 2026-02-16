// Licensed under the Apache License, Version 2.0.
// See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace TheNerdCollective.Integrations.GitHub.Extensions;

/// <summary>
/// Extension methods for registering GitHub API integration services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds GitHub API integration services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration containing GitHub settings.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// Configure in appsettings.json:
    /// <code>
    /// {
    ///   "GitHub": {
    ///     "Token": "your_personal_access_token",
    ///     "Owner": "repository_owner",
    ///     "Repository": "repository_name"
    ///   }
    /// }
    /// </code>
    /// 
    /// Usage in Program.cs:
    /// <code>
    /// services.AddGitHubIntegration(configuration);
    /// </code>
    /// 
    /// Includes automatic retry policy with exponential backoff (3x retry) for transient failures:
    /// - HTTP 429 (Too Many Requests)
    /// - HTTP 500, 502, 503, 504 (Server Errors)
    /// - Network timeouts
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when services or configuration is null.</exception>
    public static IServiceCollection AddGitHubIntegration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<GitHubOptions>(configuration.GetSection("GitHub"));
        
        // Configure HttpClient with Polly retry policy
        services
            .AddHttpClient<GitHubService>()
            .AddPolicyHandler(GetRetryPolicy());

        return services;
    }

    /// <summary>
    /// Creates a Polly retry policy for transient HTTP errors with exponential backoff.
    /// </summary>
    /// <remarks>
    /// Policy handles:
    /// - 3 retry attempts
    /// - Exponential backoff: 2^n seconds (2, 4, 8 seconds)
    /// - HTTP 429 (Rate Limit), 500 (Internal Server Error), 502 (Bad Gateway), 503 (Service Unavailable), 504 (Gateway Timeout)
    /// - Transient network errors (timeouts, connection refused, etc.)
    /// </remarks>
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // 429
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // 2, 4, 8 seconds
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    // Optional: Add logging here for monitoring
                });
    }
}
