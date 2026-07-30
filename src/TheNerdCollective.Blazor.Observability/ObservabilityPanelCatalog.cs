namespace TheNerdCollective.Blazor.Observability;

/// <summary>Metadata for preset observability panels.</summary>
public static class ObservabilityPanelCatalog
{
    private static readonly IReadOnlyDictionary<ObservabilityPanelId, ObservabilityPanelDefinition> Definitions =
        new Dictionary<ObservabilityPanelId, ObservabilityPanelDefinition>
        {
            [ObservabilityPanelId.RequestRate] = new("Request rate", "reqps", "rps"),
            [ObservabilityPanelId.P95Latency] = new("P95 latency", "ms", "p95"),
            [ObservabilityPanelId.ErrorRate5xx] = new("5xx rate", "reqps", "5xx/s"),
            [ObservabilityPanelId.ErrorPercentage] = new("Error percentage", "percentunit", "error %"),
            [ObservabilityPanelId.ActiveCircuits] = new("Active circuits", "short", "circuits"),
            [ObservabilityPanelId.RuntimeGcHeap] = new("GC heap", "bytes", "gc heap")
        };

    /// <summary>Gets metadata for a panel id.</summary>
    public static ObservabilityPanelDefinition GetDefinition(ObservabilityPanelId panelId) =>
        Definitions.TryGetValue(panelId, out var definition)
            ? definition
            : new ObservabilityPanelDefinition(panelId.ToString(), string.Empty, panelId.ToString());

    /// <summary>Panels included in the default overview snapshot.</summary>
    public static IReadOnlyList<ObservabilityPanelId> OverviewPanels { get; } =
    [
        ObservabilityPanelId.RequestRate,
        ObservabilityPanelId.P95Latency,
        ObservabilityPanelId.ErrorRate5xx,
        ObservabilityPanelId.ErrorPercentage
    ];
}

/// <summary>Display metadata for a dashboard panel.</summary>
public sealed record ObservabilityPanelDefinition(string Title, string Unit, string Legend);
