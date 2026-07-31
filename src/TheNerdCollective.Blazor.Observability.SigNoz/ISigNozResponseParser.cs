using TheNerdCollective.Blazor.Observability;

namespace TheNerdCollective.Blazor.Observability.SigNoz;

/// <summary>
/// Pluggable SigNoz response parser. Return <c>null</c> when this parser cannot handle the payload.
/// Register custom parsers before built-in defaults to take precedence.
/// </summary>
public interface ISigNozResponseParser
{
    /// <summary>Attempts to parse a query_range JSON payload into a time series.</summary>
    ObservabilityTimeSeriesResult? TryParseTimeSeries(string json, SigNozParseContext context);

    /// <summary>Attempts to parse a services list JSON payload.</summary>
    IReadOnlyList<ObservabilityServiceInfo>? TryParseServices(string json, SigNozParseContext context);
}
