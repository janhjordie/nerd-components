namespace TheNerdCollective.Blazor.Observability;

/// <summary>Pluggable telemetry query backend (SigNoz, Prometheus, in-process).</summary>
public interface IObservabilityBackend
{
    /// <summary>Backend identifier, e.g. <c>signoz</c>.</summary>
    string BackendId { get; }

    /// <summary>Lists services visible to the backend in the given time window.</summary>
    Task<IReadOnlyList<ObservabilityServiceInfo>> ListServicesAsync(
        ObservabilityQueryContext context,
        CancellationToken cancellationToken = default);

    /// <summary>Queries a time series for a preset panel.</summary>
    Task<ObservabilityTimeSeriesResult> QueryTimeSeriesAsync(
        ObservabilityPanelQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>Queries a single scalar value for a preset panel (typically the latest point).</summary>
    Task<ObservabilityScalarResult> QueryScalarAsync(
        ObservabilityPanelQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>Returns a coarse health summary for a service.</summary>
    Task<ObservabilityHealthSummary> GetHealthSummaryAsync(
        string serviceName,
        ObservabilityQueryContext context,
        CancellationToken cancellationToken = default);
}
