using TheNerdCollective.Blazor.Observability;

namespace TheNerdCollective.Blazor.Observability.SigNoz;

/// <summary>Chains registered <see cref="ISigNozResponseParser"/> implementations.</summary>
public sealed class SigNozResponseParserCoordinator(IEnumerable<ISigNozResponseParser> parsers)
{
  private readonly IReadOnlyList<ISigNozResponseParser> _parsers = parsers.ToList();

  public ObservabilityTimeSeriesResult ParseTimeSeries(string json, SigNozParseContext context)
  {
    foreach (var parser in _parsers)
    {
      var result = parser.TryParseTimeSeries(json, context);
      if (result is not null && result.Points.Count > 0)
      {
        return result;
      }
    }

    var definition = ObservabilityPanelCatalog.GetDefinition(context.PanelId);
    return new ObservabilityTimeSeriesResult(definition.Legend, definition.Unit, []);
  }

  public IReadOnlyList<ObservabilityServiceInfo> ParseServices(string json, SigNozParseContext context)
  {
    foreach (var parser in _parsers)
    {
      var result = parser.TryParseServices(json, context);
      if (result is not null && result.Count > 0)
      {
        return result;
      }
    }

    foreach (var parser in _parsers)
    {
      var result = parser.TryParseServices(json, context);
      if (result is not null)
      {
        return result;
      }
    }

    return [];
  }
}
