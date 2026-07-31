using TheNerdCollective.Blazor.Observability;

namespace TheNerdCollective.Blazor.Observability.SigNoz;

/// <summary>Built-in v5 + legacy SigNoz query_range response parser.</summary>
public sealed class BuiltInSigNozResponseParser : ISigNozResponseParser
{
    public ObservabilityTimeSeriesResult? TryParseTimeSeries(string json, SigNozParseContext context)
    {
        var result = SigNozResponseParserInternals.ParseTimeSeriesCore(json, context.PanelId, deepWalk: false);
        return result.Points.Count > 0 ? result : null;
    }

    public IReadOnlyList<ObservabilityServiceInfo>? TryParseServices(string json, SigNozParseContext context) =>
        SigNozResponseParserInternals.ParseServicesCore(json);
}

/// <summary>Deep JSON walk fallback when v5 aggregation shape differs.</summary>
public sealed class DeepWalkSigNozResponseParser : ISigNozResponseParser
{
    public ObservabilityTimeSeriesResult? TryParseTimeSeries(string json, SigNozParseContext context)
    {
        var result = SigNozResponseParserInternals.ParseTimeSeriesCore(json, context.PanelId, deepWalk: true);
        return result.Points.Count > 0 ? result : null;
    }

    public IReadOnlyList<ObservabilityServiceInfo>? TryParseServices(string json, SigNozParseContext context) =>
        null;
}
