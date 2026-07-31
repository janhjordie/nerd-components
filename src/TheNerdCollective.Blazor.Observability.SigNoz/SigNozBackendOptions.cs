namespace TheNerdCollective.Blazor.Observability.SigNoz;

/// <summary>SigNoz HTTP API settings for <see cref="SigNozObservabilityBackend"/>.</summary>
public sealed class SigNozBackendOptions
{
    /// <summary>Default query_range path on SigNoz v5.</summary>
    public const string DefaultQueryRangePath = "/api/v5/query_range";

    /// <summary>Configuration subsection under <see cref="ObservabilityDashboardOptions.SectionName"/>.</summary>
    public const string SectionName = "SigNoz";

    /// <summary>SigNoz base URL without trailing slash.</summary>
    public string BaseUrl { get; set; } = "http://127.0.0.1:8080";

    /// <summary>SigNoz service-account API key (server-side only; sent as SIGNOZ-API-KEY).</summary>
    public string? ApiToken { get; set; }

    /// <summary>Optional org id for v2 session APIs (not required for API keys).</summary>
    public string? OrgId { get; set; }

    /// <summary>HTTP timeout for SigNoz queries.</summary>
    public TimeSpan HttpTimeout { get; set; } = TimeSpan.FromSeconds(15);

    /// <summary>Override query_range path (e.g. <c>/api/v4/query_range</c>).</summary>
    public string? QueryRangePath { get; set; }

    /// <summary>Override schemaVersion field. <c>null</c> omits the field (recommended).</summary>
    public string? SchemaVersion { get; set; }

    /// <summary>Probe SigNoz on startup to discover path and schema.</summary>
    public bool DiscoverOnStartup { get; set; }

    /// <summary>Service name used during capability discovery probes.</summary>
    public string DiscoveryServiceName { get; set; } = "nerd-consent-host";

    /// <summary>Lookback window for discovery probes.</summary>
    public int DiscoveryLookbackMinutes { get; set; } = 15;

    /// <summary>Candidate query_range paths for discovery.</summary>
    public string[] QueryRangePathCandidates { get; set; } =
    [
        DefaultQueryRangePath,
        "/api/v4/query_range"
    ];

    /// <summary>Candidate schema versions for discovery. Empty string = omit field.</summary>
    public string[] SchemaVersionCandidates { get; set; } =
    [
        "",
        "v1",
        "v2"
    ];
}
