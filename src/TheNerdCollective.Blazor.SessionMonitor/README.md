# TheNerdCollective.Blazor.SessionMonitor

Track and monitor active Blazor Server sessions/circuits in real-time. Get insights into current load, historical usage patterns, and identify optimal deployment windows.

[![NuGet](https://img.shields.io/nuget/v/TheNerdCollective.Blazor.SessionMonitor.svg)](https://www.nuget.org/packages/TheNerdCollective.Blazor.SessionMonitor/)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)

## Features

✅ **Real-time Session Tracking** - Monitor active Blazor Server circuits  
✅ **Historical Metrics** - Track session counts over time (last 10k snapshots)  
✅ **Deployment Windows** - Find optimal times to deploy with zero/minimal active users  
✅ **REST API Endpoints** - Query metrics via HTTP for monitoring dashboards  
✅ **Zero Configuration** - Works out-of-the-box with sensible defaults  
✅ **Lightweight** - Minimal performance overhead

## Installation

```bash
dotnet add package TheNerdCollective.Blazor.SessionMonitor
```

## Quick Start

### 1. Register Services

In your `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Blazor Server services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add session monitoring
builder.Services.AddSessionMonitoring();

var app = builder.Build();

// Map session monitoring API endpoints (optional but recommended)
app.MapSessionMonitoringEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

### 2. Query Current Sessions

Inject `ISessionMonitorService` anywhere in your app:

```csharp
@inject ISessionMonitorService SessionMonitor

<p>Active Sessions: @metrics.ActiveSessions</p>
<p>Peak Sessions: @metrics.PeakSessions</p>

@code {
    private SessionMetrics metrics = new();

    protected override void OnInitialized()
    {
        metrics = SessionMonitor.GetCurrentMetrics();
    }
}
```

### 3. Find Deployment Windows

```csharp
@inject ISessionMonitorService SessionMonitor

<h3>Optimal Deployment Windows (Next 24 Hours)</h3>

@foreach (var window in windows.Take(5))
{
    <div class="deployment-window">
        <strong>@window.StartTime.ToLocalTime() - @window.EndTime.ToLocalTime()</strong>
        <p>Max Sessions: @window.MaxActiveSessions</p>
        <p>Avg Sessions: @window.AverageActiveSessions.ToString("F2")</p>
        @if (window.ZeroSessionsWindow)
        {
            <span class="badge">✅ Zero Sessions</span>
        }
    </div>
}

@code {
    private IEnumerable<DeploymentWindow> windows = Array.Empty<DeploymentWindow>();

    protected override void OnInitialized()
    {
        // Find 5-minute windows with minimal active sessions in last 24 hours
        windows = SessionMonitor.FindOptimalDeploymentWindows(
            windowMinutes: 5,
            lookbackHours: 24
        );
    }
}
```

## API Endpoints

Once you call `app.MapSessionMonitoringEndpoints()`, the following endpoints are available:

### Get Current Metrics

```http
GET /api/session-monitor/current
```

**Response:**
```json
{
  "activeSessions": 5,
  "timestamp": "2026-01-28T14:30:00Z",
  "peakSessions": 12,
  "totalSessionsStarted": 150,
  "totalSessionsEnded": 145,
  "averageSessionDurationSeconds": 320.5
}
```

### Get Historical Data

```http
GET /api/session-monitor/history?since=2026-01-28T00:00:00Z&maxCount=100
```

**Response:**
```json
[
  {
    "timestamp": "2026-01-28T14:30:00Z",
    "activeSessions": 5,
    "sessionsStarted": 2,
    "sessionsEnded": 1
  },
  ...
]
```

### Get Active Circuit IDs

```http
GET /api/session-monitor/active-circuits
```

**Response:**
```json
{
  "activeCircuits": [
    "circuit-abc123",
    "circuit-def456"
  ],
  "count": 2
}
```

### Find Deployment Windows

```http
GET /api/session-monitor/deployment-windows?windowMinutes=5&lookbackHours=24
```

**Response:**
```json
[
  {
    "startTime": "2026-01-28T03:00:00Z",
    "endTime": "2026-01-28T03:05:00Z",
    "maxActiveSessions": 0,
    "averageActiveSessions": 0,
    "zeroSessionsWindow": true
  },
  ...
]
```

### Check if Deployment is Safe

```http
GET /api/session-monitor/can-deploy?maxActiveSessions=0
```

**Response:**
```json
{
  "canDeploy": true,
  "currentActiveSessions": 0,
  "threshold": 0,
  "timestamp": "2026-01-28T14:30:00Z"
}
```

## Use Cases

### 1. Production Monitoring Dashboard

```csharp
@page "/admin/sessions"
@inject ISessionMonitorService SessionMonitor
@implements IDisposable

<h2>Session Monitor Dashboard</h2>

<div class="metrics-grid">
    <div class="metric-card">
        <h4>Active Sessions</h4>
        <div class="metric-value">@metrics.ActiveSessions</div>
    </div>
    <div class="metric-card">
        <h4>Peak Today</h4>
        <div class="metric-value">@metrics.PeakSessions</div>
    </div>
    <div class="metric-card">
        <h4>Total Started</h4>
        <div class="metric-value">@metrics.TotalSessionsStarted</div>
    </div>
    <div class="metric-card">
        <h4>Avg Duration</h4>
        <div class="metric-value">
            @(metrics.AverageSessionDurationSeconds?.ToString("F1") ?? "N/A")s
        </div>
    </div>
</div>

@code {
    private SessionMetrics metrics = new();
    private Timer? refreshTimer;

    protected override void OnInitialized()
    {
        RefreshMetrics();
        refreshTimer = new Timer(_ => InvokeAsync(() =>
        {
            RefreshMetrics();
            StateHasChanged();
        }), null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }

    private void RefreshMetrics()
    {
        metrics = SessionMonitor.GetCurrentMetrics();
    }

    public void Dispose()
    {
        refreshTimer?.Dispose();
    }
}
```

### 2. Deployment Automation Script

```bash
#!/bin/bash

# Check if deployment is safe (zero active sessions)
RESPONSE=$(curl -s http://your-app.com/api/session-monitor/can-deploy?maxActiveSessions=0)
CAN_DEPLOY=$(echo $RESPONSE | jq -r '.canDeploy')

if [ "$CAN_DEPLOY" = "true" ]; then
  echo "✅ Safe to deploy - no active sessions"
  # Proceed with deployment
  ./deploy.sh
else
  ACTIVE=$(echo $RESPONSE | jq -r '.currentActiveSessions')
  echo "❌ Cannot deploy - $ACTIVE active sessions"
  
  # Find next available window
  WINDOWS=$(curl -s "http://your-app.com/api/session-monitor/deployment-windows?windowMinutes=5")
  NEXT_WINDOW=$(echo $WINDOWS | jq -r '.[0].startTime')
  echo "Next available window: $NEXT_WINDOW"
  exit 1
fi
```

### 3. Capacity Planning

```csharp
// Analyze peak usage patterns
var last7Days = DateTime.UtcNow.AddDays(-7);
var history = SessionMonitor.GetHistory(since: last7Days, maxCount: 10000);

var hourlyPeaks = history
    .GroupBy(s => s.Timestamp.Hour)
    .Select(g => new
    {
        Hour = g.Key,
        PeakSessions = g.Max(s => s.ActiveSessions),
        AvgSessions = g.Average(s => s.ActiveSessions)
    })
    .OrderByDescending(x => x.PeakSessions);

foreach (var peak in hourlyPeaks)
{
    Console.WriteLine($"Hour {peak.Hour:00}:00 - Peak: {peak.PeakSessions}, Avg: {peak.AvgSessions:F1}");
}
```

## Configuration

### Custom API Endpoint Path

```csharp
app.MapSessionMonitoringEndpoints("/monitoring/sessions");
```

### History Retention

By default, the service retains the last 10,000 snapshots in memory. Snapshots are recorded when:
- Session count changes
- At least 1 minute has passed since last snapshot

For persistent storage, implement a custom `ISessionMonitorService` backed by a database.

## API Reference

### ISessionMonitorService

| Method | Description |
|--------|-------------|
| `GetCurrentMetrics()` | Get current session statistics |
| `GetHistory(since?, maxCount)` | Get historical snapshots |
| `GetActiveCircuitIds()` | Get all active circuit IDs |
| `HasActiveSessions()` | Quick check if any sessions exist |
| `FindOptimalDeploymentWindows(windowMinutes, lookbackHours)` | Find low-activity periods |

### SessionMetrics

| Property | Type | Description |
|----------|------|-------------|
| `ActiveSessions` | int | Currently active sessions |
| `Timestamp` | DateTime | When metrics were captured |
| `PeakSessions` | int | Highest concurrent sessions since start |
| `TotalSessionsStarted` | long | All-time session starts |
| `TotalSessionsEnded` | long | All-time session ends |
| `AverageSessionDurationSeconds` | double? | Average session length |

### DeploymentWindow

| Property | Type | Description |
|----------|------|-------------|
| `StartTime` | DateTime | Window start |
| `EndTime` | DateTime | Window end |
| `MaxActiveSessions` | int | Peak sessions in window |
| `AverageActiveSessions` | double | Average sessions in window |
| `ZeroSessionsWindow` | bool | True if max = 0 |

## Performance Considerations

- **Memory**: ~10MB for 10k snapshots (default retention)
- **CPU**: Negligible - only tracks circuit open/close events
- **Thread-safe**: All operations use concurrent collections
- **Singleton Service**: Maintains state across entire app lifetime

## License

Licensed under the **Apache License 2.0**. See [LICENSE](LICENSE) for details.

Copyright © 2026 The Nerd Collective Aps

## Support

For issues, feature requests, or questions:
- 🐛 [GitHub Issues](https://github.com/janhjordie/TheNerdCollective.Components/issues)
- 📧 Contact: [The Nerd Collective](https://www.thenerdcollective.dk/)

## Built by

[The Nerd Collective Aps](https://www.thenerdcollective.dk/)  
Developed by [Jan Hjørdie](https://github.com/janhjordie/)

---

Built with ❤️ by [The Nerd Collective Aps](https://www.thenerdcollective.dk/)
