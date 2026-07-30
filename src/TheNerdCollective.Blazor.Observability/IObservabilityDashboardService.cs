namespace TheNerdCollective.Blazor.Observability;

/// <summary>Facade used by Blazor components — panel presets and overview aggregation.</summary>
public interface IObservabilityDashboardService
{
    /// <summary>Configured dashboard options.</summary>
    ObservabilityDashboardOptions Options { get; }

    /// <summary>Invalidates any cached overview snapshot.</summary>
    Task RefreshAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets scalar overview metrics for the default or specified service.</summary>
    Task<ObservabilityOverviewSnapshot> GetOverviewAsync(
        string? serviceName = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a preset panel time series.</summary>
    Task<ObservabilityTimeSeriesResult> GetPanelAsync(
        ObservabilityPanelId panelId,
        string serviceName,
        ObservabilityTimeRange timeRange,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the latest scalar value for a preset panel.</summary>
    Task<ObservabilityScalarResult?> GetScalarAsync(
        ObservabilityPanelId panelId,
        string serviceName,
        ObservabilityTimeRange? timeRange = null,
        CancellationToken cancellationToken = default);

    /// <summary>External deep-link URL (SigNoz/Grafana UI) when configured.</summary>
    Uri? GetExternalDashboardUrl(string? serviceName = null);
}
