using System.Text.Json.Nodes;
using TheNerdCollective.Blazor.Observability;

namespace TheNerdCollective.Blazor.Observability.SigNoz;

/// <summary>Builds SigNoz v4 <c>query_range</c> payloads for preset panels.</summary>
public static class SigNozQueryBuilder
{
    /// <summary>Builds a SigNoz <c>/api/v4/query_range</c> request body.</summary>
    public static JsonObject BuildQueryRangeRequest(ObservabilityPanelQuery query)
    {
        var stepSeconds = Math.Max(1, (int)query.TimeRange.Step.TotalSeconds);
        var startMs = query.TimeRange.Start.ToUnixTimeMilliseconds();
        var endMs = query.TimeRange.End.ToUnixTimeMilliseconds();

        return new JsonObject
        {
            ["start"] = startMs,
            ["end"] = endMs,
            ["step"] = stepSeconds,
            ["variables"] = new JsonObject(),
            ["compositeQuery"] = BuildCompositeQuery(query.PanelId, query.ServiceName, stepSeconds)
        };
    }

    internal static JsonObject BuildCompositeQuery(
        ObservabilityPanelId panelId,
        string serviceName,
        int stepIntervalSeconds)
    {
        return panelId switch
        {
            ObservabilityPanelId.RequestRate => SingleMetricQuery(
                stepIntervalSeconds,
                serviceName,
                metricName: "signoz_latency.count",
                timeAggregation: "rate",
                spaceAggregation: "sum",
                legend: "rps"),

            ObservabilityPanelId.P95Latency => SingleMetricQuery(
                stepIntervalSeconds,
                serviceName,
                metricName: "signoz_latency.bucket",
                timeAggregation: "p95",
                spaceAggregation: "p95",
                legend: "p95"),

            ObservabilityPanelId.ErrorRate5xx => SingleMetricQuery(
                stepIntervalSeconds,
                serviceName,
                metricName: "signoz_calls_total",
                timeAggregation: "rate",
                spaceAggregation: "sum",
                legend: "5xx/s",
                extraFilter: "http_status_code >= 500"),

            ObservabilityPanelId.ErrorPercentage => ErrorPercentageQuery(stepIntervalSeconds, serviceName),

            _ => SingleMetricQuery(
                stepIntervalSeconds,
                serviceName,
                metricName: "signoz_calls_total",
                timeAggregation: "rate",
                spaceAggregation: "sum",
                legend: panelId.ToString())
        };
    }

    private static JsonObject SingleMetricQuery(
        int stepIntervalSeconds,
        string serviceName,
        string metricName,
        string timeAggregation,
        string spaceAggregation,
        string legend,
        string? extraFilter = null)
    {
        var filter = BuildServiceFilter(serviceName, extraFilter);

        return new JsonObject
        {
            ["queryType"] = "builder",
            ["panelType"] = "graph",
            ["queries"] = new JsonArray
            {
                new JsonObject
                {
                    ["type"] = "builder_query",
                    ["spec"] = new JsonObject
                    {
                        ["name"] = "A",
                        ["signal"] = "metrics",
                        ["stepInterval"] = stepIntervalSeconds,
                        ["aggregations"] = new JsonArray
                        {
                            new JsonObject
                            {
                                ["metricName"] = metricName,
                                ["timeAggregation"] = timeAggregation,
                                ["spaceAggregation"] = spaceAggregation
                            }
                        },
                        ["filter"] = new JsonObject { ["expression"] = filter },
                        ["legend"] = legend
                    }
                }
            }
        };
    }

    private static JsonObject ErrorPercentageQuery(int stepIntervalSeconds, string serviceName)
    {
        var serviceFilter = BuildServiceFilter(serviceName, null);
        var errorFilter = BuildServiceFilter(serviceName, "status.code = 'STATUS_CODE_ERROR'");

        return new JsonObject
        {
            ["queryType"] = "builder",
            ["panelType"] = "graph",
            ["queries"] = new JsonArray
            {
                new JsonObject
                {
                    ["type"] = "builder_query",
                    ["spec"] = new JsonObject
                    {
                        ["name"] = "A",
                        ["signal"] = "metrics",
                        ["stepInterval"] = stepIntervalSeconds,
                        ["disabled"] = true,
                        ["aggregations"] = new JsonArray
                        {
                            new JsonObject
                            {
                                ["metricName"] = "signoz_calls_total",
                                ["timeAggregation"] = "rate",
                                ["spaceAggregation"] = "sum"
                            }
                        },
                        ["filter"] = new JsonObject { ["expression"] = errorFilter },
                        ["legend"] = "errors"
                    }
                },
                new JsonObject
                {
                    ["type"] = "builder_query",
                    ["spec"] = new JsonObject
                    {
                        ["name"] = "B",
                        ["signal"] = "metrics",
                        ["stepInterval"] = stepIntervalSeconds,
                        ["disabled"] = true,
                        ["aggregations"] = new JsonArray
                        {
                            new JsonObject
                            {
                                ["metricName"] = "signoz_calls_total",
                                ["timeAggregation"] = "rate",
                                ["spaceAggregation"] = "sum"
                            }
                        },
                        ["filter"] = new JsonObject { ["expression"] = serviceFilter },
                        ["legend"] = "total"
                    }
                }
            },
            ["formulas"] = new JsonArray
            {
                new JsonObject
                {
                    ["name"] = "F1",
                    ["expression"] = "A/B",
                    ["disabled"] = false,
                    ["legend"] = "error %"
                }
            }
        };
    }

    public static string BuildServiceFilter(string serviceName, string? extraFilter)
    {
        var escaped = serviceName.Replace("'", "\\'", StringComparison.Ordinal);
        var filter = $"service.name = '{escaped}'";
        if (!string.IsNullOrWhiteSpace(extraFilter))
        {
            filter += $" AND {extraFilter}";
        }

        return filter;
    }
}
