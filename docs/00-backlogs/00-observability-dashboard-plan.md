---
title: "Observability Dashboard — NuGet packages (Blazor + MudBlazor)"
status: Draft
author: "@janhjordie"
last_updated: "30-07-2026"
id_prefix: "HR"
epic: "HR-176–HR-190"
pattern: "SessionMonitor two-package model"
first_consumer: "Nerd Consent — /admin/ops"
---

# Observability Dashboard — implementation plan

Public NuGet packages in **TheNerdCollective.Components** that provide a **simple ops dashboard** for Blazor Server apps instrumented with **OpenTelemetry**. Data is read from a backend (SigNoz first) or live in-process metrics — **not** by re-implementing SigNoz UI.

**Reference pattern:** `TheNerdCollective.Blazor.SessionMonitor` + `TheNerdCollective.MudComponents.SessionMonitor`.

**Out of scope:** OTel instrumentation (host uses `OpenTelemetry.*` NuGet), full trace explorer, log search UI, alert management UI.

---

## 1. Goals

| Goal | Metric |
|------|--------|
| Dogfood in Nerd Consent | `/admin/ops` shows request rate, P95, 5xx, error % for `nerd-consent-host` |
| Reusable public packages | Published to nuget.org (Apache-2.0) |
| Backend-agnostic core | SigNoz v1 adapter; Prometheus/Grafana adapter later |
| MudBlazor-native UI | Cards, charts, tables — same visual language as SessionMonitor |
| Secure by default | API keys server-side only; dashboard behind host `[Authorize]` |

---

## 2. Package split

| Package | SDK | Version (initial) | Role |
|---------|-----|-------------------|------|
| `TheNerdCollective.Blazor.Observability` | `Microsoft.NET.Sdk` | `1.0.0` | Backends, DTOs, DI, optional minimal API |
| `TheNerdCollective.MudComponents.ObservabilityDashboard` | `Microsoft.NET.Sdk.Razor` | `1.0.0` | MudBlazor widgets + composed dashboard |

**Dependencies**

| Package | References |
|---------|------------|
| `Blazor.Observability` | `FrameworkReference Microsoft.AspNetCore.App`, `Microsoft.Extensions.Http` |
| `MudComponents.ObservabilityDashboard` | `MudBlazor 9.7.0`, project ref → `Blazor.Observability` |

**Optional v1.1:** `SigNozObservabilityBackend` in separate package `TheNerdCollective.Blazor.Observability.SigNoz` if we want zero SigNoz coupling in core — **recommendation:** keep SigNoz adapter in core v1 for simplicity; extract when a second backend lands.

---

## 3. Architecture

```
Host (Consent, Token Studio, …)
  ├─ OpenTelemetry.*  ──OTLP──►  SigNoz / other backend
  └─ builder.Services.AddObservabilityDashboard(o => …)
           │
           ▼
  IObservabilityBackend ──► SigNozObservabilityBackend (HTTP, server-side token)
           │
           ▼
  IObservabilityDashboardService ──► MudComponents (MetricCard, TimeSeriesChart, …)
```

### 3.1 Core abstractions

```csharp
namespace TheNerdCollective.Blazor.Observability;

/// <summary>Pluggable telemetry query backend (SigNoz, Prometheus, in-process).</summary>
public interface IObservabilityBackend
{
    string BackendId { get; }  // e.g. "signoz", "in-process"

    Task<IReadOnlyList<ObservabilityServiceInfo>> ListServicesAsync(
        ObservabilityQueryContext context,
        CancellationToken cancellationToken = default);

    Task<ObservabilityTimeSeriesResult> QueryTimeSeriesAsync(
        ObservabilityPanelQuery query,
        CancellationToken cancellationToken = default);

    Task<ObservabilityScalarResult> QueryScalarAsync(
        ObservabilityPanelQuery query,
        CancellationToken cancellationToken = default);

    Task<ObservabilityHealthSummary> GetHealthSummaryAsync(
        string serviceName,
        ObservabilityQueryContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>Facade used by Blazor components — caches, refresh, panel presets.</summary>
public interface IObservabilityDashboardService
{
    ObservabilityDashboardOptions Options { get; }

    Task RefreshAsync(CancellationToken cancellationToken = default);

    Task<ObservabilityOverviewSnapshot> GetOverviewAsync(
        string? serviceName = null,
        CancellationToken cancellationToken = default);

    Task<ObservabilityTimeSeriesResult> GetPanelAsync(
        ObservabilityPanelId panelId,
        string serviceName,
        ObservabilityTimeRange timeRange,
        CancellationToken cancellationToken = default);

    Uri? GetExternalDashboardUrl(string? serviceName = null);
}
```

### 3.2 DTOs

```csharp
public sealed record ObservabilityQueryContext(
    DateTimeOffset Start,
    DateTimeOffset End,
    string? ServiceName = null);

public sealed record ObservabilityTimeRange(
    DateTimeOffset Start,
    DateTimeOffset End,
    TimeSpan Step = TimeSpan.FromMinutes(1));

public enum ObservabilityPanelId
{
    RequestRate,
    P95Latency,
    ErrorRate5xx,
    ErrorPercentage,
    ActiveCircuits,      // in-process / optional SessionMonitor bridge
    RuntimeGcHeap
}

public sealed record ObservabilityTimeSeriesPoint(DateTimeOffset Timestamp, double Value);

public sealed record ObservabilityTimeSeriesResult(
    string Legend,
    string Unit,
    IReadOnlyList<ObservabilityTimeSeriesPoint> Points);

public sealed record ObservabilityScalarResult(double Value, string Unit, string Label);

public sealed record ObservabilityOverviewSnapshot(
    string ServiceName,
    ObservabilityScalarResult? RequestRate,
    ObservabilityScalarResult? P95LatencyMs,
    ObservabilityScalarResult? ErrorRate5xx,
    ObservabilityScalarResult? ErrorPercentage,
    ObservabilityHealthStatus Health,
    DateTimeOffset QueriedAtUtc);

public enum ObservabilityHealthStatus { Unknown, Healthy, Degraded, Unhealthy }

public sealed record ObservabilityServiceInfo(
    string Name,
    string? Environment,
    double? RequestRate,
    double? P95LatencyMs,
    double? ErrorRate);
```

### 3.3 Options & registration

```csharp
public sealed class ObservabilityDashboardOptions
{
    public const string SectionName = "NerdObservability";

    public ObservabilityBackendKind Backend { get; set; } = ObservabilityBackendKind.SigNoz;

    /// <summary>Default service.name filter (OTel resource attribute).</summary>
    public string DefaultServiceName { get; set; } = "app";

    public ObservabilityTimeRange DefaultTimeRange { get; set; } =
        ObservabilityTimeRange.LastMinutes(15);

    public SigNozBackendOptions SigNoz { get; set; } = new();

    public InProcessBackendOptions InProcess { get; set; } = new();

    /// <summary>Link opened by ExternalDashboardLink (full SigNoz/Grafana UI).</summary>
    public string? ExternalDashboardBaseUrl { get; set; }

    public bool EnableMinimalApi { get; set; } = true;
}

public sealed class SigNozBackendOptions
{
    public string BaseUrl { get; set; } = "http://127.0.0.1:8080";
    public string? ApiToken { get; set; }
    public string? OrgId { get; set; }
    public TimeSpan HttpTimeout { get; set; } = TimeSpan.FromSeconds(15);
}

public static class ObservabilityDashboardServiceCollectionExtensions
{
    public static IServiceCollection AddObservabilityDashboard(
        this IServiceCollection services,
        IConfiguration configuration)
        => services.AddObservabilityDashboard(
            configuration.GetSection(ObservabilityDashboardOptions.SectionName));

    public static IServiceCollection AddObservabilityDashboard(
        this IServiceCollection services,
        Action<ObservabilityDashboardOptions> configure);

    public static IServiceCollection AddObservabilityDashboard(
        this IServiceCollection services,
        IConfigurationSection section);
}

public static class ObservabilityDashboardEndpointExtensions
{
    public static IEndpointRouteBuilder MapObservabilityDashboardEndpoints(
        this IEndpointRouteBuilder endpoints,
        string pattern = "/api/observability");
}
```

**Minimal API routes (mirror SessionMonitor)**

| Method | Route | Returns |
|--------|-------|---------|
| GET | `/api/observability/overview?service=` | `ObservabilityOverviewSnapshot` |
| GET | `/api/observability/panel/{panelId}?service=&minutes=15` | `ObservabilityTimeSeriesResult` |
| GET | `/api/observability/services` | `ObservabilityServiceInfo[]` |
| GET | `/api/observability/health?service=` | `ObservabilityHealthSummary` |

All routes should require authorization when host maps them (document `[Authorize]` policy in README — package does not enforce auth by default).

### 3.4 SigNoz adapter (v1)

`SigNozObservabilityBackend` calls SigNoz **Query API v4**:

- `POST {BaseUrl}/api/v4/query_range` — time series (metrics from span-derived series)
- `POST {BaseUrl}/api/v1/services` — service list (Bearer token)
- Auth: `Authorization: Bearer {ApiToken}` from options (never exposed to browser)

**Preset panel queries** (ported from Nerd Consent `consent-host-overview.json`):

| PanelId | SigNoz metric / filter | Aggregation |
|---------|------------------------|-------------|
| RequestRate | `signoz_latency.count` | rate, sum, filter `service.name` |
| P95Latency | `signoz_latency.bucket` | p95, filter `service.name` |
| ErrorRate5xx | `signoz_calls_total` | rate, filter `http.status_code >= 500` |
| ErrorPercentage | `signoz_calls_total` | error rate % |

Implement query builders in `SigNoz/SignozQueryBuilder.cs` — one method per panel, unit-tested with golden JSON fixtures.

**Version note:** Target SigNoz **v0.135.x** (current Hetzner deploy). Adapter tests pin sample responses; breaking API changes bump minor package version.

### 3.5 In-process backend (v1.1 — optional in v1.0)

`InProcessObservabilityBackend` uses:

- `System.Diagnostics.Metrics.MeterListener` for registered meters
- Optional bridge: if `TheNerdCollective.Blazor.SessionMonitor` is registered, surface `ActiveSessions` as `ObservabilityPanelId.ActiveCircuits`

No external HTTP — useful for local dev without SigNoz.

---

## 4. MudBlazor UI components

| Component | Purpose |
|-----------|---------|
| `ObservabilityDashboard.razor` | Full page: header, time range, 4 metric cards, 2 charts, external link |
| `ObservabilityOverviewCards.razor` | Grid of scalar metrics |
| `ObservabilityMetricCard.razor` | Title, value, unit, severity color, icon |
| `ObservabilityTimeSeriesChart.razor` | MudChart line chart wrapper |
| `ObservabilityServiceSelector.razor` | MudSelect bound to backend service list |
| `ObservabilityHealthBadge.razor` | Healthy / degraded / unhealthy chip |
| `ObservabilityExternalLink.razor` | "Open in SigNoz →" MudLink |
| `ObservabilityRefreshToolbar.razor` | Refresh + last updated timestamp |

**Parameters (dashboard root)**

```razor
<ObservabilityDashboard
    ServiceName="nerd-consent-host"
    TimeRangeMinutes="15"
    ShowExternalLink="true"
    ExternalLinkText="Åbn fuld observability i SigNoz" />
```

**Styling:** Use MudBlazor tokens only; no hardcoded hex. Optional `data-testid` on cards (`observability-metric-request-rate`, etc.) for Playwright.

**Registration (optional routed page — PlayBook pattern)**

```csharp
builder.Services.AddObservabilityDashboard(o => { … });

app.MapRazorComponents<App>()
   .AddObservabilityDashboardPage(app.Services);  // adds /nerd-observability if enabled
```

---

## 5. File tree (new projects)

```
src/TheNerdCollective.Blazor.Observability/
├── TheNerdCollective.Blazor.Observability.csproj
├── README.md
├── IObservabilityBackend.cs
├── IObservabilityDashboardService.cs
├── ObservabilityDashboardService.cs
├── ObservabilityDashboardOptions.cs
├── ObservabilityModels.cs
├── ObservabilityPanelCatalog.cs          # preset panel definitions
├── ObservabilityDashboardServiceCollectionExtensions.cs
├── ObservabilityDashboardEndpointExtensions.cs
├── Backends/
│   ├── SigNozObservabilityBackend.cs
│   ├── SigNozQueryBuilder.cs
│   ├── SigNozApiClient.cs
│   ├── SigNozAuthHandler.cs
│   └── InProcessObservabilityBackend.cs  # v1.1
└── Internal/
    └── ObservabilityJsonSerializerContext.cs

src/TheNerdCollective.MudComponents.ObservabilityDashboard/
├── TheNerdCollective.MudComponents.ObservabilityDashboard.csproj
├── README.md
├── ObservabilityDashboard.razor
├── ObservabilityOverviewCards.razor
├── ObservabilityMetricCard.razor
├── ObservabilityTimeSeriesChart.razor
├── ObservabilityServiceSelector.razor
├── ObservabilityHealthBadge.razor
├── ObservabilityExternalLink.razor
├── ObservabilityRefreshToolbar.razor
├── ObservabilityDashboardPage.razor        # optional @page /nerd-observability
├── ObservabilityDashboardOptions.cs        # UI-only options (show panels, layout)
├── ObservabilityDashboardServiceCollectionExtensions.cs
└── ObservabilityDashboardWebApplicationExtensions.cs

tests/TheNerdCollective.Blazor.Observability.Tests/
├── SigNozQueryBuilderTests.cs
├── ObservabilityDashboardServiceTests.cs
├── Fixtures/
│   ├── signoz-query-range-request-rate.json
│   └── signoz-query-range-response.sample.json
└── TheNerdCollective.Blazor.Observability.Tests.csproj

tests/e2e/observability-dashboard.spec.ts   # optional v1.1 — mock API or test host
```

---

## 6. Backlog slices (HR-176–HR-190)

### Fase 1 — Core + SigNoz adapter (MVP)

| ID | P | Task | Acceptance criteria |
|----|---|------|---------------------|
| **HR-176** | P0 | Scaffold both packages + sln + publish map | Projects build on net10.0; listed in `publish-packages.yml`; Apache-2.0 metadata; README stubs |
| **HR-177** | P0 | Core models + `IObservabilityBackend` | All DTOs in §3.2; XML docs on public API; no MudBlazor ref in Blazor package |
| **HR-178** | P0 | `SigNozQueryBuilder` + golden tests | 4 panel queries match `consent-host-overview.json` semantics; unit tests assert request JSON shape |
| **HR-179** | P0 | `SigNozObservabilityBackend` | Integration test with mocked `HttpMessageHandler`; parses v4 query_range response into time series |
| **HR-180** | P0 | `AddObservabilityDashboard()` + dashboard service | Options binding; `IObservabilityDashboardService` returns overview snapshot |
| **HR-181** | P0 | `MapObservabilityDashboardEndpoints()` | 4 routes return JSON; documented auth requirement |

### Fase 2 — MudBlazor UI

| ID | P | Task | Acceptance criteria |
|----|---|------|---------------------|
| **HR-182** | P0 | `ObservabilityMetricCard` + `ObservabilityHealthBadge` | Render value + severity; bUnit smoke test |
| **HR-183** | P0 | `ObservabilityTimeSeriesChart` | MudChart line series from `ObservabilityTimeSeriesResult` |
| **HR-184** | P0 | `ObservabilityDashboard` composed page | 4 cards + 2 charts + refresh; injects `IObservabilityDashboardService` |
| **HR-185** | P1 | `ObservabilityServiceSelector` + time range | User can switch service and 15m/1h/24h |
| **HR-186** | P1 | Installation guide | `docs/ObservabilityDashboard-Installation-Guide.md` mirroring SessionMonitor guide |

### Fase 3 — Dogfood + publish

| ID | P | Task | Acceptance criteria |
|----|---|------|---------------------|
| **HR-187** | P0 | Nerd Consent `/admin/ops` | Super-admin page uses package; links to `devops.nerdconsent.dk`; Playwright asserts 4 metric cards visible with non-empty or "no data" state |
| **HR-188** | P1 | Publish v1.0.0 | Both packages on nuget.org; tags `mudblazor;observability;signoz;opentelemetry` |
| **HR-189** | P2 | SessionMonitor panel embed | Optional `<SessionMonitorQuickBar />` slot on dashboard when SessionMonitor package present |
| **HR-190** | P2 | In-process backend | `InProcessObservabilityBackend` + runtime GC chart without SigNoz |

**Cross-repo (Nerd Consent — not HR):** NC-512 Dogfood observability dashboard (depends HR-187).

---

## 7. Host integration (Nerd Consent)

```csharp
// Program.cs
using TheNerdCollective.Blazor.Observability;

builder.Services.AddObservabilityDashboard(o =>
{
    o.DefaultServiceName = "nerd-consent-host";
    o.ExternalDashboardBaseUrl = "https://devops.nerdconsent.dk";
    o.SigNoz.BaseUrl = builder.Configuration["NerdObservability:SigNoz:BaseUrl"]
        ?? "http://nerd-consent-signoz-signoz-0:8080";
    o.SigNoz.ApiToken = builder.Configuration["NerdObservability:SigNoz:ApiToken"];
    o.SigNoz.OrgId = builder.Configuration["NerdObservability:SigNoz:OrgId"];
});

// After build — restrict to admin policy
app.MapObservabilityDashboardEndpoints()
   .RequireAuthorization("SuperAdmin"); // host-defined policy
```

```razor
@* Components/Pages/Admin/Ops.razor *@
@page "/admin/ops"
@attribute [Authorize(Policy = PortalPolicies.SuperAdmin)]

<PageTitle>Drift | Nerd Consent</PageTitle>
<ObservabilityDashboard ServiceName="nerd-consent-host" />
```

**Config (server-only secrets):**

```json
{
  "NerdObservability": {
    "SigNoz": {
      "BaseUrl": "http://nerd-consent-signoz-signoz-0:8080",
      "ApiToken": "<editor-api-key>",
      "OrgId": "<org-uuid>"
    }
  }
}
```

---

## 8. Testing strategy

| Layer | Tool | Scope |
|-------|------|-------|
| Query builder | xUnit + golden JSON | SigNoz request payloads |
| Backend | xUnit + `HttpClient` mock | Response parsing, error handling |
| Dashboard service | xUnit | Overview aggregation, cache TTL (if added) |
| UI | bUnit | Metric card, empty state, loading skeleton |
| E2E | Playwright | Consent `/admin/ops` — roles, testids, computed visibility |
| Manual | SigNoz v0.135 | Compare panel values with imported dashboard |

---

## 9. Security & ops

- **Never** pass SigNoz API token to WASM/client — Blazor Server only for v1
- Document `[Authorize]` on ops page and minimal API routes
- `ExternalDashboardBaseUrl` opens full SigNoz (may be auth-protected separately)
- Redaction: dashboard shows aggregates only — no trace payloads, no PII tags
- Rate-limit refresh: default auto-refresh 60s; manual refresh button

---

## 10. Publish checklist

- [ ] Bump `<Version>` in both csproj files
- [ ] Add entries to `.github/workflows/publish-packages.yml` `PACKAGES` map
- [ ] `dotnet sln add` both projects + test project
- [ ] README with install snippet (mirror SessionMonitor)
- [ ] Root `README.md` package catalog section
- [ ] Tag `{PackageId}-v{Version}` on merge to `main`

---

## 11. Future (post v1)

| Idea | Package |
|------|---------|
| Prometheus backend | `Blazor.Observability.Prometheus` |
| Grafana embed panel | `MudComponents.ObservabilityDashboard.Grafana` |
| Trace list (top 10 slow) | v1.2 in core dashboard |
| Alert status strip | Read SigNoz alert rules API |
| JSON panel loader | Import SigNoz dashboard widget JSON dynamically (HR parity with deploy scripts) |

---

## 12. Relationship to SessionMonitor

| SessionMonitor | Observability Dashboard |
|----------------|-------------------------|
| Blazor circuits / sessions | HTTP, DB, runtime, external services |
| In-process only | Backend query (SigNoz) + optional in-process |
| Deployment windows | Error rate / latency SLOs |
| Complementary | Embed SessionMonitor widgets in ops dashboard (HR-189) |

---

## Agent prompt (copy-paste)

```text
Implement HR-176 through HR-181 from docs/00-backlogs/00-observability-dashboard-plan.md
and docs/BACKLOG.md. Follow SessionMonitor patterns. BACKLOG REPORT when done.
```
