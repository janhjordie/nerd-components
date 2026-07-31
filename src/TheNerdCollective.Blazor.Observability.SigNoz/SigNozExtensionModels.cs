using System.Text.Json.Nodes;
using TheNerdCollective.Blazor.Observability;

namespace TheNerdCollective.Blazor.Observability.SigNoz;

/// <summary>Context passed to <see cref="ISigNozResponseParser"/> implementations.</summary>
public sealed record SigNozParseContext(
    ObservabilityPanelId PanelId,
    string QueryRangePath,
    string? SchemaVersion,
    int? HttpStatusCode);

/// <summary>Context passed to <see cref="ISigNozQueryMutator"/> implementations.</summary>
public sealed record SigNozQueryContext(
    string QueryRangePath,
    string? SchemaVersion,
    SigNozQueryOverrides? Overrides = null);

/// <summary>Per-request overrides for SigNoz query_range (filter, path, schema).</summary>
public sealed record SigNozQueryOverrides(
    string? FilterExpression = null,
    string? QueryRangePath = null,
    string? SchemaVersion = null);

/// <summary>Runtime capability profile discovered from live SigNoz.</summary>
public sealed record SigNozRuntimeProfile(
    string? SigNozVersion,
    string QueryRangePath,
    string? RecommendedSchemaVersion,
    DateTimeOffset DiscoveredAt);

/// <summary>SigNoz version endpoint payload.</summary>
public sealed record SigNozVersionInfo(
    string? Version,
    string? Edition,
    bool? SetupCompleted,
    int? HealthStatusCode,
    bool HealthOk);
