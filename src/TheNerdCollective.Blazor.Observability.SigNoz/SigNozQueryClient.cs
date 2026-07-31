using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using TheNerdCollective.Blazor.Observability;

namespace TheNerdCollective.Blazor.Observability.SigNoz;

/// <summary>Executes SigNoz query_range with mutators, profile path/schema, and parser chain.</summary>
public sealed class SigNozQueryClient(
    IHttpClientFactory httpClientFactory,
    IOptions<SigNozBackendOptions> signozOptions,
    ISigNozRuntimeProfileProvider profileProvider,
    SigNozResponseParserCoordinator parserCoordinator,
    IEnumerable<ISigNozQueryMutator> mutators) : ISigNozQueryClient
{
    public async Task<ObservabilityTimeSeriesResult> QueryTimeSeriesAsync(
        ObservabilityPanelQuery query,
        SigNozQueryOverrides? overrides = null,
        CancellationToken cancellationToken = default)
    {
        var (statusCode, json) = await SendQueryRangeAsync(query, overrides, cancellationToken).ConfigureAwait(false);
        var context = BuildParseContext(query.PanelId, overrides, statusCode);
        return parserCoordinator.ParseTimeSeries(json, context);
    }

    public async Task<(int? StatusCode, int PointCount)> ProbeTimeSeriesAsync(
        ObservabilityPanelQuery query,
        SigNozQueryOverrides overrides,
        CancellationToken cancellationToken = default)
    {
        var (statusCode, json) = await SendQueryRangeAsync(query, overrides, cancellationToken).ConfigureAwait(false);
        if (statusCode is null or < 200 or >= 300)
        {
            return (statusCode, 0);
        }

        var context = BuildParseContext(query.PanelId, overrides, statusCode);
        var series = parserCoordinator.ParseTimeSeries(json, context);
        return (statusCode, series.Points.Count);
    }

    private async Task<(int? StatusCode, string Json)> SendQueryRangeAsync(
        ObservabilityPanelQuery query,
        SigNozQueryOverrides? overrides,
        CancellationToken cancellationToken)
    {
        var signoz = signozOptions.Value;
        var path = overrides?.QueryRangePath
            ?? profileProvider.ResolveQueryRangePath();
        var schemaVersion = overrides?.SchemaVersion ?? profileProvider.ResolveSchemaVersion();
        var body = BuildRequestBody(query, path, schemaVersion, overrides);

        var client = httpClientFactory.CreateClient(SigNozObservabilityBackend.HttpClientName);
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{signoz.BaseUrl.TrimEnd('/')}{path}")
        {
            Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json")
        };

        if (!string.IsNullOrWhiteSpace(signoz.ApiToken))
        {
            request.Headers.TryAddWithoutValidation("SIGNOZ-API-KEY", signoz.ApiToken);
        }

        using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return ((int)response.StatusCode, json);
    }

    private JsonObject BuildRequestBody(
        ObservabilityPanelQuery query,
        string path,
        string? schemaVersion,
        SigNozQueryOverrides? overrides)
    {
        var body = SigNozQueryBuilder.BuildQueryRangeRequest(query, schemaVersion);
        var context = new SigNozQueryContext(path, schemaVersion, overrides);
        foreach (var mutator in mutators)
        {
            mutator.MutateQueryRangeBody(body, query, context);
        }

        if (!string.IsNullOrWhiteSpace(overrides?.FilterExpression)
            && body["compositeQuery"]?["queries"]?[0]?["spec"]?["filter"] is JsonObject filterNode)
        {
            filterNode["expression"] = overrides.FilterExpression;
        }

        return body;
    }

    private SigNozParseContext BuildParseContext(
        ObservabilityPanelId panelId,
        SigNozQueryOverrides? overrides,
        int? statusCode) =>
        new(
            panelId,
            overrides?.QueryRangePath ?? profileProvider.ResolveQueryRangePath(),
            overrides?.SchemaVersion ?? profileProvider.ResolveSchemaVersion(),
            statusCode);
}
