---
title: "TheNerdCollective.Components — Master Backlog"
status: Active
author: "@janhjordie"
last_updated: "18-07-2026 21:55"
id_prefix: "HR"
---

# Master Backlog — `HR-###`

Nerd Rules master backlog for **TheNerdCollective.Components**.  
Spec: nerd-rules `00-ai-system/13-master-backlog-spec.md` · Prompts: `backlog-slices` · Portfolio: [00-backlog-portfolio.md](00-backlogs/00-backlog-portfolio.md)

| Status | Meaning |
|--------|---------|
| **open** | Ready — not started |
| **in_progress** | Agent active this session |
| **blocked** | Waiting on human/external |
| **done** | DoD verified + evidence |
| **partial** | MVP shipped — optional hardening left |
| **parked** | Deferred |
| **wontfix** | Rejected |

**Sources:** Design Tokens [FEATURES.md](../src/TheNerdCollective.MudComponents.DesignTokens/docs/FEATURES.md) · Responsive Typography [FEATURES.md](../src/TheNerdCollective.MudComponents.ResponsiveTypography/docs/FEATURES.md)

---

## Inbox (untriaged)

| Captured | Source | Notes |
|----------|--------|-------|
| | | |

---

## Open — Design Tokens (HR-001–HR-020)

| ID | P | Status | Task | DoD (verifiable) | Evidence | Source |
|----|---|--------|------|------------------|----------|--------|
| HR-001 | P1 | done | Live Token Studio (in-browser) | Color picker + hex/rgb/hsl på `/nerd-design-tokens`; live MudButton/Chip/TextField preview; WCAG-meter opdateres live; undo/redo | MudColorPicker, apply, live WCAG chip, undo/redo stacks, TextField preview | DT-FEATURE-01 |
| HR-002 | P0 | done | Save as Client Pack (Design Tokens) | `INerdTokenPackStore` + JSON DTO; “Gem som klient” i catalog; gem/load under `App_Data/token-packs/`; aktivt pack efter genstart | Save/load/list i catalog; `ApplyTo` + CSS refresh; hub viser aktivt pack | DT-FEATURE-02 · ROADMAP-F2 |
| HR-003 | P1 | done | Token Discovery Everywhere | Ny token i pack → swatches i catalog, matrix i PlayBook, badge i Hub, export — uden hardcoded Razor-lister | Fuld pack JSON export + Stitch export med aliases/recipes; dynamisk discovery i catalog/PlayBook/hub | DT-FEATURE-03 |
| HR-004 | P1 | done | Recipe Composer | UI: vælg surface + content + action → generér recipe-klasse; preview MudCard + MudButton + MudText | Recipe composer i `NerdDesignTokensCatalog` med token-selects og live preview | DT-FEATURE-04 |
| HR-005 | P2 | done | Contrast Pair Matrix | Tabel forgrund × baggrund med ratio + AA/AAA i catalog (DNF-style) | `BuildContrastMatrix` + scrollbar matrix i catalog med light/dark toggle | DT-FEATURE-05 |
| HR-006 | P0 | done | Portal-aware Pickers | Date/Time/Select/Menu-popovers arver token-klasse; Playwright bekræfter `dnf-*` på `.mud-popover-open` | `nerd-shared.js` portal bridge (pointerdown + MutationObserver); `tests/e2e/portal-pickers.spec.ts` | DT-FEATURE-06 · ROADMAP-F0 |
| HR-007 | P2 | done | Figma / Tokens Studio Export | Eksport + import Tokens Studio JSON for farver | `ExportTokensStudioJson` + catalog export-knap | DT-FEATURE-07 |
| HR-008 | P2 | done | Stitch / DESIGN.md Sync | Udvid eksport med recipes, radii, shadows, typography hooks | Stitch export med aliases/recipes/radii/shadows + typography roles fra brand bundle | DT-FEATURE-08 |
| HR-009 | P1 | done | Accessibility Gate in CI | Test eller build-step fejler ved WCAG AA-brud på tokens | `AssertAccessibilityCompliance` + DNF gate test + `.github/workflows/ci.yml` | DT-FEATURE-09 |
| HR-010 | P2 | done | Token Diff & Changelog | Diff-view preset vs klient-pack (`+` `~` `-`) i catalog | `NerdTokenPackDiff` + changelog-tabel i catalog med baseline preset | DT-FEATURE-10 |
| HR-011 | P2 | done | Semantic Alias Browser | UI: `primary-action → himmel` med “hvor bruges den?” | Semantic alias browser med usage chips + DNF aliases | DT-FEATURE-11 |
| HR-012 | P1 | done | Dark Mode Dual Preview | Split-view light \| dark for samme token i catalog | Swatch toggle “Dual light \| dark preview” med side-by-side kolonner | DT-FEATURE-12 |
| HR-013 | P2 | done | Hover / Focus / Disabled Storyboard | State-strip pr. komponent: default → hover → focus → disabled | State storyboard i token detail tabs | DT-FEATURE-13 |
| HR-015 | P3 | done | Watermark / Opacity Tokens | Tokens med opacity til watermark/overlays (DNF-PDF) | `NerdOpacityToken`, CSS `color-mix` overlays, DNF watermark/hero-overlay preset, catalog section | DT-FEATURE-15 |
| HR-016 | P1 | done | Layout Kits (Hero, Nav, Footer) | PlayBook compositions: hero, link card, CTA strip, footer — recipes + tokens | `NerdPlayBookLayoutKits` + 4 DNF layout recipes i preset | DT-FEATURE-16 |
| HR-017 | P2 | done | Token Search & Favorites | Søg, favoritstjerne, senest brugt i catalog | Swatch search + favorite stars + favorites-only filter | DT-FEATURE-17 |
| HR-018 | P3 | done | Brand Health Score | Samlet score: kontrast, naming, recipe coverage, unused tokens | `NerdBrandHealthTools` + score panel i catalog | DT-FEATURE-18 |
| HR-019 | P1 | done | Multi-Brand Switcher | Dropdown DNF \| Acme \| Demo \| TNC — regenerer CSS live i Development | Brand dropdown i catalog; `dnf`/`acme`/`demo`/`tnc` presets + live CSS refresh; `NerdTncDesignTokenPresets` | DT-FEATURE-19 |
| HR-020 | P3 | done | Collaborative Comments (Dev-only) | Annotationer på tokens i catalog | `INerdTokenCommentStore` + `FileNerdTokenCommentStore`; comment fields på swatches/tabs (Development only) | DT-FEATURE-20 |

---

## Open — Design Tokens infrastructure (HR-021–HR-025)

| ID | P | Status | Task | DoD (verifiable) | Evidence | Source |
|----|---|--------|------|------------------|----------|--------|
| HR-021 | P1 | done | `NerdTokenPack` JSON DTO + schema | Serialiserbar pack; `token-pack.schema.json`; validation pipeline (navne, hex, WCAG, recipe refs) | DTO, schema, reference validation and accessibility evaluation; 37 unit tests | DT-ROADMAP-F1 |
| HR-022 | P1 | done | Token pack loaders | `FromJson`, `FromPreset("dnf")`, merge med `FromOptions` | `NerdTokenPack` loaders and merge; 37 unit tests | DT-ROADMAP-F1 |
| HR-023 | P1 | done | DNF snapshot tests | bUnit/CSS snapshot for alle 12 DNF tokens + recipes | Deterministisk DNF baseline-test for 12 tokens, recipe og genereret CSS | DT-ROADMAP-F0 |
| HR-024 | P2 | done | Live editor & export (fase 4) | Inline picker + “Promote to preset”; export CSS/JSON/Stitch/Figma; import JSON pack | Studio picker + promote/download preset JSON + import pack; export-knapper i catalog | DT-ROADMAP-F4 |
| HR-025 | P2 | done | Dark mode recipe variants | Recipes med light/dark varianter i generator + catalog | `[data-theme="dark"]` recipe CSS i generator; recipe preview respekterer preview dark toggle | DT-ROADMAP-F6 |

---

## Open — Responsive Typography (HR-030–HR-040)

| ID | P | Status | Task | DoD (verifiable) | Evidence | Source |
|----|---|--------|------|------------------|----------|--------|
| HR-032 | P2 | done | Type Scale Curve | Graf H1→Caption ved viewport + overlay | Catalog curve med viewport-slider og role-bars | RT-FEATURE-03 |
| HR-033 | P1 | done | Save as Client Typography Pack | `INerdTypographyPackStore` + JSON; “Gem som klient” i catalog | Save/load/list i catalog; auto DI for typography store | RT-FEATURE-04 · RT-ROADMAP-F2 |
| HR-034 | P2 | done | Modular Scale Generator | Input base + ratio → generér alle roller + clamp wrap | Catalog UI med preview-tabel og “Apply to preview” | RT-FEATURE-05 |
| HR-035 | P1 | done | Live Clamp Editor | Sliders min/preferred/max + live MudText preview i catalog | Role-select, clamp parse/apply, live preview | RT-FEATURE-06 |
| HR-036 | P2 | done | Accessibility Storyboard | Resize 200%, min 16px body, line-height — pass/fail pr. role i UI | Catalog chips + accessibility result coverage test for configured roles | RT-FEATURE-07 |
| HR-038 | P2 | done | Brand Pack Bundle | Én JSON/zip: colors + typography + recipes | `NerdBrandPack` + `NerdBrandPackTools` JSON/ZIP; `NerdBrandPackExporter` i hub/catalog/demo | RT-FEATURE-09 |
| HR-039 | P3 | done | Figma / Tokens Studio Sync (typography) | Eksport/import font-size tokens | `NerdTypographyTools` export/import + catalog export/import knapper | RT-FEATURE-10 |
| HR-040 | P1 | done | Typography snapshot tests | bUnit: computed sizes ved 320/768/1280/1920 for presets | Deterministiske viewport-tests for H1 og Body1 ved 320/768/1280/1920 | RT-ROADMAP-F0 |

---

## Open — Cross-cutting (HR-050–HR-054)

| ID | P | Status | Task | DoD (verifiable) | Evidence | Source |
|----|---|--------|------|------------------|----------|--------|
| HR-050 | P1 | done | PlayBook typography panel | Typo-scale preview i PlayBook med aktive packs | `NerdPlayBookTypographyPanel` med role/clamp/computed table + viewport slider | RT-ROADMAP-F4 |
| HR-051 | P1 | done | Hub dynamic token/typo counts | Design System Hub viser “Tokens (N)” + link til aktivt pack | `NerdDesignSystemHub` viser counts + aktivt pack chip | DT-ROADMAP-F3 |
| HR-052 | P2 | done | Demo design-system recipes from DI | `/design-system` læser recipes fra DI, ikke hardcode | `DesignSystemDemo.razor` itererer `TokenOptions.Colors` + `Recipes` | DT-ROADMAP-F3 |
| HR-053 | P2 | done | Editorial / Dashboard typography presets | `NerdTypographyPresets` udvidet med 2 nye presets | `ApplyEditorial` og `ApplyDashboard` med tests | RT-ROADMAP-F3 |

---

## Partial

| ID | Status | Task | Remaining |
|----|--------|------|-----------|
| | | | |

---

## Parked

| ID | Status | Task |
|----|--------|------|
| | | |

---

## Shipped (done)

| ID | Status | Task | Evidence |
|----|--------|------|----------|
| HR-065 | done | Split catalog: Colors page | `/nerd-design-tokens` swatches, aliases, export, pack tools only |
| HR-066 | done | Recipes catalog page | `/nerd-design-token-recipes` gallery, studio, composer, matrix, brand health |
| HR-067 | done | Hub + nav dual links | Colors + Recipes buttons in hub and NavMenu |
| HR-068 | done | DNF approved pairings grid | `NerdDnfPairingTools.GetAllApprovedPairings()` — 12 PDF combinations |
| HR-014 | done | Copy Snippets (Design Tokens) | Catalog exposes class, Razor and CSS copy buttons; DesignTokens test suite 39/39 |
| HR-030 | done | Device Frame Studio | Catalog supports editable sample text across phone/tablet/desktop frames; ResponsiveTypography test suite 30/30 |
| HR-037 | done | Copy Snippets (Typography) | Catalog exposes CSS, Razor and C# options snippets; ResponsiveTypography test suite 30/30 |
| HR-031 | done | Breakpoint Comparison Table | `NerdResponsiveTypographyCatalog.razor` MudTable; commit d56080c |
| HR-060 | done | Design Tokens strategy/roadmap/features docs | `src/.../DesignTokens/docs/` |
| HR-061 | done | Responsive Typography strategy/roadmap/features/docs | `src/.../ResponsiveTypography/docs/` |
| HR-062 | done | DNF presets + recipes (baseline) | `NerdDnfDesignTokenPresets`, `NerdDesignTokenRecipe` |
| HR-063 | done | Responsive catalog device frames (baseline) | `NerdResponsiveTypographyCatalog` device frames |
| HR-054 | done | Fix Demo `AddAdditionalAssemblies` dedupe | Demo smoke test via HTTP launch profile starter uden “Assembly already defined” |
| HR-064 | done | Token dogfooding i design-system UI | Semantiske aliases (`muted-content`, `primary-action`, …); ingen `Color="Color.*"` i catalogs/hub/PlayBook/demo; alias-CSS genererer fulde MudBlazor-regler | `NerdDesignSystemUi`, `MudBlazorDesignTokenCssGenerator` alias fix |
| HR-006 | done | Portal-aware Pickers | `nerd-shared.js` portal token observer; Playwright `portal-pickers.spec.ts` + `catalog-helpers.ts` |

---

## Open — Brand packages (HR-070–HR-077)

| ID | P | Status | Task | DoD (verifiable) | Evidence | Source |
|----|---|--------|------|------------------|----------|--------|
| HR-070 | P0 | done | Brand pack extension points | `INerdBrandPack`, `INerdPairingPolicy`, `NerdBrandPackRegistry`; `PairingPolicy` på options | `INerdBrandPack.cs`, `NerdBrandPackRegistry.cs`, `NerdBrandPackServiceCollectionExtensions.cs` | [00-brand-packages-plan.md](00-backlogs/00-brand-packages-plan.md) |
| HR-071 | P0 | done | Extract `Brand.Dnf` | DNF presets + pairing i egen pakke; `AddNerdDnfBrand()` | `TheNerdCollective.Brand.Dnf` | Plan fase 2 |
| HR-072 | P1 | done | Extract `Brand.Tnc/Acme/Demo` | Tre sample-brands i egne pakker | `Brand.Tnc`, `Brand.Acme`, `Brand.Demo` | Plan fase 2 |
| HR-073 | P1 | done | Decouple catalogs from hardcoded brands | Brand dropdown fra `IEnumerable<INerdBrandPack>` | `InstalledBrandPacks` i catalogs | Plan fase 3 |
| HR-074 | P1 | done | Pairing via policy (not DNF static) | `NerdTokenPairingTools` bruger `options.PairingPolicy` | `NerdDnfPairingPolicy`, recipes catalog | Plan fase 3 |
| HR-075 | P1 | done | Demo + test wiring | Demo registrerer alle brands; tests init registry | `Program.cs`, `NerdBrandPackTestBootstrap` | Plan fase 4 |
| HR-076 | P2 | done | Consumer install guide | README per brand + `docs/BRAND-PACKAGES.md` med dependency diagram | `docs/BRAND-PACKAGES.md`, `Brand.*/README.md` | Plan fase 5 |
| HR-077 | P3 | done | Split `DesignTokens.Catalog` | Catalog UI i separat valgfri pakke | `TheNerdCollective.MudComponents.DesignTokens.Catalog`, `AddNerdDesignTokenCatalog()` | Plan later |

---

## Open — Design system follow-up (HR-078–HR-081)

| ID | P | Status | Task | DoD (verifiable) | Evidence | Source |
|----|---|--------|------|------------------|----------|--------|
| HR-078 | P2 | done | Split `ResponsiveTypography.Catalog` | Typography catalog UI i separat valgfri pakke | `ResponsiveTypography.Catalog`, `AddNerdResponsiveTypographyCatalog()` | Follow-up |
| HR-079 | P2 | done | DNF typography in `Brand.Dnf` | `NerdDnfTypographyPresets`, `AddNerdDnfTypography()`, `AddNerdDnfDesignSystem()` in `Brand.Dnf` | `TheNerdCollective.Brand.Dnf` | [00-brand-packages-plan.md](00-backlogs/00-brand-packages-plan.md) |
| HR-080 | P2 | done | Pairing policies for TNC/Acme/Demo | `NerdTncPairingPolicy`, `NerdAcmePairingPolicy`, `NerdDemoPairingPolicy` wired on brand packs | Brand.Tnc/Acme/Demo + pairing tests | Follow-up |
| HR-081 | P3 | done | Sync ROADMAP + portfolio docs | Roadmaps markerer shipped features; portfolio inkl. brand follow-up | `docs/ROADMAP.md`, `00-backlog-portfolio.md` | Follow-up |

---

## Open — Brand identity polish (HR-082)

| ID | P | Status | Task | DoD (verifiable) | Evidence | Source |
|----|---|--------|------|------------------|----------|--------|
| HR-082 | P3 | done | Brand identity version hub + token-pack roundtrip | `ToOptions`/`ApplyTo` bevarer `ActiveBrandIdentityVersion`; hub chip viser version; catalog sync ved brand-skift | `NerdTokenPack`, `NerdDesignSystemOptions`, catalog + tests | Follow-up |
| HR-083 | P2 | done | Brand typography follows token brand switch | `INerdBrandTypographyPack`, registry + catalog switcher; per-brand presets in `Brand.*` | `NerdBrandTypographyRegistry`, catalog + Demo | Follow-up |

---

## Open — JSON-first brand UX (HR-084–HR-092)

| ID | P | Status | Task | DoD (verifiable) | Evidence | Source |
|----|---|--------|------|------------------|----------|--------|
| HR-084 | P0 | done | Token-pack schema v2 | Schema + DTO: `brandId`, `displayName`, color `description`/`roles`, recipe `label`/`usage`, `pairingGuideName`, `approvedPairings`, `lockedTokens` | `token-pack.schema.json`, model types, validation tests | Brand UX spec |
| HR-085 | P0 | done | JSON pairing policy | `NerdJsonPairingPolicy` hydrates `INerdPairingPolicy` from pack; `ApplyTo` wires policy | Studio/recipes use JSON pairings without C# policy | Brand UX spec |
| HR-086 | P1 | done | Reference brand JSON packs | `reference/brands/*.token-pack.json` for TNC/DNF/Acme/Demo; roundtrip tests match presets | Embedded loader + snapshot tests | Brand UX spec |
| HR-087 | P1 | done | Embedded brand pack loader | `NerdEmbeddedBrandPack` + `INerdBrandPack` from JSON resource; registry registers alongside C# packs | Brand packages load JSON as source of truth | Brand UX spec |
| HR-088 | P1 | done | Import brand pack onboarding | Hub + colors: drag-drop JSON, schema validation feedback, apply + refresh CSS | Workbook + colors catalog import; error messages | Brand UX spec |
| HR-089 | P2 | done | Brand workbook wizard | `/nerd-brand-workbook` stepper: palette → aliases → recipes → pairings → export | MudStepper + links from hub | Brand UX spec |
| HR-090 | P2 | done | Pairing preview templates | Hero / card / form-strip templates using `NerdPairingSurface` + recipes | Templates in workbook + recipes catalog | Brand UX spec |
| HR-091 | P2 | done | Governance: locked tokens | `lockedTokens` in pack; studio blocks edit + UI badge | Catalog respects lock list | Brand UX spec |
| HR-092 | P3 | done | Docs sync | `FEATURES.md`, `BRAND-PACKAGES.md`, portfolio metrics for JSON-first | Docs updated | Brand UX spec |

---

## Open — Brand UX hardening (HR-093–HR-097)

| ID | P | Status | Task | DoD (verifiable) | Evidence | Source |
|----|---|--------|------|------------------|----------|--------|
| HR-093 | P1 | done | Brand workbook inline editor | Workbook steps redigerer palette/aliases/recipes; ændringer ApplyTo + CSS refresh; locked tokens respekteres | `NerdBrandWorkbook` editors + remove APIs på options | Brand UX hardening |
| HR-094 | P1 | done | Hub drag-drop import | `INerdBrandPackImportSink` + upload på hub; validering → apply; status chip | `NerdDesignSystemHub`, `NerdBrandPackImportSink` | HR-088 gap |
| HR-095 | P2 | done | Design-system E2E matrix | `design-system-all-brands.spec.ts` dækker hub/workbook navigation; stabilere recipes/hub assertions | Playwright helpers opdateret | PORT-062 follow-up |
| HR-096 | P2 | done | JSON Schema validation | Import + tests validerer mod schema; reference packs passer | `NerdTokenPackSchemaValidator`, `NerdTokenPackSchemaValidatorTests` | Brand UX hardening |
| HR-097 | P3 | done | Recipe metadata + pairings step | Reference JSON har `label`/`usage`; workbook pairings step med add/remove grid | `EnrichRecipeMetadata`, workbook step 4 | HR-089 gap |

---

## Open — Mud Token Studio (Tokens Studio-inspired, HR-098–HR-110)

**Product note:** Vi bygger en **MudBlazor-native token studio** — tokens + recipes er kernen. Direkte Figma-integration er **ikke** et mål; Tokens Studio er inspirationskilde for UX og token-modellering, ikke målplatform.

| ID | P | Status | Task | DoD (verifiable) | Evidence | Source |
|----|---|--------|------|------------------|----------|--------|
| HR-098 | P1 | open | Full token taxonomy (tree) | Udvid pack/schema + `NerdDesignTokenOptions` med **spacing**, **motion** (duration/easing), **breakpoints**, **z-index**, **border-width**; hierarkisk token-træ i catalog/workbook | | Mud Studio vision |
| HR-099 | P2 | open | Token tree navigator | Sidebar: grupper (color · spacing · typography · motion · recipes) med expand/collapse, søg og “jump to token” — Tokens Studio-style | | Mud Studio vision |
| HR-100 | P2 | open | Spacing scale generator | Base unit + ratio → spacing-skala; live preview på MudStack/MudGrid gap og `pa-*` | | Tokens Studio-inspired |
| HR-101 | P2 | open | Breakpoint tokens | Navngivne breakpoints (xs–xl) som tokens; kobling til responsive typography og MudBlazor `Breakpoint` | | Tokens Studio-inspired |
| HR-102 | P3 | open | Motion tokens | Duration + easing tokens; preview på transitions (collapse, progress, snackbar timing) | | Tokens Studio-inspired |
| HR-103 | P2 | open | Token transforms | Reference-baserede transforms (lighten, darken, alpha, math) uden Figma — genererer afledte tokens i pack | | Tokens Studio-inspired |
| HR-104 | P2 | open | Theme sets (light/dark) | First-class **token sets** pr. mode i pack + tree UI; ud over nuværende dual swatch preview | | Tokens Studio-inspired |
| HR-105 | P1 | open | Recipe semantic layer i træet | Recipes som top-level “semantic tokens” (surface + content + action); link til PlayBook layout kits og Mud patterns | | Product focus |
| HR-106 | P2 | open | DTCG / W3C Design Tokens format | Import/export [Design Tokens Format](https://design-tokens.github.io/community-group/format/) ved siden af Tokens Studio JSON | | Tokens Studio-inspired |
| HR-107 | P3 | open | Token documentation panel | Per-token `description`, usage, do/don’t i tree + PlayBook tooltip/chip | | Tokens Studio-inspired |
| HR-108 | P3 | open | Naming & structure linter | Catalog/CI advarsler for navngivning, duplikater, ubrugte tokens, manglende recipe coverage | | Tokens Studio-inspired |
| HR-109 | P2 | open | Composite / alias chains i UI | Visuel kæde `primary-action → himmel → #hex` med edit på alias-niveau i træet | | Tokens Studio-inspired |
| HR-110 | P3 | open | Multi-export pipeline | Én “Export pack” dialog: CSS, JSON, DTCG, Tokens Studio, Stitch — filtreret pr. token-gruppe | | Tokens Studio-inspired |

---

## Commercial backlog (private repo only)

Licence gate, pricing, hosted Studio, multi-framework adapters (**HR-111–HR-118**) trackes i [TheNerdCollective.TokenStudio](https://github.com/janhjordie/token-studio) — se `docs/MUD-TOKEN-STUDIO-COMMERCIAL.md`.

---

| Status | Count |
|--------|------:|
| **open** | **13** |
| **in_progress** | **0** |
| **partial** | **0** |
| **done** | **76** |
| **parked** | **0** |
| **Total** | **89** |

### Recommended next slices (MVP)

**Product (OSS):**
1. **HR-098** — Full token taxonomy
2. **HR-105** — Recipes som semantic layer
3. **HR-099** — Token tree navigator

### Agent commands

```bash
# Næste 5 slices (fra nerd-rules install)
bash "$HOME/.nerd-rules/scripts/backlog-next.sh" docs/BACKLOG.md --count 5
```

```text
Implement HR-006 only from docs/BACKLOG.md. Backlog runner rules. BACKLOG REPORT.
```
