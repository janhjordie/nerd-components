using Microsoft.Extensions.Logging;
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
        if (_options.Colors.Count == 0)
        {
            return Task.CompletedTask;
        }

        if (_options.FailOnAccessibilityFailuresAtStartup)
        {
            NerdDesignTokenTools.AssertAccessibilityCompliance(_options);
            NerdStyleGuardTools.AssertPlacementCompliance(_options);
            return Task.CompletedTask;
        }

        if (_options.WarnOnAccessibilityFailuresAtStartup)
        {
            NerdDesignTokenTools.LogAccessibilityWarnings(_options, _logger);
            foreach (var violation in NerdStyleGuardTools.ValidatePlacements(_options))
            {
                _logger.LogWarning(
                    "Design token style guard ({Placement}/{Role}): {ContrastRatio:0.0}:1 < {RequiredRatio:0.0}:1 ({Foreground} on {Background}).",
                    violation.Placement,
                    violation.Role,
                    violation.ContrastRatio,
                    violation.RequiredRatio,
                    violation.Foreground,
                    violation.Background);
            }
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
