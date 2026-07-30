namespace TheNerdCollective.Blazor.Observability;

/// <summary>Configuration for observability dashboard services.</summary>
public sealed class ObservabilityDashboardOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "NerdObservability";

    /// <summary>Default OpenTelemetry <c>service.name</c> filter.</summary>
    public string DefaultServiceName { get; set; } = "app";

    /// <summary>Default lookback in minutes when callers omit an explicit range.</summary>
    public int DefaultLookbackMinutes { get; set; } = 15;

    /// <summary>In-process backend settings (future).</summary>
    public InProcessBackendOptions InProcess { get; set; } = new();

    /// <summary>External UI base URL (e.g. SigNoz or Grafana). Components append service filters when possible.</summary>
    public string? ExternalDashboardBaseUrl { get; set; }

    /// <summary>When true, registers <see cref="ObservabilityDashboardEndpointExtensions"/> routes.</summary>
    public bool EnableMinimalApi { get; set; } = true;

    /// <summary>5xx rate (req/s) above which health is <see cref="ObservabilityHealthStatus.Degraded"/>.</summary>
    public double Degraded5xxRatePerSecond { get; set; } = 0.01;

    /// <summary>Error percentage above which health is <see cref="ObservabilityHealthStatus.Unhealthy"/>.</summary>
    public double UnhealthyErrorPercentage { get; set; } = 0.05;

    /// <summary>P95 latency (ms) above which health is <see cref="ObservabilityHealthStatus.Degraded"/>.</summary>
    public double DegradedP95LatencyMs { get; set; } = 2000;

    /// <summary>Resolves the default time range from UTC now.</summary>
    public ObservabilityTimeRange GetDefaultTimeRange(DateTimeOffset? now = null) =>
        ObservabilityTimeRange.LastMinutes(DefaultLookbackMinutes, now);
}

/// <summary>Placeholder for future in-process metrics backend.</summary>
public sealed class InProcessBackendOptions
{
    /// <summary>When true, collects runtime metrics via <see cref="System.Diagnostics.Metrics"/>.</summary>
    public bool Enabled { get; set; }
}
