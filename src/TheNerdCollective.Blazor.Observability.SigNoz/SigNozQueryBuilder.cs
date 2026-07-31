using System.Text.Json.Nodes;
using TheNerdCollective.Blazor.Observability;

namespace TheNerdCollective.Blazor.Observability.SigNoz;

/// <summary>Builds SigNoz v5 <c>query_range</c> payloads for preset panels.</summary>
public static class SigNozQueryBuilder
{
    /// <summary>Builds a SigNoz <c>/api/v5/query_range</c> request body.</summary>
    /// <param name="schemaVersion">Optional schema version (<c>v1</c>, <c>v2</c>). Omit for forward-compatible payloads.</param>
    public static JsonObject BuildQueryRangeRequest(ObservabilityPanelQuery query, string? schemaVersion = null)
    {
        var stepSeconds = Math.Max(1, (int)query.TimeRange.Step.TotalSeconds);
        var body = new JsonObject
        {
            ["start"] = query.TimeRange.Start.ToUnixTimeMilliseconds(),
            ["end"] = query.TimeRange.End.ToUnixTimeMilliseconds(),
            ["requestType"] = "time_series",
            ["compositeQuery"] = BuildCompositeQuery(query.PanelId, query.ServiceName, stepSeconds)
        };

        ApplySchemaVersion(body, schemaVersion);
        return body;
    }

    /// <summary>Sets or removes <c>schemaVersion</c> on a query_range body.</summary>
    public static void ApplySchemaVersion(JsonObject body, string? schemaVersion)
    {
        body.Remove("schemaVersion");
        if (!string.IsNullOrWhiteSpace(schemaVersion))
        {
            body["schemaVersion"] = schemaVersion;
        }
    }

    internal static JsonObject BuildCompositeQuery(
        ObservabilityPanelId panelId,
        string serviceName,
        int stepIntervalSeconds)
    {
        return panelId switch
        {
            ObservabilityPanelId.RequestRate => ServiceMetricQuery(
                stepIntervalSeconds,
                serviceName,
                metricName: "signoz_latency.count",
                timeAggregation: "rate",
                spaceAggregation: "sum",
                legend: "rps"),

            ObservabilityPanelId.P95Latency => ServiceMetricQuery(
                stepIntervalSeconds,
                serviceName,
                metricName: "signoz_latency.bucket",
                timeAggregation: "p95",
                spaceAggregation: "p95",
                legend: "p95"),

            ObservabilityPanelId.ErrorRate5xx => ServiceMetricQuery(
                stepIntervalSeconds,
                serviceName,
                metricName: "signoz_calls_total",
                timeAggregation: "rate",
                spaceAggregation: "sum",
                legend: "5xx/s",
                extraFilter: "http_status_code >= 500"),

            ObservabilityPanelId.ErrorPercentage => ErrorPercentageQuery(stepIntervalSeconds, serviceName),

            ObservabilityPanelId.RuntimeGcHeap => ServiceMetricQuery(
                stepIntervalSeconds,
                serviceName,
                metricName: "process.runtime.dotnet.gc.heap.size",
                timeAggregation: "avg",
                spaceAggregation: "sum",
                legend: "gc heap"),

            ObservabilityPanelId.RuntimeProcessMemory => ServiceMetricQuery(
                stepIntervalSeconds,
                serviceName,
                metricName: "process.runtime.dotnet.gc.memory.committed",
                timeAggregation: "avg",
                spaceAggregation: "sum",
                legend: "memory"),

            ObservabilityPanelId.HostCpuUtilization => HostMetricQuery(
                stepIntervalSeconds,
                metricName: "system.cpu.utilization",
                timeAggregation: "avg",
                spaceAggregation: "sum",
                legend: "cpu",
                filter: "state != 'idle'"),

            ObservabilityPanelId.HostMemoryUtilization => HostMetricQuery(
                stepIntervalSeconds,
                metricName: "system.memory.utilization",
                timeAggregation: "avg",
                spaceAggregation: "avg",
                legend: "ram",
                filter: "state = 'used'"),

            ObservabilityPanelId.HostDiskUtilization => HostMetricQuery(
                stepIntervalSeconds,
                metricName: "system.filesystem.utilization",
                timeAggregation: "max",
                spaceAggregation: "max",
                legend: "disk",
                filter: "mountpoint = '/' AND state != 'free'"),

            ObservabilityPanelId.DbQueryRate => ServiceMetricQuery(
                stepIntervalSeconds,
                serviceName,
                metricName: "signoz_latency.count",
                timeAggregation: "rate",
                spaceAggregation: "sum",
                legend: "db/s",
                extraFilter: "db.system EXISTS"),

            ObservabilityPanelId.DbQueryP95 => ServiceMetricQuery(
                stepIntervalSeconds,
                serviceName,
                metricName: "signoz_latency.bucket",
                timeAggregation: "p95",
                spaceAggregation: "p95",
                legend: "db p95",
                extraFilter: "db.system EXISTS"),

            ObservabilityPanelId.HttpClientRate => ServiceMetricQuery(
                stepIntervalSeconds,
                serviceName,
                metricName: "signoz_latency.count",
                timeAggregation: "rate",
                spaceAggregation: "sum",
                legend: "http/s",
                extraFilter: "span_kind = 'SPAN_KIND_CLIENT'"),

            ObservabilityPanelId.HttpClientP95 => ServiceMetricQuery(
                stepIntervalSeconds,
                serviceName,
                metricName: "signoz_latency.bucket",
                timeAggregation: "p95",
                spaceAggregation: "p95",
                legend: "http p95",
                extraFilter: "span_kind = 'SPAN_KIND_CLIENT'"),

            _ => ServiceMetricQuery(
                stepIntervalSeconds,
                serviceName,
                metricName: "signoz_calls_total",
                timeAggregation: "rate",
                spaceAggregation: "sum",
                legend: panelId.ToString())
        };
    }

    private static JsonObject ServiceMetricQuery(
        int stepIntervalSeconds,
        string serviceName,
        string metricName,
        string timeAggregation,
        string spaceAggregation,
        string legend,
        string? extraFilter = null)
    {
        var filter = BuildServiceFilter(serviceName, extraFilter);
        return MetricQuery(stepIntervalSeconds, metricName, timeAggregation, spaceAggregation, legend, filter);
    }

    private static JsonObject HostMetricQuery(
        int stepIntervalSeconds,
        string metricName,
        string timeAggregation,
        string spaceAggregation,
        string legend,
        string filter)
        => MetricQuery(stepIntervalSeconds, metricName, timeAggregation, spaceAggregation, legend, filter);

    private static JsonObject MetricQuery(
        int stepIntervalSeconds,
        string metricName,
        string timeAggregation,
        string spaceAggregation,
        string legend,
        string filterExpression)
    {
        return new JsonObject
        {
            ["queries"] = new JsonArray
            {
                BuildBuilderQuery(stepIntervalSeconds, metricName, timeAggregation, spaceAggregation, legend, filterExpression)
            }
        };
    }

    private static JsonObject BuildBuilderQuery(
        int stepIntervalSeconds,
        string metricName,
        string timeAggregation,
        string spaceAggregation,
        string legend,
        string filterExpression,
        string name = "A",
        bool disabled = false)
    {
        var spec = new JsonObject
        {
            ["name"] = name,
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
            ["filter"] = new JsonObject { ["expression"] = filterExpression },
            ["legend"] = legend
        };

        if (disabled)
        {
            spec["disabled"] = true;
        }

        return new JsonObject
        {
            ["type"] = "builder_query",
            ["spec"] = spec
        };
    }

    private static JsonObject ErrorPercentageQuery(int stepIntervalSeconds, string serviceName)
    {
        var serviceFilter = BuildServiceFilter(serviceName, null);
        var errorFilter = BuildServiceFilter(serviceName, "status.code = 'STATUS_CODE_ERROR'");

        return new JsonObject
        {
            ["queries"] = new JsonArray
            {
                BuildBuilderQuery(
                    stepIntervalSeconds,
                    "signoz_calls_total",
                    "rate",
                    "sum",
                    "errors",
                    errorFilter,
                    name: "A",
                    disabled: true),
                BuildBuilderQuery(
                    stepIntervalSeconds,
                    "signoz_calls_total",
                    "rate",
                    "sum",
                    "total",
                    serviceFilter,
                    name: "B",
                    disabled: true)
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
