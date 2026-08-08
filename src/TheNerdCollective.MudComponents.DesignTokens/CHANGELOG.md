# Changelog

## 2.2.5

- `/nerd-theme` coverage is brand-relevant: Typography is **External**, intentional Mud fallbacks are **AcceptedDefault**, and only real **Unmapped** gaps dilute the %.

## 2.2.4

- Publish pipeline: DesignTokens.Catalog is included in NuGet auto-publish (hosts can consume `/nerd-theme` via package).

## 2.2.3

- Added `NerdMudThemeMappingTools` + `/nerd-theme` catalog route for precise token → `MudTheme` prop mapping.
- `/nerd-theme` lists the **complete** MudBlazor `MudTheme` inventory (Palette, Layout, Shadows[0–25], ZIndex, Typography, PseudoCss) with Mapped / Hardcoded / Derived / Unmapped status so missing mappings are visible.

## 2.2.2

- **NRDT001:** flag `MudTabs` with `primary-action` / status intents or `Color.Primary` — active tab text uses the channel color on page-surface (light green on white). Prefer `BrandChrome`.

## 2.2.1

- **Style guard:** `CssPaintsOutlinedBrandChromeWithContrastText` scopes to `:root` only so intentional matching-surface `on-brand-chrome` paint under `[data-nerd-token="…-brand-chrome"]` is not a false positive (HttpBridge FailOnAccessibility).

## 2.2.0

- **NRDT001:** also flags Outlined/Text `Color="Color.Primary"` (and Info/Success/Warning/Secondary/Tertiary) without token Class.
- **NRDT001:** flags `MudLink` with action/status token intents (`dnf-primary-action`, etc.).
- **NRDT001:** flags `MudAlert` with `Severity.*` but no design-token `Class` (Mud theme severity on page-surface).
- **CSS:** same-intent controls on matching `data-nerd-token` surface (e.g. brand-chrome PlayBook cell) use `on-brand-chrome` paint.
- MudBlazor **9.8.0** pin + `reference/mudblazor/9.8.0` archive.

## 2.1.10

- MudBlazor **9.8.0** pin + `reference/mudblazor/9.8.0` archive (palette unchanged from 9.7.0).

## 2.1.9

- **NRDT001:** flag Outlined/Text + `info` / `success` / `highlight` / `flod` (not only muted/primary/page-surface).
- Style guard: `ValidateOutlinedStatusIntentWarnings` + startup warn; catalog shows `outlined-status-intent:*` (advisory — Filled status still OK).


## 2.1.0

- Brand pack registry: `NerdEmbeddedBrandPack`, `AddNerdDesignTokensFromBrand`, `RegisterBrandPack`.
- `NerdMudThemeFactory`, `NerdMudThemeProvider`, intent pseudo CSS themes.
- Embedded reference brand JSON (`dnf`, `tnc`, `acme`, `demo`) ships in package.

## 2.0.0

- Complete MudBlazor 9.6 palette coverage: all 80 `--mud-palette-*` variables are now mapped per token.
- Added `MudBlazorPaletteMapper` with derived darken, lighten, and RGB channels for every color role.
- Added `MudBlazorComponentRuleBuilder` with pattern-based selectors covering buttons, chips, alerts, inputs, tables, data grids, navigation, ratings, switches, and structural components.
- Descendant selectors (`.token .mud-*`) support tokens applied on container elements such as `MudGrid`.
- Design token catalog previews now include icon buttons, FABs, badges, form controls, ratings, and progress bars.

## 1.9.0

- WCAG contrast checks now resolve CSS variables such as `var(--dnf-color-forest)`.

## 1.8.0

- Design token catalog now wraps MudBlazor previews in `NerdCatalogThemeProvider` with dark-mode toggle.
- Clipboard and export buttons load `NerdDesignSystemScripts` automatically.
- Default catalog route moved to `Pages/NerdDesignTokensPage.razor` for custom-route support.
- Hub links sync `CatalogRoute` at DI registration time.

## 1.7.0

- Added `TheNerdCollective.MudComponents.Shared` dependency for WCAG helpers, clipboard copy, and design-system hub links.
- Added `DarkContrastText` on `NerdColorToken` for dark-mode-specific foreground colors.
- Added `UseImportantOverrides` option (default `true`) to control `!important` on component selectors.
- Added `ConfiguredColors` and `ConfiguredAliases` tracking on `NerdDesignTokenOptions`.
- Catalog exports CSS, JSON, and Stitch `DESIGN.md` files and shows configured/alias badges.
- `AddNerdDesignTokenCatalog` now registers the catalog assembly for discovery at `/nerd-design-tokens`.
- Startup accessibility validator now runs only when colors are configured.

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
