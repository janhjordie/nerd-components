using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using TheNerdCollective.Blazor.Observability;

namespace TheNerdCollective.Blazor.Observability.SigNoz;

/// <summary>Probes live SigNoz to discover working query path and schema version.</summary>
public sealed class SigNozCapabilityDiscovery(
    IHttpClientFactory httpClientFactory,
    IOptions<SigNozBackendOptions> signozOptions,
    SigNozResponseParserCoordinator parserCoordinator) : ISigNozCapabilityDiscovery
{
    public async Task<SigNozRuntimeProfile> DiscoverAsync(CancellationToken cancellationToken = default)
    {
        var options = signozOptions.Value;
        var http = httpClientFactory.CreateClient(SigNozObservabilityBackend.HttpClientName);
        var version = await FetchVersionAsync(http, options, cancellationToken).ConfigureAwait(false);
        var timeRange = ObservabilityTimeRange.LastMinutes(options.DiscoveryLookbackMinutes);
        var canonicalQuery = new ObservabilityPanelQuery(
            ObservabilityPanelId.HostMemoryUtilization,
            options.DiscoveryServiceName,
            timeRange);
        var canonicalBody = SigNozQueryBuilder.BuildQueryRangeRequest(canonicalQuery, schemaVersion: null);

        var endpointResults = new List<(string Path, bool Ok, int Points)>();
        foreach (var path in options.QueryRangePathCandidates)
        {
            var (ok, _, points) = await SendProbeAsync(http, options, path, canonicalBody, cancellationToken)
                .ConfigureAwait(false);
            endpointResults.Add((path, ok, points));
        }

        var bestPath = endpointResults
            .Where(r => r.Ok && r.Points > 0)
            .OrderByDescending(r => r.Points)
            .Select(r => r.Path)
            .FirstOrDefault()
            ?? endpointResults.FirstOrDefault(r => r.Ok).Path
            ?? options.QueryRangePath
            ?? SigNozBackendOptions.DefaultQueryRangePath;

        string? recommendedSchema = null;
        var schemaCandidates = options.SchemaVersionCandidates;
        foreach (var candidate in schemaCandidates)
        {
            var schema = string.IsNullOrEmpty(candidate) ? null : candidate;
            var body = SigNozQueryBuilder.BuildQueryRangeRequest(canonicalQuery, schema);
            var (ok, _, points) = await SendProbeAsync(http, options, bestPath, body, cancellationToken)
                .ConfigureAwait(false);
            if (ok && points > 0)
            {
                recommendedSchema = schema;
                if (schema is null)
                {
                    break;
                }
            }
        }

        return new SigNozRuntimeProfile(
            version.Version,
            bestPath,
            recommendedSchema,
            DateTimeOffset.UtcNow);
    }

    private async Task<SigNozVersionInfo> FetchVersionAsync(
        HttpClient http,
        SigNozBackendOptions options,
        CancellationToken cancellationToken)
    {
        var (healthOk, healthStatus) = await TryGetStatusAsync(http, options, "/api/v1/health", cancellationToken)
            .ConfigureAwait(false);
        var versionJson = await TryGetBodyAsync(http, options, "/api/v1/version", cancellationToken)
            .ConfigureAwait(false);
        return ParseVersion(versionJson, healthStatus, healthOk);
    }

    private async Task<(bool Ok, int Status, int Points)> SendProbeAsync(
        HttpClient http,
        SigNozBackendOptions options,
        string path,
        JsonObject body,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{options.BaseUrl.TrimEnd('/')}{path}")
        {
            Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json")
        };

        if (!string.IsNullOrWhiteSpace(options.ApiToken))
        {
            request.Headers.TryAddWithoutValidation("SIGNOZ-API-KEY", options.ApiToken);
        }

        using var response = await http.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var status = (int)response.StatusCode;
        if (!response.IsSuccessStatusCode)
        {
            return (false, status, 0);
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var context = new SigNozParseContext(
            ObservabilityPanelId.HostMemoryUtilization,
            path,
            null,
            status);
        var series = parserCoordinator.ParseTimeSeries(json, context);
        return (true, status, series.Points.Count);
    }

    private static async Task<(bool ok, int? status)> TryGetStatusAsync(
        HttpClient http,
        SigNozBackendOptions options,
        string path,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await http.GetAsync($"{options.BaseUrl.TrimEnd('/')}{path}", cancellationToken)
                .ConfigureAwait(false);
            return (response.IsSuccessStatusCode, (int)response.StatusCode);
        }
        catch
        {
            return (false, null);
        }
    }

    private static async Task<string?> TryGetBodyAsync(
        HttpClient http,
        SigNozBackendOptions options,
        string path,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await http.GetAsync($"{options.BaseUrl.TrimEnd('/')}{path}", cancellationToken)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false)
                : null;
        }
        catch
        {
            return null;
        }
    }

    private static SigNozVersionInfo ParseVersion(string? json, int? healthStatus, bool healthOk)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new SigNozVersionInfo(null, null, null, healthStatus, healthOk);
        }

        var root = JsonNode.Parse(json) as JsonObject;
        var data = root?["data"] as JsonObject ?? root;
        return new SigNozVersionInfo(
            data?["version"]?.GetValue<string>(),
            data?["ee"]?.GetValue<string>(),
            data?["setupCompleted"]?.GetValue<bool>(),
            healthStatus,
            healthOk);
    }
}
