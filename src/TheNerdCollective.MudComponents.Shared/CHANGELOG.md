# Changelog

## 1.4.0

- Added `PlayBookRoute` to `NerdDesignSystemOptions` and a PlayBook link on the design system hub.

## 1.3.0

- Added `NerdColorDerivatives` for lighten, darken, and RGB tuple derivation from supported CSS colors.

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
