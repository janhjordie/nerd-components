using System.Text.Json.Nodes;
using TheNerdCollective.Blazor.Observability;

namespace TheNerdCollective.Blazor.Observability.SigNoz;

/// <summary>Mutates a SigNoz query_range body after the default builder runs.</summary>
public interface ISigNozQueryMutator
{
    void MutateQueryRangeBody(
        JsonObject body,
        ObservabilityPanelQuery query,
        SigNozQueryContext context);
}
