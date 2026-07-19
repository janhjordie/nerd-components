# DesignTokens.Core extract (HR-114 / TS-014)

Framework-agnostic design token model lives in **`TheNerdCollective.DesignTokens.Core`**.

## Assembly boundaries

| Assembly | Responsibility | Must NOT |
|----------|----------------|----------|
| **DesignTokens.Core** | Pack JSON, validation, WCAG, intents (`--nerd-intent-*`), `NerdCoreCssGenerator`, tree/parity/catalog tools | Reference MudBlazor |
| **MudComponents.DesignTokens** | Mud harvest inventory, `MudBlazorDesignTokenCssGenerator`, Radzen/Fluent/Bootstrap/Blazorise bridges, style guard | Own pack model (use Core) |
| **DesignTokens.Catalog** | Workbook, colors UI, design guide | Duplicate token logic |

## CSS modes

| API | Output |
|-----|--------|
| `NerdCoreCssGenerator.Generate` | Variables + shell recipe regions only |
| `NerdDesignTokenMudTools.ExportMudCss` | Full Mud adapter (palette + component rules + bridges) |
| `NerdDesignTokenTools.WriteCss` | Core CSS (framework-neutral) |

## Mud harvest (wave 3)

All SCSS harvest + inventory validation is isolated behind **`NerdMudHarvestAdapter`**:

- `ValidateHarvestCoverage(mudVersion)` — CI gate for inventory YAML
- `ValidateGeneratedCss(options)` — rule table vs generated CSS
- `LoadRuleTable(mudVersion)` — inventory entries for parity panels

Non-Mud frameworks consume **component families** (`reference/component-families/*.yaml`) and intent CSS — not Mud harvest YAML.

## Wave status

| Wave | Delivered |
|------|-----------|
| 1 | Core project, pack model, `NerdCoreCssGenerator`, `NerdDesignTokenMudTools` |
| 2 | Tree, alias, linter, manual compliance, `NerdCoreParityTools` in Core |
| 3 | `NerdMudHarvestAdapter` façade, this doc, `NerdCoreAssemblyGuardTests` |

| Remaining: optional `TheNerdCollective.DesignTokens.Core` namespace migration (breaking); move Mud-only parity out of shared APIs. **Deferred to v2** — document only (HR-114 polish, batch 8). |

## Wave 4 (deferred)

| Item | Status |
|------|--------|
| Namespace migration to `TheNerdCollective.DesignTokens.Core` | Parked — breaking change for consumers |
| Mud-only APIs removed from shared entry points | Parked |
