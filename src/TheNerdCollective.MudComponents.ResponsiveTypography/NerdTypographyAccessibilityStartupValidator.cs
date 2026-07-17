using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

internal sealed class NerdTypographyAccessibilityStartupValidator : IHostedService
{
    private readonly NerdResponsiveTypographyOptions _options;
    private readonly ILogger<NerdTypographyAccessibilityStartupValidator> _logger;

    public NerdTypographyAccessibilityStartupValidator(
        NerdResponsiveTypographyOptions options,
        ILogger<NerdTypographyAccessibilityStartupValidator> logger)
    {
        _options = options;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_options.WarnOnAccessibilityFailuresAtStartup)
        {
            NerdTypographyAccessibilityTools.LogAccessibilityWarnings(_options, _logger);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
