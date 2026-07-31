using TheNerdCollective.Blazor.Observability;

namespace TheNerdCollective.Blazor.Observability.SigNoz;

/// <summary>Parses SigNoz query API JSON into dashboard DTOs.</summary>
public static class SigNozResponseParser
{
    /// <summary>Parses a <c>query_range</c> response into a time series result.</summary>
    public static ObservabilityTimeSeriesResult ParseTimeSeries(
        string json,
        ObservabilityPanelId panelId)
    {
        var primary = SigNozResponseParserInternals.ParseTimeSeriesCore(json, panelId, deepWalk: false);
        if (primary.Points.Count > 0)
        {
            return primary;
        }

        return SigNozResponseParserInternals.ParseTimeSeriesCore(json, panelId, deepWalk: true);
    }

    /// <summary>Parses a services list response.</summary>
    public static IReadOnlyList<ObservabilityServiceInfo> ParseServices(string json) =>
        SigNozResponseParserInternals.ParseServicesCore(json);
}
