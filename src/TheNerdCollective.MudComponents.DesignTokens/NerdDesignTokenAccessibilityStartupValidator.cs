using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

internal sealed class NerdDesignTokenAccessibilityStartupValidator : Microsoft.Extensions.Hosting.IHostedService
{
    private readonly NerdDesignTokenOptions _options;
    private readonly Microsoft.Extensions.Logging.ILogger<NerdDesignTokenAccessibilityStartupValidator> _logger;

    public NerdDesignTokenAccessibilityStartupValidator(
        NerdDesignTokenOptions options,
        Microsoft.Extensions.Logging.ILogger<NerdDesignTokenAccessibilityStartupValidator> logger)
    {
        _options = options;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_options.Colors.Count == 0 || !_options.WarnOnAccessibilityFailuresAtStartup)
        {
            return Task.CompletedTask;
        }

        NerdDesignTokenTools.LogAccessibilityWarnings(_options, _logger);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
