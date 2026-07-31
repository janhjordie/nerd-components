using TheNerdCollective.Blazor.Observability;

namespace TheNerdCollective.Blazor.Observability.SigNoz;

/// <summary>Low-level SigNoz query client with override hooks (used by backend and host diagnostics).</summary>
public interface ISigNozQueryClient
{
    Task<ObservabilityTimeSeriesResult> QueryTimeSeriesAsync(
        ObservabilityPanelQuery query,
        SigNozQueryOverrides? overrides = null,
        CancellationToken cancellationToken = default);

    Task<(int? StatusCode, int PointCount)> ProbeTimeSeriesAsync(
        ObservabilityPanelQuery query,
        SigNozQueryOverrides overrides,
        CancellationToken cancellationToken = default);
}
