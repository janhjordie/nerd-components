# Changelog

All notable changes to `TheNerdCollective.Blazor.Reconnect` should be documented in this file.

## 1.12.0 - 2026-05-20

- Hardened the reconnect failure path so `keepReconnectingUiOnFailure` also keeps the primary The Nerd Collective reconnect dialog active when Blazor's polling fallback observes a failed state.
- Permanently suppresses the built-in `#components-reconnect-modal` so host apps no longer risk showing Blazor's default reconnect overlay alongside the custom reconnect experience.
- Verified BilletSalg's public and admin hosts with fixed 5-second retry and health-ping intervals so reconnect keeps probing indefinitely until the app is back.

## 1.11.0 - 2026-05-20

- Added .NET 10 reconnect display compatibility by implementing `update(...)` on the custom reconnect display hook.
- Added support for both `window.blazorReconnectConfig` and `window.blazorReconnectionConfig` so existing host apps do not silently miss configuration.
- Added `keepReconnectingUiOnFailure` so the primary The Nerd Collective reconnect dialog can remain active while `/health` polling continues indefinitely.
- Changed retry interval handling so array-based backoff keeps returning the last interval instead of stopping when the array is exhausted.
- Documented the BilletSalg always-reconnect profile and the new compatibility behavior in the package README.
- Verified the reconnect lifecycle against both public and admin hosts with focused Playwright regression coverage.

## 1.10.1

- Silent-first reconnect flow with grace period, health ping, scroll preservation, and lifecycle callbacks.