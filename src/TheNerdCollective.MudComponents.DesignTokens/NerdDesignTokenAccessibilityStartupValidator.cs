using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TheNerdCollective.MudComponents.DesignTokens;

internal sealed class NerdDesignTokenAccessibilityStartupValidator : IHostedService
{
    private readonly NerdDesignTokenOptions _options;
    private readonly ILogger<NerdDesignTokenAccessibilityStartupValidator> _logger;

    public NerdDesignTokenAccessibilityStartupValidator(
        NerdDesignTokenOptions options,
        ILogger<NerdDesignTokenAccessibilityStartupValidator> logger)
    {
        _options = options;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_options.WarnOnAccessibilityFailuresAtStartup)
        {
            NerdDesignTokenTools.LogAccessibilityWarnings(_options, _logger);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
