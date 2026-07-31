using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using TheNerdCollective.Blazor.Observability;

namespace TheNerdCollective.Blazor.Observability.SigNoz;

/// <summary>Parses SigNoz query API JSON into dashboard DTOs.</summary>
public static class SigNozResponseParser
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>Parses a <c>query_range</c> response into a time series result.</summary>
    public static ObservabilityTimeSeriesResult ParseTimeSeries(
        string json,
        ObservabilityPanelId panelId)
    {
        var definition = ObservabilityPanelCatalog.GetDefinition(panelId);
        var root = JsonNode.Parse(json) as JsonObject;
        var points = ExtractPoints(root);
        if (points.Count == 0)
        {
            points = ExtractPointsDeep(root);
        }

        return new ObservabilityTimeSeriesResult(definition.Legend, definition.Unit, points);
    }

    /// <summary>Parses a services list response.</summary>
    public static IReadOnlyList<ObservabilityServiceInfo> ParseServices(string json)
    {
        var root = JsonNode.Parse(json) as JsonObject;
        if (root is null)
        {
            return [];
        }

        var data = root["data"] as JsonArray ?? root["services"] as JsonArray;
        if (data is null)
        {
            return [];
        }

        var services = new List<ObservabilityServiceInfo>();
        foreach (var item in data)
        {
            if (item is not JsonObject serviceObject)
            {
                continue;
            }

            var name = ReadString(serviceObject, "serviceName", "name", "service_name");
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            services.Add(new ObservabilityServiceInfo(
                name,
                ReadString(serviceObject, "deploymentEnvironment", "environment"),
                ReadDouble(serviceObject, "callRate", "requestRate"),
                ReadDouble(serviceObject, "p99", "p95", "p95Latency"),
                ReadDouble(serviceObject, "errorRate")));
        }

        return services;
    }

    private static List<ObservabilityTimeSeriesPoint> ExtractPoints(JsonObject? root)
    {
        var points = new List<ObservabilityTimeSeriesPoint>();
        if (root is null)
        {
            return points;
        }

        var v5Results = root["data"]?["results"] as JsonArray;
        if (v5Results is not null)
        {
            foreach (var result in v5Results)
            {
                if (result is not JsonObject resultObject)
                {
                    continue;
                }

                AppendV5Aggregations(resultObject["aggregations"], points);
            }
        }
        else
        {
            var resultNode = root["data"]?["result"] ?? root["result"];
            if (resultNode is JsonArray results)
            {
                foreach (var result in results)
                {
                    if (result is not JsonObject resultObject)
                    {
                        continue;
                    }

                    AppendSeriesPoints(resultObject["series"], points);
                    AppendSeriesPoints(resultObject["values"], points);
                    AppendValuesArray(resultObject["values"], points);
                }
            }
        }

        points.Sort(static (a, b) => a.Timestamp.CompareTo(b.Timestamp));
        return points;
    }

    /// <summary>Deep fallback when v5 shape differs from expected (mirrors probe jq walk).</summary>
    private static List<ObservabilityTimeSeriesPoint> ExtractPointsDeep(JsonObject? root)
    {
        var points = new List<ObservabilityTimeSeriesPoint>();
        if (root is null)
        {
            return points;
        }

        WalkNode(root, points);
        points.Sort(static (a, b) => a.Timestamp.CompareTo(b.Timestamp));
        return points;
    }

    private static void WalkNode(JsonNode? node, List<ObservabilityTimeSeriesPoint> points)
    {
        switch (node)
        {
            case JsonObject obj:
                if (obj.ContainsKey("values"))
                {
                    AppendValuesArray(obj["values"], points);
                }

                foreach (var child in obj)
                {
                    WalkNode(child.Value, points);
                }

                break;
            case JsonArray array:
                foreach (var item in array)
                {
                    WalkNode(item, points);
                }

                break;
        }
    }

    private static void AppendV5Aggregations(JsonNode? aggregationsNode, List<ObservabilityTimeSeriesPoint> points)
    {
        if (aggregationsNode is not JsonArray aggregations)
        {
            return;
        }

        foreach (var aggregation in aggregations)
        {
            if (aggregation is not JsonObject aggregationObject)
            {
                continue;
            }

            AppendSeriesPoints(aggregationObject["series"], points);
        }
    }

    private static void AppendSeriesPoints(JsonNode? seriesNode, List<ObservabilityTimeSeriesPoint> points)
    {
        if (seriesNode is not JsonArray seriesArray)
        {
            return;
        }

        foreach (var series in seriesArray)
        {
            if (series is not JsonObject seriesObject)
            {
                continue;
            }

            AppendValuesArray(seriesObject["values"], points);
        }
    }

    private static void AppendValuesArray(JsonNode? valuesNode, List<ObservabilityTimeSeriesPoint> points)
    {
        if (valuesNode is not JsonArray valuesArray)
        {
            return;
        }

        foreach (var valueEntry in valuesArray)
        {
            if (valueEntry is JsonArray tuple && tuple.Count >= 2)
            {
                if (TryReadPoint(tuple[0], tuple[1], out var point))
                {
                    points.Add(point);
                }

                continue;
            }

            if (valueEntry is JsonObject valueObject
                && TryReadPoint(valueObject["timestamp"] ?? valueObject["t"], valueObject["value"] ?? valueObject["v"], out var objectPoint))
            {
                points.Add(objectPoint);
            }
        }
    }

    private static bool TryReadPoint(JsonNode? timestampNode, JsonNode? valueNode, out ObservabilityTimeSeriesPoint point)
    {
        point = default!;
        if (timestampNode is null || valueNode is null)
        {
            return false;
        }

        if (!TryReadTimestamp(timestampNode, out var timestamp))
        {
            return false;
        }

        if (!TryReadDouble(valueNode, out var value))
        {
            return false;
        }

        point = new ObservabilityTimeSeriesPoint(timestamp, value);
        return true;
    }

    private static bool TryReadTimestamp(JsonNode node, out DateTimeOffset timestamp)
    {
        timestamp = default;
        if (node is JsonValue value && value.TryGetValue<long>(out var raw))
        {
            timestamp = raw > 999_999_999_999
                ? DateTimeOffset.FromUnixTimeMilliseconds(raw)
                : DateTimeOffset.FromUnixTimeSeconds(raw);
            return true;
        }

        if (node is JsonValue stringValue
            && stringValue.TryGetValue<string>(out var text)
            && DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out timestamp))
        {
            return true;
        }

        return false;
    }

    private static bool TryReadDouble(JsonNode node, out double value)
    {
        value = 0;
        if (node is JsonValue jsonValue)
        {
            if (jsonValue.TryGetValue<double>(out value))
            {
                return true;
            }

            if (jsonValue.TryGetValue<string>(out var text)
                && double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }
        }

        return false;
    }

    private static string? ReadString(JsonObject obj, params string[] names)
    {
        foreach (var name in names)
        {
            if (obj[name] is JsonValue value && value.TryGetValue<string>(out var text) && !string.IsNullOrWhiteSpace(text))
            {
                return text;
            }
        }

        return null;
    }

    private static double? ReadDouble(JsonObject obj, params string[] names)
    {
        foreach (var name in names)
        {
            if (obj[name] is JsonNode node && TryReadDouble(node, out var value))
            {
                return value;
            }
        }

        return null;
    }
}
