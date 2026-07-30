using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using TheNerdCollective.Blazor.Observability;

namespace TheNerdCollective.Blazor.Observability.SigNoz;

/// <summary>SigNoz-backed observability queries via HTTP API.</summary>
public sealed class SigNozObservabilityBackend : IObservabilityBackend
{
    public const string HttpClientName = "TheNerdCollective.Blazor.Observability.SigNoz";

    private readonly HttpClient _httpClient;
    private readonly SigNozBackendOptions _signozOptions;
    private readonly ObservabilityDashboardOptions _dashboardOptions;

    public SigNozObservabilityBackend(
        IHttpClientFactory httpClientFactory,
        IOptions<SigNozBackendOptions> signozOptions,
        IOptions<ObservabilityDashboardOptions> dashboardOptions)
    {
        _httpClient = httpClientFactory.CreateClient(HttpClientName);
        _signozOptions = signozOptions.Value;
        _dashboardOptions = dashboardOptions.Value;
    }

    /// <inheritdoc />
    public string BackendId => "signoz";

    /// <inheritdoc />
    public async Task<IReadOnlyList<ObservabilityServiceInfo>> ListServicesAsync(
        ObservabilityQueryContext context,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            start = FormatRelativeTime(context.Start),
            end = FormatRelativeTime(context.End),
            tags = Array.Empty<object>()
        };

        using var request = CreateRequest(HttpMethod.Post, "/api/v1/services", payload);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return SigNozResponseParser.ParseServices(json);
    }

    /// <inheritdoc />
    public async Task<ObservabilityTimeSeriesResult> QueryTimeSeriesAsync(
        ObservabilityPanelQuery query,
        CancellationToken cancellationToken = default)
    {
        var body = SigNozQueryBuilder.BuildQueryRangeRequest(query);
        using var request = CreateJsonRequest(HttpMethod.Post, "/api/v4/query_range", body);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return SigNozResponseParser.ParseTimeSeries(json, query.PanelId);
    }

    /// <inheritdoc />
    public async Task<ObservabilityScalarResult> QueryScalarAsync(
        ObservabilityPanelQuery query,
        CancellationToken cancellationToken = default)
    {
        var series = await QueryTimeSeriesAsync(query, cancellationToken).ConfigureAwait(false);
        var definition = ObservabilityPanelCatalog.GetDefinition(query.PanelId);
        var value = series.Points.Count == 0 ? 0 : series.Points[^1].Value;
        return new ObservabilityScalarResult(value, definition.Unit, definition.Title);
    }

    /// <inheritdoc />
    public async Task<ObservabilityHealthSummary> GetHealthSummaryAsync(
        string serviceName,
        ObservabilityQueryContext context,
        CancellationToken cancellationToken = default)
    {
        var timeRange = new ObservabilityTimeRange(context.Start, context.End);
        var error5xx = await QueryScalarAsync(
            new ObservabilityPanelQuery(ObservabilityPanelId.ErrorRate5xx, serviceName, timeRange),
            cancellationToken).ConfigureAwait(false);
        var errorPct = await QueryScalarAsync(
            new ObservabilityPanelQuery(ObservabilityPanelId.ErrorPercentage, serviceName, timeRange),
            cancellationToken).ConfigureAwait(false);
        var p95 = await QueryScalarAsync(
            new ObservabilityPanelQuery(ObservabilityPanelId.P95Latency, serviceName, timeRange),
            cancellationToken).ConfigureAwait(false);

        var status = ObservabilityHealthStatus.Healthy;
        string? message = null;

        if (errorPct.Value >= _dashboardOptions.UnhealthyErrorPercentage)
        {
            status = ObservabilityHealthStatus.Unhealthy;
            message = $"Error rate {errorPct.Value:P1}";
        }
        else if (error5xx.Value > _dashboardOptions.Degraded5xxRatePerSecond
                 || p95.Value > _dashboardOptions.DegradedP95LatencyMs)
        {
            status = ObservabilityHealthStatus.Degraded;
            message = $"5xx={error5xx.Value:F3}/s, p95={p95.Value:F0}ms";
        }

        return new ObservabilityHealthSummary(serviceName, status, message, DateTimeOffset.UtcNow);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path, object payload)
    {
        var request = new HttpRequestMessage(method, BuildUri(path))
        {
            Content = JsonContent.Create(payload)
        };
        ApplyAuth(request);
        return request;
    }

    private HttpRequestMessage CreateJsonRequest(HttpMethod method, string path, JsonNode payload)
    {
        var json = payload.ToJsonString();
        var request = new HttpRequestMessage(method, BuildUri(path))
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        ApplyAuth(request);
        return request;
    }

    private void ApplyAuth(HttpRequestMessage request)
    {
        if (!string.IsNullOrWhiteSpace(_signozOptions.ApiToken))
        {
            request.Headers.TryAddWithoutValidation("SIGNOZ-API-KEY", _signozOptions.ApiToken);
        }
    }

    private Uri BuildUri(string path)
    {
        var baseUrl = _signozOptions.BaseUrl.TrimEnd('/');
        return new Uri($"{baseUrl}{path}");
    }

    private static string FormatRelativeTime(DateTimeOffset timestamp)
    {
        var delta = DateTimeOffset.UtcNow - timestamp.ToUniversalTime();
        if (delta <= TimeSpan.FromMinutes(1))
        {
            return "now-1m";
        }

        if (delta <= TimeSpan.FromHours(1))
        {
            return $"now-{(int)Math.Ceiling(delta.TotalMinutes)}m";
        }

        if (delta <= TimeSpan.FromDays(1))
        {
            return $"now-{(int)Math.Ceiling(delta.TotalHours)}h";
        }

        return $"now-{(int)Math.Ceiling(delta.TotalDays)}d";
    }
}
