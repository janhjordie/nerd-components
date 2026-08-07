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
            if (NerdStyleGuardTools.CssPaintsOutlinedBrandChromeWithContrastText(_options))
            {
                throw new InvalidOperationException(
                    "Style guard failed: BrandChrome outlined controls are painted with OnBrandChrome (ContrastText) — white-on-light on page-surface.");
            }

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

            if (NerdStyleGuardTools.CssPaintsOutlinedBrandChromeWithContrastText(_options))
            {
                _logger.LogWarning(
                    "Design token style guard: BrandChrome outlined MudButton CSS uses OnBrandChrome (ContrastText) — expect white-on-white on page-surface. Use BrandChrome accent for outlined/text.");
            }

            foreach (var warning in NerdStyleGuardTools.ValidateOutlinedStatusIntentWarnings(_options))
            {
                _logger.LogWarning(
                    "Design token style guard NRDT001 risk ({Placement}/{Role}): {ContrastRatio:0.0}:1 < {RequiredRatio:0.0}:1 ({Foreground} on {Background}). Do not use this intent on Outlined/Text — use BrandChrome or Filled.",
                    warning.Placement,
                    warning.Role,
                    warning.ContrastRatio,
                    warning.RequiredRatio,
                    warning.Foreground,
                    warning.Background);
            }
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
