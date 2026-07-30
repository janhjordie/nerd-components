# TheNerdCollective.MudComponents.ObservabilityDashboard

MudBlazor **9.7** observability dashboard components for Blazor Server ops overviews.

Requires [`TheNerdCollective.Blazor.Observability`](../TheNerdCollective.Blazor.Observability/README.md) and a query adapter such as [`TheNerdCollective.Blazor.Observability.SigNoz`](../TheNerdCollective.Blazor.Observability.SigNoz/README.md).

## Installation

```bash
dotnet add package TheNerdCollective.MudComponents.ObservabilityDashboard
dotnet add package TheNerdCollective.Blazor.Observability
dotnet add package TheNerdCollective.Blazor.Observability.SigNoz
```

## Quick start

```csharp
// Program.cs
builder.Services.AddObservabilityDashboardWithSigNoz(builder.Configuration);
```

```razor
@using TheNerdCollective.MudComponents.ObservabilityDashboard

<ObservabilityDashboard ServiceName="my-app"
                        TimeRangeMinutes="15"
                        ShowExternalLink="true"
                        ExternalLinkText="Open in SigNoz" />
```

## Components

| Component | Purpose |
|-----------|---------|
| `ObservabilityDashboard` | Full ops page — 4 metric cards, 2 charts, refresh |
| `ObservabilityOverviewCards` | Scalar metric grid |
| `ObservabilityMetricCard` | Single metric tile |
| `ObservabilityTimeSeriesChart` | MudChart line wrapper |
| `ObservabilityHealthBadge` | Healthy / degraded / unhealthy chip |

All metric cards and charts expose `data-testid` hooks for Playwright.

## Related

- [Blazor.Observability core](../TheNerdCollective.Blazor.Observability/README.md)
- [Implementation plan](../../docs/00-backlogs/00-observability-dashboard-plan.md)
