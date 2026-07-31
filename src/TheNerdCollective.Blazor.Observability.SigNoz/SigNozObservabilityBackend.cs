using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using TheNerdCollective.Blazor.Observability;

namespace TheNerdCollective.Blazor.Observability.SigNoz;

/// <summary>SigNoz-backed observability queries via HTTP API.</summary>
public sealed class SigNozObservabilityBackend(
    IHttpClientFactory httpClientFactory,
    IOptions<SigNozBackendOptions> signozOptions,
    IOptions<ObservabilityDashboardOptions> dashboardOptions,
    ISigNozQueryClient queryClient,
    SigNozResponseParserCoordinator parserCoordinator) : IObservabilityBackend
{
    public const string HttpClientName = "TheNerdCollective.Blazor.Observability.SigNoz";

    private readonly SigNozBackendOptions _signozOptions = signozOptions.Value;
    private readonly ObservabilityDashboardOptions _dashboardOptions = dashboardOptions.Value;

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

        var client = httpClientFactory.CreateClient(HttpClientName);
        using var request = CreateRequest(client, HttpMethod.Post, "/api/v1/services", payload);
        using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var parseContext = new SigNozParseContext(
            ObservabilityPanelId.RequestRate,
            _signozOptions.QueryRangePath ?? SigNozBackendOptions.DefaultQueryRangePath,
            _signozOptions.SchemaVersion,
            (int)response.StatusCode);
        return parserCoordinator.ParseServices(json, parseContext);
    }

    /// <inheritdoc />
    public Task<ObservabilityTimeSeriesResult> QueryTimeSeriesAsync(
        ObservabilityPanelQuery query,
        CancellationToken cancellationToken = default) =>
        queryClient.QueryTimeSeriesAsync(query, overrides: null, cancellationToken);

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

    private HttpRequestMessage CreateRequest(HttpClient client, HttpMethod method, string path, object payload)
    {
        var request = new HttpRequestMessage(method, $"{_signozOptions.BaseUrl.TrimEnd('/')}{path}")
        {
            Content = JsonContent.Create(payload)
        };

        if (!string.IsNullOrWhiteSpace(_signozOptions.ApiToken))
        {
            request.Headers.TryAddWithoutValidation("SIGNOZ-API-KEY", _signozOptions.ApiToken);
        }

        return request;
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
