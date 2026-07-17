# Changelog

## 1.6.0

- `AddNerdDesignTokenCatalog(app.Services)` now respects `EnableCatalogPage` and skips registration when disabled.

## 1.5.0

- WCAG badges now show version, contrast ratio, and separate light/dark results.
- Added catalog warning banner and per-token contrast recommendations.
- Added startup accessibility warnings via hosted service logging.

## 1.4.0

- Added visual design token catalog page at `/nerd-design-tokens` (configurable).
- Added dark mode preview, WCAG badges, aliases, radius, and shadow sections.
- Added `AddNerdDesignTokenCatalog()` for Razor component discovery.

## 1.3.0

- Added opt-in CSS layers, scopes, deterministic ordering, and minification option.
- Added AAA accessibility reporting and recommended contrast text.
- Added Google Stitch `DESIGN.md` export support.
- Added configurable fallback and version-profile hooks for MudBlazor mappings.

## 1.2.0

- Added CSS `@layer nerd-design-tokens` isolation.
- Added token aliases, radius and shadow tokens.
- Added build-time CSS writing, JSON export, and WCAG AA checks.
- Added documented fallback behavior for missing token variants.

## 1.1.0

- Added light/dark token values and automatic contrast text fallback.
- Added semantic surface, content, and interactive roles.
- Added stronger CSS value validation and additional interaction states.
- Replaced raw `string` CSS injection with a typed `NerdDesignTokenCss` service.
- Expanded MudBlazor variable and component-state mappings.

## 1.0.0

- Added customer-specific CSS design tokens for MudBlazor 9.6.
- Added CSS generation for common MudBlazor component variants and states.
- Added DI setup and `NerdDesignTokenStyles`.
