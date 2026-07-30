# TheNerdCollective.Blazor.Observability

Backend-agnostic observability dashboard services for **Blazor Server** apps instrumented with **OpenTelemetry**.

Pairs with [`TheNerdCollective.MudComponents.ObservabilityDashboard`](../TheNerdCollective.MudComponents.ObservabilityDashboard/README.md) for MudBlazor 9.7 UI components.

## Features

- **SigNoz adapter** — preset panels for request rate, P95 latency, 5xx rate, error %
- **`IObservabilityBackend`** — swap backends without changing UI
- **Minimal API** — `/api/observability/*` for dashboard data
- **Server-side only** — API tokens never sent to the browser

## Installation

```bash
dotnet add package TheNerdCollective.Blazor.Observability
```

## Quick start

```csharp
using TheNerdCollective.Blazor.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddObservabilityDashboard(o =>
{
    o.DefaultServiceName = "my-app";
    o.ExternalDashboardBaseUrl = "https://devops.example.com";
    o.SigNoz.BaseUrl = "http://127.0.0.1:8080";
    o.SigNoz.ApiToken = builder.Configuration["NerdObservability:SigNoz:ApiToken"];
});

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

Aligned with SigNoz span metrics (`signoz_latency.count`, `signoz_calls_total`, etc.) — same semantics as Nerd Consent `consent-host-overview.json`.

## Security

- Keep SigNoz API tokens in server configuration/secrets only
- Protect minimal API routes with your admin authorization policy
- This package queries aggregates — no trace payloads or PII

## Related

- [Observability Dashboard plan](../../docs/00-backlogs/00-observability-dashboard-plan.md)
- [SessionMonitor](../TheNerdCollective.Blazor.SessionMonitor/README.md) — complementary Blazor circuit monitoring
