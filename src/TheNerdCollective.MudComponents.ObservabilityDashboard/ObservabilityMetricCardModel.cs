namespace TheNerdCollective.MudComponents.ObservabilityDashboard;

using MudBlazor;
using TheNerdCollective.Blazor.Observability;

/// <summary>Display model for a metric card row.</summary>
public sealed record ObservabilityMetricCardModel(
    string Title,
    ObservabilityScalarResult? Value,
    Color Color,
    string Icon,
    string DataTestId,
    string? Subtitle = null);
