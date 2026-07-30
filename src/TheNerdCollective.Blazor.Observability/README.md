# TheNerdCollective.Blazor.Observability

Backend-agnostic observability dashboard services for **Blazor Server** apps instrumented with **OpenTelemetry**.

Pairs with:

- [`TheNerdCollective.Blazor.Observability.SigNoz`](../TheNerdCollective.Blazor.Observability.SigNoz/README.md) — SigNoz adapter (v1)
- [`TheNerdCollective.MudComponents.ObservabilityDashboard`](../TheNerdCollective.MudComponents.ObservabilityDashboard/README.md) — MudBlazor 9.7 UI

## Features

- **`IObservabilityBackend`** — swap query backends without changing UI
- **`IObservabilityDashboardService`** — overview snapshots and preset panels
- **Minimal API** — `/api/observability/*` for dashboard data
- **Server-side only** — API tokens never sent to the browser

## Installation

```bash
dotnet add package TheNerdCollective.Blazor.Observability
dotnet add package TheNerdCollective.Blazor.Observability.SigNoz   # SigNoz backend
dotnet add package TheNerdCollective.MudComponents.ObservabilityDashboard   # optional UI
```

## Quick start

```csharp
using TheNerdCollective.Blazor.Observability;
using TheNerdCollective.Blazor.Observability.SigNoz;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddObservabilityDashboardWithSigNoz(builder.Configuration);

var app = builder.Build();

app.MapObservabilityDashboardEndpoints()
   .RequireAuthorization(); // host must restrict ops APIs

app.Run();
```

## Configuration

```json
{
  "NerdObservability": {
    "DefaultServiceName": "my-app",
    "DefaultLookbackMinutes": 15,
    "ExternalDashboardBaseUrl": "https://devops.example.com",
    "SigNoz": {
      "BaseUrl": "http://127.0.0.1:8080",
      "ApiToken": "your-editor-api-key"
    }
  }
}
```

## API routes

| Route | Description |
|-------|-------------|
| `GET /api/observability/overview?service=` | Scalar overview snapshot |
| `GET /api/observability/panel/{panelId}?service=&minutes=` | Time series for preset panel |
| `GET /api/observability/services?minutes=` | Service list |
| `GET /api/observability/health?service=&minutes=` | Coarse health summary |

## Preset panels

Panel IDs are backend-neutral (`ObservabilityPanelId`). Each adapter maps them to backend-specific queries — SigNoz uses span metrics aligned with Nerd Consent `consent-host-overview.json`.

## Backend extensibility (SigNoz vs Grafana)

| Layer | Package | Role |
|-------|---------|------|
| Instrumentation | Host app (`OpenTelemetry.*`) | Export traces/metrics via OTLP |
| Query adapter | `Blazor.Observability.SigNoz`, future `*.Grafana` | Implement `IObservabilityBackend` |
| Dashboard | `Blazor.Observability` + MudComponents | Backend-agnostic UI |

**OpenTelemetry does not automatically connect this dashboard to Grafana.** OTel exports data; each backend adapter queries it back via that backend's HTTP API.

Register adapters explicitly:

```csharp
builder.Services.AddObservabilityDashboard(configuration);
builder.Services.AddSigNozObservabilityBackend(configuration);
// future: builder.Services.AddGrafanaObservabilityBackend(configuration);
```

## Security

- Keep backend API tokens in server configuration/secrets only
- Protect minimal API routes with your admin authorization policy
- This package queries aggregates — no trace payloads or PII

## Related

- [Observability Dashboard plan](../../docs/00-backlogs/00-observability-dashboard-plan.md)
- [SessionMonitor](../TheNerdCollective.Blazor.SessionMonitor/README.md) — complementary Blazor circuit monitoring
