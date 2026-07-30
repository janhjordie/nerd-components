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
            [ObservabilityPanelId.RuntimeGcHeap] = new("GC heap", "bytes", "gc heap"),
            [ObservabilityPanelId.RuntimeProcessMemory] = new("Process memory", "bytes", "memory"),
            [ObservabilityPanelId.HostCpuUtilization] = new("CPU", "percentunit", "cpu"),
            [ObservabilityPanelId.HostMemoryUtilization] = new("RAM", "percentunit", "ram"),
            [ObservabilityPanelId.HostDiskUtilization] = new("Disk", "percentunit", "disk"),
            [ObservabilityPanelId.DbQueryRate] = new("DB queries", "reqps", "db/s"),
            [ObservabilityPanelId.DbQueryP95] = new("DB P95", "ms", "db p95"),
            [ObservabilityPanelId.HttpClientRate] = new("HTTP outbound", "reqps", "http/s"),
            [ObservabilityPanelId.HttpClientP95] = new("HTTP P95", "ms", "http p95")
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
