using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TheNerdCollective.Blazor.Observability.SigNoz;

/// <summary>Runs capability discovery on startup when <see cref="SigNozBackendOptions.DiscoverOnStartup"/> is enabled.</summary>
public sealed class SigNozCapabilityDiscoveryHostedService(
    ISigNozCapabilityDiscovery discovery,
    ISigNozRuntimeProfileProvider profileProvider,
    IOptions<SigNozBackendOptions> options,
    ILogger<SigNozCapabilityDiscoveryHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.DiscoverOnStartup)
        {
            return;
        }

        try
        {
            var profile = await discovery.DiscoverAsync(cancellationToken).ConfigureAwait(false);
            profileProvider.SetProfile(profile);
            logger.LogInformation(
                "SigNoz capability discovery: version={Version}, path={Path}, schema={Schema}",
                profile.SigNozVersion ?? "?",
                profile.QueryRangePath,
                profile.RecommendedSchemaVersion ?? "(omit)");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "SigNoz capability discovery failed; using configured defaults");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
