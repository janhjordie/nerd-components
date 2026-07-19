namespace TheNerdCollective.MudComponents.DesignTokens;

public sealed record NerdBrandHealthMetric(string Name, int Score, string Detail);

public sealed record NerdBrandHealthResult(
    int Score,
    IReadOnlyList<NerdBrandHealthMetric> Metrics);
