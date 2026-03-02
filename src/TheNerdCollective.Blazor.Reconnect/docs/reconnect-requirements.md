# Reconnection Requirements & Behaviour Specification

**Package**: `TheNerdCollective.Blazor.Reconnect`  
**Last updated**: 2026-03-02 | **Current version**: `1.5.2`

> **Core principle**: The component must **never permanently give up**. Showing a dead "Unable to reconnect" screen while the server is running is unacceptable. The reconnect handler operates in two phases: (1) try to resume the circuit, (2) if the circuit is gone, poll until the server is reachable and then reload automatically.

---

## Background — How Blazor Server Reconnection Works

Blazor Server maintains a persistent SignalR **circuit** between the browser and the server. Every interactive component, event handler and render tree lives inside this circuit. When the connection is lost, Blazor attempts to reconnect to the **same circuit** using the circuit ID it stored internally in session storage.

### Circuit lifecycle

```
Browser connects → circuit created (ID assigned)
        │
        ▼
Connection lost (network drop, app backgrounded, server restart…)
        │
        ▼
╔══════════════════════════════╗     starts 3s after disconnect
║  PHASE 1: Circuit retry loop ║ ══════════════════════════════════╗
║  (maxRetries=1000, ≈ ∞)      ║     ╔══════════════════════════════════════╗
╚══════════════════════════════╝     ║  PHASE 2: Server-alive ping loop      ║
        │                            ║  runs IN PARALLEL with Phase 1        ║
   ┌────┴──────────────┐             ║  (polls indefinitely every 5s)        ║
   │                   │             ╚══════════════════════════════════════╝
   ▼                   ▼                      │
Reconnect succeeds   Circuit dead        ┌────┴────────────────┐
(circuit alive)      → Phase 1 keeps    │                     │
   │                   retrying     Server down          Server responds
   ▼                                → keep pinging       → auto-reload
hide() fires                                                   │
→ circuitReconnected=true                            window.location.reload()
→ AbortController.abort()                            (new circuit starts)
→ stopServerPing() cancels Phase 2
```

Key point: **Phase 2 starts 3 seconds after disconnect** (`serverPingStartDelayMilliseconds`), not after Phase 1 exhausts.
- If Blazor reconnects the circuit within 3s → `hide()` fires → sets `circuitReconnected = true` + `AbortController.abort()` cancels the Phase 2 timer before it fires (brief blip, no reload)
- If the circuit is dead → Phase 2 is already pinging well before Phase 1 even notices
- `maxRetries=1000` makes Phase 1 effectively infinite — Phase 2 is the sole exit mechanism when the circuit is gone

#### Why 3 seconds is safe — AbortController + `circuitReconnected` guard

The short 3s delay is made safe by two mechanisms working together:

1. **`AbortController`**: Each `fetch()` call in Phase 2 is given the signal of a fresh `AbortController`. When `hide()` fires (Blazor circuit reconnected), `stopServerPing(circuitRestored=true)` calls `controller.abort()`, instantly cancelling any in-flight request. The `AbortError` is silently swallowed.

2. **`circuitReconnected` flag**: Even if a Phase 2 response arrives after `abort()` (race condition — extremely unlikely), the handler checks `if (circuitReconnected) return` before calling `window.location.reload()`. Belt-and-suspenders.

> **Why not use sessionStorage?** Blazor Server's circuit reconnect token is **not** stored in `sessionStorage` in any accessible or documented form. It is internal to the SignalR `HubConnection` — not observable from application JS. The only `sessionStorage` key found in `blazor.web.js` is a WASM authentication key, unrelated to Server circuit reconnect. `AbortController` is the correct and sufficient mechanism.

### Server-side circuit lifetime

The server holds a **disconnected circuit** in memory for a configurable idle timeout:

```csharp
// In Program.cs / AddRazorComponents()
builder.Services.AddServerSideBlazor(options =>
{
    options.DisconnectedCircuitMaxRetained = 100;
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3); // default
});
```

If the user reconnects within this window → seamless resume.  
If the window expires → the circuit is garbage-collected → reconnect attempt hits a dead circuit → `failed()`.

---

## Scenario Requirements

### 1. Mobile — App Backgrounded (iPhone / Android)

**Trigger**: User opens the site in Safari/Chrome, switches to another app (Maps, Messages, etc.), returns to the browser after an indeterminate time.

**What happens on the OS level**:
- iOS Safari freezes JavaScript execution when the browser is backgrounded after ~30 seconds.
- The WebSocket (SignalR) connection is dropped by iOS, typically within 30s–2 minutes of backgrounding.
- When the user returns, the browser tab is resumed — JS execution restarts from where it froze.

**Expected behaviour**:

| Time away | Result |
|---|---|
| < ~3 min (within `DisconnectedCircuitRetentionPeriod`) | Reconnect modal appears briefly, then **auto-hides** as Blazor reconnects to the existing circuit. No state lost. |
| > ~3 min | Reconnect modal appears → retries exhaust → **failed UI** shown with "Reload page" button. User must reload; a new circuit starts. |

**Requirements**:
- [x] Reconnect modal **must appear immediately** when the user returns to the tab (within the 1s polling interval).
- [x] If reconnect succeeds, modal **must disappear** without user interaction.
- [x] If retries exhaust (Phase 1 ends), modal **must switch to the "server polling" state** — NOT a permanent dead-end "Unable to reconnect" screen.
- [x] In Phase 2 (server polling), the component polls a configurable endpoint (default `/health`) at a configurable interval and **auto-reloads** when it gets a successful response.
- [x] The "failed" UI must update to communicate "Still trying to reach the server…" rather than implying the situation is permanent.
- [x] `maxRetries` and `retryIntervalMilliseconds` must be configurable so the total Phase 1 window can be tuned to match `DisconnectedCircuitRetentionPeriod`.
- [x] Phase 2 polling must also be configurable and must default to **enabled**.
- [x] **`visibilitychange` event** — when the user returns to the tab and the reconnect modal is showing, an immediate one-shot health check fires instantly (bypassing `serverPingStartDelayMilliseconds` and the current interval position). Discovery time collapses to one network RTT (~100–300 ms) instead of up to 8 seconds with defaults.
- [x] **`pageshow` bfcache reload** — when iOS restores the page from its back/forward cache (`event.persisted === true`), the page reloads immediately to obtain a fresh Blazor circuit. Without this, the page appears to load but the circuit is dead and the modal never fires.

**Recommended alignment**:
```
maxRetries = 1000 (≈ infinite)          →  Phase 1 never stops by itself
serverPingStartDelayMilliseconds = 3000 →  Phase 2 starts 3s after disconnect (safe: AbortController cancels if Blazor reconnects first)
serverPingIntervalMilliseconds = 5000   →  Phase 2 checks every 5s, indefinitely
```
The Phase 1 window should be long enough to survive a brief network blip or slow server restart,
but short enough that the component quickly enters Phase 2 and starts polling if the server is down.

---

### 2. Desktop — Tab Left Inactive

**Trigger**: User opens the site in a desktop browser, switches to another tab or window, or the computer sleeps/locks, for some period.

**What happens**:
- Desktop browsers throttle background tabs but generally do NOT freeze JS the way iOS does.
- WebSockets are kept alive by the OS TCP stack as long as the machine is not sleeping.
- If the machine sleeps, the TCP connection drops. On wake, the OS resumes the browser and SignalR's reconnect logic fires.

**Expected behaviour**: Identical to Mobile — see Scenario 1.

**Additional consideration**: Desktop browsers may not visually resurface the tab after sleep. The reconnect modal should be visible the moment the user clicks back to the tab.

---

### 3. Local Development — App Stop / Restart (dotnet run / hot reload)

**Trigger**: Developer stops `dotnet run` (Ctrl+C) or the watch recompiles, then starts the app again. The browser tab is still open.

**What happens**:
- Server process dies → WebSocket immediately drops → `components-reconnect-show` fires.
- All circuits are destroyed (server memory wiped) — **the circuit is 100% gone**.
- When the server is back, Blazor reconnects the SignalR transport, but the circuit no longer exists on the server.
- Blazor calls `failed()` after all retries are exhausted.

**Expected behaviour** (this is the canonical driver for Phase 2 behaviour):
- [x] Reconnect modal appears immediately after the server stops.
- [x] Phase 1 retries run while the developer waits for the server to restart.
- [x] If the server comes back **within the Phase 1 retry window**, BUT the circuit is dead, `failed()` fires (correct — Blazor reconnects the transport, then immediately fails circuit negotiation).
- [x] Whether `failed()` fires during or after Phase 1, **Phase 2 polling starts immediately**.
- [x] Phase 2 polls the server (e.g. `GET /health`) until a `2xx` response is received.
- [x] On success: **auto-reload the page** — developer gets a fresh circuit without any manual interaction.
- [x] The UI during Phase 2 must NOT say "Unable to reconnect" (implying permanent failure). Instead: "Server is restarting — will reload automatically…" or similar.

**This is the scenario that must work**: the developer restarts their app and the browser tab recovers on its own.

---

### 4. Network Drop — WiFi → Mobile Data Switch (or complete outage)

**Trigger**: User loses WiFi and the device switches to mobile data, or there is a brief complete outage.

**Expected behaviour**:
- [x] Reconnect modal appears when SignalR drops.
- [x] If network is restored and circuit is still alive → auto-hide on successful reconnect.
- [x] If network outage was long enough to expire the circuit → Phase 1 exhausted → Phase 2 starts.
- [x] Phase 2 polls the health endpoint. Once reachable → auto-reload.
- [x] **The component must never sit permanently on a dead "Unable to reconnect" screen** while the server is reachable.

---

### 5. Server Deployment (New Version)

**Trigger**: A new version of the app is deployed while users are connected.

**Handled by**: `TheNerdCollective.Blazor.VersionMonitor` — shows a "new version available" banner.  
**Also handled by**: `blazor-reconnect.js` error suppression, which detects `The list of component operations is not valid` and triggers a reload.

This scenario is **out of scope** for the reconnect handler's modal UI.

---

## Circuit ID Access

### From JavaScript

Blazor Server does **not** expose the circuit ID in a public JavaScript API. Internally it stores reconnection state in `sessionStorage` (key: `_blazor-reconnect`), but the format is private and subject to change.

```js
// ❌ Not recommended — private/fragile internal API
window.Blazor._internal.navigationManager.getCircuitId?.()
```

**Conclusion**: Do not attempt to read or persist the circuit ID from JavaScript.

### From C# (server-side)

On the server, the circuit ID is available via `ICircuitAccessor`:

```csharp
using Microsoft.AspNetCore.Components.Server.Circuits;

public class MyService
{
    private readonly ICircuitAccessor _circuitAccessor;

    public MyService(ICircuitAccessor circuitAccessor)
    {
        _circuitAccessor = circuitAccessor;
    }

    public string GetCircuitId() => _circuitAccessor.Circuit?.Id ?? "(no circuit)";
}
```

This is useful for **server-side logging, diagnostics, and analytics** (e.g. correlating SignalR logs with application logs).

### Is "reconnect to last known circuit" something we need to implement?

**No.** Blazor Server handles this automatically:

1. On first connect, Blazor creates a circuit and stores its reconnection token in the browser's `sessionStorage`.
2. On every reconnect attempt, the browser sends this token back to the server.
3. If the server finds the circuit → resumes it seamlessly.
4. If not (token is stale, circuit was GC'd, server restarted) → negotiation fails → `failed()` fires.

The reconnect handler only needs to:
- Show the reconnecting UI while Blazor is retrying.
- Show the failed UI when retries are exhausted.
- Hide the UI when reconnection succeeds.

There is no action we can take to "reconnect to a specific circuit" from the client side — that is entirely a server-side decision based on whether the circuit is still alive.

---

## Configuration Reference

| Option | Default | Description |
|---|---|---|
| `maxRetries` | `1000` | Effectively infinite Phase 1 retries. Phase 2 (server ping) is now the sole exit when the circuit is dead. |
| `retryIntervalMilliseconds` | `3000` | ms between Phase 1 retry attempts. |
| `serverPingEnabled` | `true` | Enable Phase 2 parallel ping. |
| `serverPingUrl` | `/health` | URL polled in Phase 2. Any `2xx` response triggers auto-reload. |
| `serverPingStartDelayMilliseconds` | `3000` | ms after disconnect before Phase 2 polling begins. Reduced from 10s to 3s (one Phase 1 retry interval) because `AbortController` + `circuitReconnected` guard make it safe to start Phase 2 early — if Blazor reconnects the circuit, the in-flight fetch is aborted instantly and a guard flag blocks any reload. |
| `serverPingIntervalMilliseconds` | `5000` | ms between Phase 2 poll attempts. Polls indefinitely. |
| `autoReloadOnServerBack` | `true` | Auto-reload when Phase 2 ping succeeds. If `false`, show a "Server is back" prompt instead. |
| `title` | `Connection lost` | Reconnecting modal heading (Phase 1). |
| `subtitle` | `The connection was interrupted…` | Reconnecting modal body text (Phase 1). |
| `statusText` | `Reconnecting…` | Reconnecting modal sub-status (Phase 1). |
| `reloadButtonText` | `Reload now` | Button label in Phase 1 modal. |
| `failedTitle` | `Waiting for server…` | Phase 2 modal heading. **Must NOT say "Unable to reconnect"** — that implies permanent failure. |
| `failedSubtitle` | `The connection was lost. Waiting for the server to become available…` | Phase 2 modal body. |
| `failedReloadButtonText` | `Reload page` | Manual reload button label in Phase 2 modal (always shown as fallback). |
| `logoUrl` | `null` | Logo URL shown above spinner/error icon. |
| `primaryColor` | `#594AE2` | Button and spinner colour. |
| `reconnectingHtml` | `null` | Full HTML override for Phase 1 state. |
| `failedHtml` | `null` | Full HTML override for Phase 2 state. |

### Tuning the retry window to match server idle timeout

```js
// Recommended production config (defaults — no config needed unless overriding):
// Phase 1: retry forever in background
// Phase 2: start pinging 3s after disconnect; AbortController cancels it if Blazor reconnects first
window.blazorReconnectConfig = {
    maxRetries: 1000,                       // ≈ infinite
    retryIntervalMilliseconds: 3000,
    serverPingEnabled: true,
    serverPingUrl: '/health',
    serverPingStartDelayMilliseconds: 3000, // 3s — safe due to AbortController + circuitReconnected guard
    serverPingIntervalMilliseconds: 5000,
    autoReloadOnServerBack: true
};

// If you want a manual "Server is back — reload?" prompt instead of auto-reload:
window.blazorReconnectConfig = {
    autoReloadOnServerBack: false
};

// Even shorter grace period (e.g. local dev — server restarts fast):
window.blazorReconnectConfig = {
    serverPingStartDelayMilliseconds: 1000,  // start pinging after 1s
    serverPingIntervalMilliseconds: 2000
};
```

---

## Hard Requirements (must implement)

- [x] **Phase 2 server-alive poll in parallel with Phase 1** (`serverPingEnabled`, `serverPingUrl`, `serverPingStartDelayMilliseconds`, `serverPingIntervalMilliseconds`): Phase 2 starts 3s after disconnect — in parallel with Phase 1, not after it. `AbortController` cancels in-flight fetches the instant `hide()` fires; `circuitReconnected` flag provides a belt-and-suspenders guard. Auto-reloads when server responds.
- [x] **Rename / rethink the "failed" state UI**: The default copy for Phase 2 must NOT say "Unable to reconnect". Default: "Waiting for server…" / "Checking server availability…"
- [x] **`autoReloadOnServerBack`** (default `true`): When Phase 2 ping succeeds, reload without user interaction.
- [x] **`maxRetries` effectively infinite** (`1000`): Phase 1 never stops on its own — Phase 2 is the sole exit when the circuit is dead.

## Open Questions / Future Work

- [ ] **Expose `onReconnecting` / `onReconnected` / `onFailed` / `onServerBack` JS callbacks** so host apps can add custom telemetry or analytics.
- [ ] **Persist `circuitId` server-side** (via `CircuitHandler`) for session continuity diagnostics — log when a circuit is resumed vs. when a new one is created.
- [x] **iOS PWA / Home Screen apps** (partially addressed): `visibilitychange` fires correctly in PWA standalone mode. `pageshow` bfcache reload also applies. Aggressive backgrounding behaviour may still need testing for very long sessions.
- [ ] **Phase 2 with no `/health` endpoint**: Consider falling back to polling the app root (`/`) or a signalr negotiate endpoint if `/health` is not available.
