# TheNerdCollective.Blazor.Observability.SigNoz

SigNoz query adapter for [`TheNerdCollective.Blazor.Observability`](../TheNerdCollective.Blazor.Observability/README.md).

Implements `IObservabilityBackend` against SigNoz v4 query APIs. Future backends (Grafana, Prometheus) follow the same adapter pattern in separate packages.

## Installation

```bash
dotnet add package TheNerdCollective.Blazor.Observability
dotnet add package TheNerdCollective.Blazor.Observability.SigNoz
```

## Registration

```csharp
using TheNerdCollective.Blazor.Observability;
using TheNerdCollective.Blazor.Observability.SigNoz;

builder.Services.AddObservabilityDashboard(builder.Configuration);
builder.Services.AddSigNozObservabilityBackend(builder.Configuration);

// Or convenience one-liner:
builder.Services.AddObservabilityDashboardWithSigNoz(builder.Configuration);
```

## Configuration

```json
{
  "NerdObservability": {
    "DefaultServiceName": "my-app",
    "ExternalDashboardBaseUrl": "https://devops.example.com",
    "SigNoz": {
      "BaseUrl": "http://127.0.0.1:8080",
      "ApiToken": "your-editor-api-key"
    }
  }
}
```

## Related

- [Core observability package](../TheNerdCollective.Blazor.Observability/README.md)
- [MudBlazor dashboard UI](../TheNerdCollective.MudComponents.ObservabilityDashboard/README.md)
