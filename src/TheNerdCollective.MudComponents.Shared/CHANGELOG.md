# Changelog

## 1.2.0

- Added `hsl()` / `hsla()` and `var(--token)` resolution to `NerdColorParser`.
- Added XML documentation for the Shared public API.

## 1.1.0

- Added `NerdDesignSystemScripts` to load `nerd-shared.js` for clipboard and download helpers.
- Added `NerdCatalogThemeProvider` for consistent MudBlazor theme context in catalogs.
- `AddNerdDesignSystem` now merges configuration from multiple package registrations.
- Added `hsl()` / `hsla()` support to `NerdColorParser`.
- Split default hub route into `Pages/NerdDesignSystemPage.razor` (component no longer owns `@page`).

## 1.0.0

- Added `WcagStandards` and `NerdColorParser` for shared contrast calculations (hex and `rgb()`/`rgba()`).
- Added `NerdClipboardService`, `NerdDownloadService`, and `NerdClipboardButton` with `wwwroot/nerd-shared.js`.
- Added design-system hub at `/nerd-design-system` via `AddNerdDesignSystem()` and `AddNerdDesignSystemHub()`.
