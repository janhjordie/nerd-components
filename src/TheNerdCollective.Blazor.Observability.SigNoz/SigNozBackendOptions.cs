namespace TheNerdCollective.Blazor.Observability.SigNoz;

/// <summary>SigNoz HTTP API settings for <see cref="SigNozObservabilityBackend"/>.</summary>
public sealed class SigNozBackendOptions
{
    /// <summary>Configuration subsection under <see cref="ObservabilityDashboardOptions.SectionName"/>.</summary>
    public const string SectionName = "SigNoz";

    /// <summary>SigNoz base URL without trailing slash.</summary>
    public string BaseUrl { get; set; } = "http://127.0.0.1:8080";

    /// <summary>Bearer API token (server-side only).</summary>
    public string? ApiToken { get; set; }

    /// <summary>Optional org id for v2 session APIs (not required for API keys).</summary>
    public string? OrgId { get; set; }

    /// <summary>HTTP timeout for SigNoz queries.</summary>
    public TimeSpan HttpTimeout { get; set; } = TimeSpan.FromSeconds(15);
}
