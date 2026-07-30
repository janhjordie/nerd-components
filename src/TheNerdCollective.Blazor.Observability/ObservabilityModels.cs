namespace TheNerdCollective.Blazor.Observability;

/// <summary>Supported observability backend adapter kinds (metadata only).</summary>
public enum ObservabilityBackendKind
{
    SigNoz,
    Grafana,
    Prometheus,
    InProcess
}

/// <summary>Preset dashboard panels aligned with SigNoz span metrics.</summary>
public enum ObservabilityPanelId
{
    RequestRate,
    P95Latency,
    ErrorRate5xx,
    ErrorPercentage,
    ActiveCircuits,
    RuntimeGcHeap
}

/// <summary>Coarse health status for ops dashboards.</summary>
public enum ObservabilityHealthStatus
{
    Unknown,
    Healthy,
    Degraded,
    Unhealthy
}

/// <summary>Time window for observability queries.</summary>
public sealed record ObservabilityTimeRange(
    DateTimeOffset Start,
    DateTimeOffset End,
    TimeSpan Step)
{
    public static TimeSpan DefaultStep { get; } = TimeSpan.FromMinutes(1);

    public ObservabilityTimeRange(DateTimeOffset start, DateTimeOffset end, TimeSpan? step = null)
        : this(start, end, step ?? DefaultStep)
    {
    }

    public static ObservabilityTimeRange LastMinutes(int minutes, DateTimeOffset? now = null)
    {
        var end = now ?? DateTimeOffset.UtcNow;
        return new ObservabilityTimeRange(end.AddMinutes(-minutes), end);
    }
}

/// <summary>Backend-agnostic query context.</summary>
public sealed record ObservabilityQueryContext(
    DateTimeOffset Start,
    DateTimeOffset End,
    string? ServiceName = null);

/// <summary>Request for a preset panel query.</summary>
public sealed record ObservabilityPanelQuery(
    ObservabilityPanelId PanelId,
    string ServiceName,
    ObservabilityTimeRange TimeRange);

/// <summary>Single point in a time series.</summary>
public sealed record ObservabilityTimeSeriesPoint(DateTimeOffset Timestamp, double Value);

/// <summary>Time series query result.</summary>
public sealed record ObservabilityTimeSeriesResult(
    string Legend,
    string Unit,
    IReadOnlyList<ObservabilityTimeSeriesPoint> Points);

/// <summary>Scalar query result.</summary>
public sealed record ObservabilityScalarResult(double Value, string Unit, string Label);

/// <summary>Overview snapshot for dashboard cards.</summary>
public sealed record ObservabilityOverviewSnapshot(
    string ServiceName,
    ObservabilityScalarResult? RequestRate,
    ObservabilityScalarResult? P95LatencyMs,
    ObservabilityScalarResult? ErrorRate5xx,
    ObservabilityScalarResult? ErrorPercentage,
    ObservabilityHealthStatus Health,
    DateTimeOffset QueriedAtUtc);

/// <summary>Service metadata from the backend.</summary>
public sealed record ObservabilityServiceInfo(
    string Name,
    string? Environment,
    double? RequestRate,
    double? P95LatencyMs,
    double? ErrorRate);

/// <summary>Health summary for a single service.</summary>
public sealed record ObservabilityHealthSummary(
    string ServiceName,
    ObservabilityHealthStatus Status,
    string? Message,
    DateTimeOffset QueriedAtUtc);
