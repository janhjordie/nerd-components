using System.Text.Json.Nodes;
using TheNerdCollective.Blazor.Observability;
using TheNerdCollective.Blazor.Observability.SigNoz;

namespace TheNerdCollective.Blazor.Observability.SigNoz.Tests;

public sealed class SigNozQueryBuilderTests
{
    [Fact]
    public void BuildQueryRangeRequest_request_rate_uses_signoz_latency_count()
    {
        var query = CreateQuery(ObservabilityPanelId.RequestRate);
        var body = SigNozQueryBuilder.BuildQueryRangeRequest(query);

        Assert.Equal("builder", body["compositeQuery"]?["queryType"]?.GetValue<string>());
        var spec = body["compositeQuery"]?["queries"]?[0]?["spec"] as JsonObject;
        Assert.NotNull(spec);
        Assert.Equal("signoz_latency.count", spec["aggregations"]?[0]?["metricName"]?.GetValue<string>());
        Assert.Equal("rate", spec["aggregations"]?[0]?["timeAggregation"]?.GetValue<string>());
        Assert.Contains("service.name = 'nerd-consent-host'", spec["filter"]?["expression"]?.GetValue<string>());
    }

    [Fact]
    public void BuildQueryRangeRequest_p95_uses_latency_bucket()
    {
        var query = CreateQuery(ObservabilityPanelId.P95Latency);
        var body = SigNozQueryBuilder.BuildQueryRangeRequest(query);
        var spec = body["compositeQuery"]?["queries"]?[0]?["spec"] as JsonObject;

        Assert.Equal("signoz_latency.bucket", spec?["aggregations"]?[0]?["metricName"]?.GetValue<string>());
        Assert.Equal("p95", spec?["aggregations"]?[0]?["timeAggregation"]?.GetValue<string>());
    }

    [Fact]
    public void BuildQueryRangeRequest_5xx_filters_http_status_code()
    {
        var query = CreateQuery(ObservabilityPanelId.ErrorRate5xx);
        var body = SigNozQueryBuilder.BuildQueryRangeRequest(query);
        var expression = body["compositeQuery"]?["queries"]?[0]?["spec"]?["filter"]?["expression"]?.GetValue<string>();

        Assert.Contains("http_status_code >= 500", expression);
    }

    [Fact]
    public void BuildQueryRangeRequest_error_percentage_uses_formula()
    {
        var query = CreateQuery(ObservabilityPanelId.ErrorPercentage);
        var body = SigNozQueryBuilder.BuildQueryRangeRequest(query);
        var composite = body["compositeQuery"] as JsonObject;

        Assert.NotNull(composite);
        Assert.Equal(2, composite["queries"]?.AsArray().Count);
        Assert.Equal("A/B", composite["formulas"]?[0]?["expression"]?.GetValue<string>());
    }

    [Fact]
    public void BuildServiceFilter_escapes_single_quotes()
    {
        var filter = SigNozQueryBuilder.BuildServiceFilter("app's-service", "http_status_code >= 500");
        Assert.Contains("app\\'s-service", filter);
    }

    private static ObservabilityPanelQuery CreateQuery(ObservabilityPanelId panelId) =>
        new(panelId, "nerd-consent-host", ObservabilityTimeRange.LastMinutes(15));
}
