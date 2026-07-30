# TheNerdCollective.MudComponents.ObservabilityDashboard

MudBlazor **9.7** observability dashboard components for Blazor Server ops overviews.

Requires [`TheNerdCollective.Blazor.Observability`](../TheNerdCollective.Blazor.Observability/README.md).

## Installation

```bash
dotnet add package TheNerdCollective.MudComponents.ObservabilityDashboard
dotnet add package TheNerdCollective.Blazor.Observability
```

## Quick start

```csharp
// Program.cs
builder.Services.AddObservabilityDashboard(o =>
{
    o.DefaultServiceName = "my-app";
    o.SigNoz.BaseUrl = "http://127.0.0.1:8080";
    o.ExternalDashboardBaseUrl = "https://devops.example.com";
});
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
