using Microsoft.Extensions.Options;

namespace TheNerdCollective.Blazor.Observability;

/// <summary>Default dashboard facade over <see cref="IObservabilityBackend"/>.</summary>
public sealed class ObservabilityDashboardService : IObservabilityDashboardService
{
    private readonly IObservabilityBackend _backend;
    private readonly ObservabilityDashboardOptions _options;

    public ObservabilityDashboardService(IObservabilityBackend backend, IOptions<ObservabilityDashboardOptions> options)
    {
        _backend = backend;
        _options = options.Value;
    }

    /// <inheritdoc />
    public ObservabilityDashboardOptions Options => _options;

    /// <inheritdoc />
    public Task RefreshAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <inheritdoc />
    public async Task<ObservabilityOverviewSnapshot> GetOverviewAsync(
        string? serviceName = null,
        CancellationToken cancellationToken = default)
    {
        var resolvedService = string.IsNullOrWhiteSpace(serviceName)
            ? _options.DefaultServiceName
            : serviceName;
        var timeRange = _options.GetDefaultTimeRange();

        var requestRateTask = QueryScalarPanelAsync(ObservabilityPanelId.RequestRate, resolvedService, timeRange, cancellationToken);
        var p95Task = QueryScalarPanelAsync(ObservabilityPanelId.P95Latency, resolvedService, timeRange, cancellationToken);
        var error5xxTask = QueryScalarPanelAsync(ObservabilityPanelId.ErrorRate5xx, resolvedService, timeRange, cancellationToken);
        var errorPctTask = QueryScalarPanelAsync(ObservabilityPanelId.ErrorPercentage, resolvedService, timeRange, cancellationToken);

        await Task.WhenAll(requestRateTask, p95Task, error5xxTask, errorPctTask).ConfigureAwait(false);

        var requestRate = await requestRateTask.ConfigureAwait(false);
        var p95 = await p95Task.ConfigureAwait(false);
        var error5xx = await error5xxTask.ConfigureAwait(false);
        var errorPct = await errorPctTask.ConfigureAwait(false);

        var health = ResolveHealth(error5xx, errorPct, p95);

        return new ObservabilityOverviewSnapshot(
            resolvedService,
            requestRate,
            p95,
            error5xx,
            errorPct,
            health,
            DateTimeOffset.UtcNow);
    }

    /// <inheritdoc />
    public Task<ObservabilityTimeSeriesResult> GetPanelAsync(
        ObservabilityPanelId panelId,
        string serviceName,
        ObservabilityTimeRange timeRange,
        CancellationToken cancellationToken = default)
    {
        var query = new ObservabilityPanelQuery(panelId, serviceName, timeRange);
        return _backend.QueryTimeSeriesAsync(query, cancellationToken);
    }

    /// <inheritdoc />
    public Uri? GetExternalDashboardUrl(string? serviceName = null)
    {
        if (string.IsNullOrWhiteSpace(_options.ExternalDashboardBaseUrl))
        {
            return null;
        }

        var baseUrl = _options.ExternalDashboardBaseUrl.TrimEnd('/');
        var resolvedService = string.IsNullOrWhiteSpace(serviceName)
            ? _options.DefaultServiceName
            : serviceName;

        var encoded = Uri.EscapeDataString(resolvedService);
        return new Uri($"{baseUrl}/services?service={encoded}");
    }

    private async Task<ObservabilityScalarResult?> QueryScalarPanelAsync(
        ObservabilityPanelId panelId,
        string serviceName,
        ObservabilityTimeRange timeRange,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _backend.QueryScalarAsync(
                new ObservabilityPanelQuery(panelId, serviceName, timeRange),
                cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return null;
        }
    }

    private ObservabilityHealthStatus ResolveHealth(
        ObservabilityScalarResult? error5xx,
        ObservabilityScalarResult? errorPct,
        ObservabilityScalarResult? p95)
    {
        if (errorPct?.Value >= _options.UnhealthyErrorPercentage)
        {
            return ObservabilityHealthStatus.Unhealthy;
        }

        if (error5xx?.Value > _options.Degraded5xxRatePerSecond
            || p95?.Value > _options.DegradedP95LatencyMs)
        {
            return ObservabilityHealthStatus.Degraded;
        }

        if (error5xx is null && errorPct is null && p95 is null)
        {
            return ObservabilityHealthStatus.Unknown;
        }

        return ObservabilityHealthStatus.Healthy;
    }
}
