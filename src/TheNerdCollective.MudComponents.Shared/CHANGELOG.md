# Changelog

## 1.5.7

- WCAG guide CTA uses BrandChrome Filled (not PrimaryAction lime fill as only emphasis).

## 1.5.6

- Catalog layout: `NerdCatalogPage` is the only width frame (ExtraLarge). Hub nav counts always come from `NerdDesignSystemOptions` so badges match across pages. Hub/WCAG no longer use Medium.

## 1.5.5

- `NerdWcagGuide`: info alert uses token `Info` + `Outlined` (WCAG 1.4.3 on page-surface).
- `NerdClipboardButton`: default intent is `brand-chrome` (documented; NRDT001-safe on outlined).

## 1.5.4

- MudBlazor **9.8.0** package pin.

## 1.5.3

- Added `NerdCatalogPageHeader` + `NerdCatalogHubNav` for aligned catalog menus (BrandChrome Outlined/Filled, no route chips).

## 1.5.0

- Added `INerdBrandPackImportSink` for design-system hub token-pack import.

## 1.4.0

- Added `PlayBookRoute` to `NerdDesignSystemOptions` and a PlayBook link on the design system hub.

## 1.3.0

- Added `NerdColorDerivatives` for lighten, darken, and RGB tuple derivation from supported CSS colors.

## 1.2.0

- Added `hsl()` / `hsla()` and `var(--token)` resolution to `NerdColorParser`.
- Added XML documentation for the Shared public API.

## 1.1.0

- Initial shared catalog utilities and WCAG guide.
