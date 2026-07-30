namespace TheNerdCollective.Blazor.Observability;

/// <summary>Configuration for observability dashboard services.</summary>
public sealed class ObservabilityDashboardOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "NerdObservability";

    /// <summary>Selected backend implementation.</summary>
    public ObservabilityBackendKind Backend { get; set; } = ObservabilityBackendKind.SigNoz;

    /// <summary>Default OpenTelemetry <c>service.name</c> filter.</summary>
    public string DefaultServiceName { get; set; } = "app";

    /// <summary>Default lookback in minutes when callers omit an explicit range.</summary>
    public int DefaultLookbackMinutes { get; set; } = 15;

    /// <summary>SigNoz-specific settings.</summary>
    public SigNozBackendOptions SigNoz { get; set; } = new();

    /// <summary>In-process backend settings (future).</summary>
    public InProcessBackendOptions InProcess { get; set; } = new();

    /// <summary>External UI base URL (e.g. SigNoz). Components append service filters when possible.</summary>
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

/// <summary>SigNoz HTTP API settings.</summary>
public sealed class SigNozBackendOptions
{
    /// <summary>SigNoz base URL without trailing slash.</summary>
    public string BaseUrl { get; set; } = "http://127.0.0.1:8080";

    /// <summary>Bearer API token (server-side only).</summary>
    public string? ApiToken { get; set; }

    /// <summary>Optional org id for v2 session APIs (not required for API keys).</summary>
    public string? OrgId { get; set; }

    /// <summary>HTTP timeout for SigNoz queries.</summary>
    public TimeSpan HttpTimeout { get; set; } = TimeSpan.FromSeconds(15);
}

/// <summary>Placeholder for future in-process metrics backend.</summary>
public sealed class InProcessBackendOptions
{
    /// <summary>When true, collects runtime metrics via <see cref="System.Diagnostics.Metrics"/>.</summary>
    public bool Enabled { get; set; }
}
