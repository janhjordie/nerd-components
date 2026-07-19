---
title: "TheNerdCollective.Components — Master Backlog"
status: Active
author: "@janhjordie"
last_updated: "19-07-2026 19.00"
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
| HR-098 | P1 | done | Full token taxonomy (tree) | Breakpoints, motion durations/easings, z-index i pack/schema/CSS + token tree | `NerdFoundationTaxonomyTools`, `token-pack.schema.json` | Mud Studio vision |
| HR-099 | P2 | done | Token tree navigator | Sidebar i colors catalog: grupper (color · spacing · aliases · recipes · theme sets), søg og jump-to-token | `NerdTokenTreeNavigator`, `token-tree-navigator.spec.ts` | Mud Studio vision |
| HR-100 | P2 | done | Spacing scale generator | Workbook step: base unit + ratio (linear/geometric); `GenerateScale` + CSS utilities | `NerdSpacingScaleTools`, workbook spacing step | Tokens Studio-inspired |
| HR-101 | P2 | done | Breakpoint tokens | xs–xl tokens koblet til typography catalog + MudBlazor `Breakpoint` map | `NerdBreakpointTools`, typography breakpoint chips | Tokens Studio-inspired |
| HR-102 | P3 | done | Motion tokens | Duration/easing preview (collapse, progress, snackbar) i colors catalog | `NerdMotionPreviewPanel` | Tokens Studio-inspired |
| HR-103 | P2 | done | Token transforms | lighten/darken/alpha transforms i pack/schema; derived colors on pack load; workbook editor step | `NerdTokenTransformTools`, workbook transforms step, `transforms` i schema | Tokens Studio-inspired |
| HR-104 | P2 | done | Theme sets (light/dark) | `themeSets` i pack/schema; CSS merge via `SyncColorTokensFromThemeSets`; catalog + workbook editor | `NerdThemeSetTools`, workbook theme step | Tokens Studio-inspired |
| HR-105 | P1 | done | Recipe semantic layer i træet | Recipes med surface/content/action children; PlayBook layout-kit anchors; tree navigation | `NerdRecipePlayBookLinks`, `NerdTokenTreeTools` | Product focus |
| HR-106 | P2 | done | DTCG / W3C Design Tokens format | Export/import DTCG JSON for colors/spacing/motion/breakpoints/z-index; catalog importer + filtered export | `NerdDtcgTokenTools`, `NerdDtcgImporter` | Tokens Studio-inspired |
| HR-107 | P3 | done | Token documentation panel | Per-token `description`, usage, do/don’t i tree + catalog panel; swatch tooltip | `NerdTokenDocumentationTools`, `NerdTokenDocumentationPanel` | Tokens Studio-inspired |
| HR-108 | P3 | done | Naming & structure linter | Catalog advarsler for navngivning, duplikater, ubrugte tokens, manglende recipe coverage | `NerdTokenStructureLinterTools`, `NerdTokenStructureLinterPanel` | Tokens Studio-inspired |
| HR-109 | P2 | done | Composite / alias chains i UI | Visuel alias-kæde i tree + `NerdAliasChainPanel` i catalog | `NerdAliasChainTools` | Tokens Studio-inspired |
| HR-110 | P3 | done | Multi-export pipeline | Én “Export pack” dialog: CSS, JSON, DTCG, Tokens Studio, Stitch — filtreret pr. token-gruppe | `NerdTokenPackExportTools`, `NerdTokenPackExportPanel` | Tokens Studio-inspired |

---

## Open — Cross-framework intents & shell (HR-119–HR-124)

**Strategi:** [STRATEGY.md](../src/TheNerdCollective.MudComponents.DesignTokens/docs/STRATEGY.md) §7–10.  
**Referencebrand:** DNF designmanual 2025 (`Danmarks Naturfredningsforening.pdf`) — hero, nav, footer, CTA, formular-variationer.

| ID | P | Status | Task | DoD (verifiable) | Evidence | Source |
|----|---|--------|------|------------------|----------|--------|
| HR-119 | P0 | done | Component intent vocabulary (core) | Udvid `NerdDesignSystemUi` + pack aliases: `nav-surface`, `nav-item`, `nav-item-active`, `input-surface`, `input-border`, `focus-ring`, `secondary-action`; dokumenteret i STRATEGY; WCAG på defaults | `NerdDesignSystemUi`, TNC/DNF packs |
| HR-120 | P0 | done | Sidebar shell recipe | `sidebar` recipe i DNF/TNC JSON + presets; `dnf-recipe-sidebar` / `tnc-recipe-sidebar` CSS; workbook + layout kits | `NerdDnfDesignTokenPresets`, `NerdTncDesignTokenPresets` |
| HR-121 | P1 | done | Mud nav-link rules i recipe scope | `AppendRecipeNavigationRules` i generator; unit + Playwright | `MudBlazorDesignTokenCssGenerator` |
| HR-122 | P1 | done | `shell` + `frameworkDefaults` i pack schema | Schema v3 felter; `frameworkDefaults.mudblazor` mapper intents → komponenter; validering i CI | `NerdTokenPackShell`, `NerdFrameworkDefaults`, JSON packs | STRATEGY §9 |
| HR-123 | P2 | done | DNF manual layout kit udvidelse | PlayBook layout kits: `hero-photo`, `hero-organic`, `hero-light`, `footer-minimal`, `feature-panel`, `partner-row`, `formular` fra PDF/skærmbilleder | `NerdPlayBookLayoutKits`, `docs/DNF-DESIGN-GUIDE.md` |
| HR-124 | P2 | done | `NerdAppShell` consumer | Valgfri layout-komponent læser `shell` + `frameworkDefaults` fra aktivt pack; demo + Token Studio Host | `NerdAppShell.razor`, Host `MainLayout` | STRATEGY §9 |

## Open — Design guide & handoff (HR-125–HR-129)

| ID | P | Status | Task | DoD | Evidence |
|----|---|--------|------|-----|----------|
| HR-125 | P1 | done | Live design guide route | `/nerd-design-guide` med palette, par, intents, scenes, parity; brand switcher | `NerdDesignGuide`, `NerdDesignGuidePage` |
| HR-126 | P2 | done | Design-to-code parity score | `NerdDesignParityTools` — shell, recipes, pairings, framework defaults | Design guide parity panel |
| HR-127 | P2 | done | PlayBook intent inspector | Klik preview → intent/token/framework default | `NerdPlayBookIntentInspector` |
| HR-128 | P2 | done | Recipe layout scenes | Storybook-style hero/nav/footer scene fra pack | `NerdDesignGuideScenes` |
| HR-129 | P2 | done | Handoff ZIP export | token-pack + CSS + JSON + DESIGN.md + WCAG report + README | `NerdDesignHandoffTools`, `NerdDesignHandoffExporter` |

## Open — MudBlazor CSS harvest & state fidelity (HR-130–HR-138)

**Analyse (Token Studio):** `docs/01-mudblazor-css-generator-analysis.md` i nerd-token-studio.  
**Problem:** Blanket `!important` + forkerte selektorer flader MudTabs/MudSwitch state; catalog chrome/toolbar er plaster.

| ID | P | Status | Task | DoD (verifiable) | Evidence | Source |
|----|---|--------|------|------------------|----------|--------|
| HR-130 | P0 | done | Mud version-pin + **gemt CSS-analysearkiv** | Archive + coverage; wave 14 (flexbreak, internal docs SCSS, harvest CI gate) | `9.7.0/` 74× inventory YAML, `ValidateHarvestCoverage`, `layout-utilities-dogfood.spec.ts` | CSS-GENERATOR-ANALYSIS §5.4 |
| HR-131 | P0 | done | MudTabs state-bridge | Fjern `mud-tab` fra AccentTextPatterns; explicit inactive / `.mud-tab-active` / `.mud-tab-slider`; unit + Playwright: active ≠ inactive under token scope | `MudBlazorComponentStateBridgeTests`, `mud-state-dogfood.spec.ts` | CSS-GENERATOR-ANALYSIS |
| HR-132 | P0 | done | MudSwitch (+ checkbox/radio) state-bridge | Korrekt `.mud-switch-base.mud-checked + .mud-switch-track`; thumb kun via `.mud-switch-thumb-*`; track mix mod `transparent` (ikke accent-surface når identisk); wrapper `tnc-primary-action` når Mud ignorerer `Class` | `MudBlazorComponentStateBridgeTests`, `mud-state-dogfood.spec.ts`, `catalog-chrome-dogfood.spec.ts` | CSS-GENERATOR-ANALYSIS |
| HR-133 | P1 | done | SCSS harvest pipeline | Agent/script + validator; 74× inventory YAML (wave 1–14); `ValidateHarvestCoverage` CI gate | wave 14: `_flexbreak.yaml`, internal docs YAML, `layout-utilities-dogfood.spec.ts` | CSS-GENERATOR-ANALYSIS |
| HR-134 | P1 | done | Generated rule tables | `NerdMudInventoryRuleTable` validerer alle 20 inventory YAML mod CSS (TNC+DNF); harvest script CI gate + `generated-rule-table.md` | `harvest-mudblazor-inventory.sh`, `NerdMudInventoryRuleTableTests` | CSS-GENERATOR-ANALYSIS · HR-114 |
| HR-135 | P2 | done | Retire catalog switch/tab band-aids | Fjernet tab/switch state fra catalog chrome/toolbar CSS; placement markers + label readability; style guard skipper switch-kontrast i `catalog-toolbar` (kun label-tekst) | `NerdMudInventoryRuleTableTests`, `MudBlazorComponentStateBridgeTests`, `catalog-chrome-dogfood.spec.ts` | CSS-GENERATOR-ANALYSIS |
| HR-136 | P2 | done | Full 66-component coverage matrix | 72 SCSS-filer klassificeret P0–P4 i `coverage-matrix.md`; harvest script lister upstream | `coverage-matrix.md`, `sources/COMPONENTS.md` | CSS-GENERATOR-ANALYSIS |
| HR-137 | P1 | done | Picker/portal composite harvest | Date/Time/Color/Select portal: state rules, inventory, Playwright dogfood (TNC/DNF) | `AppendPickerPortalStateRules`, `*-portal-dogfood.spec.ts`, catalog `time-picker-*`/`color-picker-*` testids | CSS-GENERATOR-ANALYSIS §5.5 |
| HR-138 | P1 | done | Component-family intent map (Core) | Framework-neutrale `reference/component-families/*.yaml` (picker.day-selected → primary-action); bruges af Mud nu og Fluent/Blazorise senere | `reference/component-families/picker.yaml` | CSS-GENERATOR-ANALYSIS §5.6 · HR-114 |

## Open — MudBlazor 100% fidelity / palette-first reskin (HR-139–HR-147)

**Analyse (Token Studio):** [02-mudblazor-100-percent-fidelity.md](https://github.com/janhjordie/nerd-token-studio/blob/main/docs/02-mudblazor-100-percent-fidelity.md) · companion [01-mudblazor-css-generator-analysis.md](https://github.com/janhjordie/nerd-token-studio/blob/main/docs/01-mudblazor-css-generator-analysis.md).

| ID | P | Status | Task | DoD (verifiable) | Evidence | Source |
|----|---|--------|------|------------------|----------|--------|
| HR-139 | P0 | done | Mud palette manifest v9.7.0 | Alle `--mud-palette-*` fra `MudThemeProvider` dokumenteret + C# catalog | `reference/mudblazor/9.7.0/PALETTE-MANIFEST.md`, `MudBlazorPaletteManifest.cs` | 02-FIDELITY §2 |
| HR-140 | P0 | done | Brand-root palette generator | `.{prefix}-mud-brand` + `.mud-theme-provider` emitter fuld palette én gang | `MudBlazorBrandPaletteGenerator`, `NerdMudBrandPaletteMap` | 02-FIDELITY §3 |
| HR-141 | P0 | done | MudThemeProvider sync | Host `MudTheme` bygges fra samme map som CSS; `NerdAppShell` brand root class | `NerdMudThemeFactory`, `MainLayout.razor`, `NerdAppShell.razor` | 02-FIDELITY §3 |
| HR-142 | P0 | done | Intent-scoped palette overrides | Action + surface intents overskriver korrekte palette slots; bridges-only på action aliases | `AppendIntentPaletteOverrides`, `AppendSurfacePaletteLines`, `bridgesOnly` | 02-FIDELITY §4 |
| HR-143 | P1 | done | Deprecate ComponentRuleBuilder bulk | `bridgesOnly` for alle palette-first semantic aliases; bulk kun raw tokens + legacy | `MudBlazorComponentRuleBuilder`, `IsContentIntentAlias` | 02-FIDELITY §5 |
| HR-144 | P1 | done | Schema `mudPalette` i token pack | `frameworkDefaults.mudblazor.palette` i schema + alle reference packs | `token-pack.schema.json`, `*.token-pack.json` | 02-FIDELITY §5 |
| HR-145 | P1 | done | Playwright full-fidelity suite | Palette root, tab slider, design guide panel, catalog swatch button ≠ Mud purple | `mud-palette-fidelity*.spec.ts`, `NerdMudPaletteParityTools` | 02-FIDELITY §7 |
| HR-146 | P2 | done | Visual regression vs Mud reference | Playwright locator screenshots (TNC/DNF) under `reference/mudblazor/9.7.0/visual/` | `mud-visual-regression.spec.ts` | 02-FIDELITY §7 |
| HR-147 | P2 | done | Mud upgrade playbook | `diff-mudblazor-upgrade.sh` + `upgrades/*.md` ritual | `NerdMudUpgradeDiffTools`, `upgrades/9.7.0-to-9.8.0.md` | 02-FIDELITY §6 |

## Open — Product ideas from Token Studio (HR-148+)

Kilde: nerd-token-studio `docs/ideas.md`.

| ID | P | Status | Task | DoD (verifiable) | Evidence | Source |
|----|---|--------|------|------------------|----------|--------|
| HR-148 | P1 | done | Intent picker workbook step | Workbook “Intents” step: vælg standard intent, live MudButton preview, alias target + apply | `NerdIntentCatalogTools`, `NerdBrandWorkbook` step 3 | ideas #1 |
| HR-149 | P1 | done | DNF manual compliance score | Score 0–100 for shell recipes, approved pairings, hero-overlay tokens; panel i recipes catalog | `NerdManualComplianceTools`, recipes catalog panel | ideas #2 |
| HR-150 | P2 | done | Composition export til Stitch | Stitch/DESIGN.md sektion med shell layout-blokke fra pack recipes | `NerdLayoutCompositionCatalog`, `ExportStitchDesignMd` | ideas #4 |
| HR-151 | P2 | done | Hero overlay opacity i layout kits | `hero-photo` / `hero-organic` bruger `hero-overlay` opacity token når defineret | `NerdPlayBookLayoutKits.razor` | ideas #7 |
| HR-152 | P2 | done | Recipe ≠ komponent onboarding | Dokumentér ~12 shell recipes + ~15 intents + `frameworkDefaults`; onboarding i STRATEGY/quickstart | `docs/RECIPE-INTENT-MODEL.md`, QUICKSTART §6 | ideas #3 |
| HR-153 | P3 | done | Bootstrap Blazor adapter spike | `--bs-*` bridge CSS; PlayBook map + preview; `reference/bootstrap/5.3/` | `NerdBootstrapDesignTokenCssGenerator` | ideas #5 |
| HR-114 | P2 | done | DesignTokens.Core extract | Wave 1–3: Core + `NerdMudHarvestAdapter` + `NerdCoreAssemblyGuardTests`; `CORE-EXTRACT.md` | `DesignTokens.Core` | TS-014 |
| HR-154 | P2 | done | Workbook intent Radzen side-by-side | Intents step: Mud + live `RadzenButton` preview; `Radzen.Blazor` i Catalog | `NerdBrandWorkbook` step 3 | ideas #1 |
| HR-155 | P2 | done | Workbook intent E2E | Host + Demo `workbook-intent-radzen.spec.ts`: Mud + Radzen primary ≠ Mud purple | `tests/e2e/workbook-intent-radzen.spec.ts` | TS-055 |
| HR-156 | P3 | done | HR-114 polish (deferred) | Wave 4 namespace migration dokumenteret som parked i `CORE-EXTRACT.md` | `CORE-EXTRACT.md` §Wave 4 | HR-114 |

## Open — Mud theme + PseudoCss (HR-157–175)

Strategi: **Fase 1** `NerdMudThemeFactory` ejer global `:root` brand → **Fase 2** `PseudoCss.Scope` per intent/recipe → **Fase 3** samlet `NerdMudThemeProvider`.  
Slet overflødig CSS/generator-kode når theme-laget overtager (ingen deprecation — ingen eksterne brugere).

### Fase 1 — Theme Provider First

| ID | P | Status | Task | DoD (verifiable) | Evidence | Source |
|----|---|--------|------|------------------|----------|--------|
| HR-157 | P0 | done | `NerdMudThemeFactory` fuld palette | `PaletteLight` + `PaletteDark` fra pack `themeSets` via `MudColor` (rgb/hover/darken/lighten); parity med `MudBlazorPaletteManifest` | `NerdMudThemeFactory.cs`, `NerdMudBrandPaletteTests` | theme strategy fase 1 |
| HR-158 | P0 | done | Fjern CSS global palette-duplikat | `MudBlazorBrandPaletteGenerator.AppendBrandRootPalette` emitter ikke længere `--mud-palette-*` på `.mud-theme-provider`; `.tnc-mud-brand` er layout-klasse only | `MudBlazorBrandPaletteGenerator.cs` | theme strategy fase 1 |
| HR-159 | P0 | done | Slet ComponentRuleBuilder bulk for intents | Fjern `AccentTextPatterns`/`InputPatterns`/`FilledPatterns` for semantic aliases; behold kun raw color token rules + inventory bridges | `MudBlazorComponentRuleBuilder.cs` | theme strategy fase 1 |
| HR-160 | P1 | done | Slet alias palette-flattening | `MudBlazorPaletteMapper.AppendPaletteVariables` kaldes ikke for aliases; semantic intents bruger ikke per-alias `--mud-palette-*` flattening | `MudBlazorDesignTokenCssGenerator.cs`, `MudBlazorPaletteMapper.cs` | theme strategy fase 1 |
| HR-161 | P1 | done | Minimal Mud bridges CSS | Switch thumb + inventory `generator_required` hardcoded hex i dedikeret bridge (ikke ComponentRuleBuilder bulk) | `reference/mudblazor/9.7.0/inventory/_switch.yaml` | theme strategy fase 1 |
| HR-162 | P1 | done | Theme/CSS parity tests | Unit: factory palette keys = manifest; CSS indeholder ikke duplikat global palette block; `NerdMudPaletteParityTools` fejler ved regression | `NerdMudPaletteParityTools.cs`, tests | theme strategy fase 1 |
| HR-163 | P2 | done | Opdatér fidelity docs fase 1 | `02-mudblazor-100-percent-fidelity.md` §4: global brand = `MudThemeProvider` only; CSS = intents + bridges | `docs/02-mudblazor-100-percent-fidelity.md` (Token Studio) | theme strategy fase 1 |

### Fase 2 — PseudoCss intents + recipes

| ID | P | Status | Task | DoD (verifiable) | Evidence | Source |
|----|---|--------|------|------------------|----------|--------|
| HR-164 | P0 | done | `NerdMudIntentThemeFactory` | Bygger `MudTheme` pr. semantic intent fra pack aliases + `frameworkDefaults.mudblazor.palette` slot mapping | `NerdMudIntentThemeFactory.cs` | theme strategy fase 2 |
| HR-165 | P1 | done | Intent → palette slot map | Dokumenteret + testet mapping: `primary-action` → Primary + action-default; `page-surface` → surface/background/text; `nav-surface` → drawer-*; osv. (~15 intents) | `NerdMudIntentPaletteMap.cs`, unit tests | theme strategy fase 2 |
| HR-166 | P0 | parked | `NerdMudThemeStack` (multi-provider) | **Parked** — flere `MudThemeProvider` risikerer popover-duplikater; brug HR-170 single `GenerateTheme` i stedet | — | theme strategy fase 2 |
| HR-167 | P0 | done | Slet intent palette CSS | Fjern `AppendIntentPaletteOverrides` m.m. når `UseIntentPseudoCssThemes` er true (HR-170 spike) | `MudBlazorBrandPaletteGenerator.cs` | theme strategy fase 2 |
| HR-168 | P1 | done | Recipe PseudoCss themes | `NerdMudRecipeThemeFactory`: shell recipes (`sidebar`, `hero-photo`, …) får `MudTheme` + scope `":root .{prefix}-recipe-{name}"` | `NerdMudRecipeThemeFactory.cs` | theme strategy fase 2 |
| HR-169 | P1 | done | PseudoCss unit tests | Assert scope strings + emitted palette slots pr. intent/recipe; ingen manuel `--mud-palette-*` i intent CSS for covered aliases | `NerdMudIntentThemeFactoryTests.cs` | theme strategy fase 2 |

### Fase 3 — NerdMudThemeProvider + polish

| ID | P | Status | Task | DoD (verifiable) | Evidence | Source |
|----|---|--------|------|------------------|----------|--------|
| HR-170 | P0 | done | `NerdMudThemeProvider` (fase 2 spike) | Arver `MudThemeProvider`; `GenerateTheme` emitter `:root` brand + intent/recipe scopes (`:root .{prefix}-{intent}`) i **ét** `<style>` — erstat HR-166 | `NerdMudThemeProvider.cs` | theme strategy fase 2 |
| HR-171 | P1 | done | Host migration til `NerdMudThemeProvider` | `MainLayout`/`NerdAppShell` bruger én provider; fjern løs `MudThemeProvider` + redundant theme CSS | `NerdAppShell.razor` | theme strategy fase 3 |
| HR-172 | P1 | done | `CurrentPalette` brand switch API | `INerdMudThemeController` opdaterer aktiv palette fra pack; catalogs trigger refresh uden CSS regen | `INerdMudThemeController.cs` | theme strategy fase 3 |
| HR-173 | P2 | done | Typography i `MudTheme` | `NerdMudThemeFactory` mapper `NerdResponsiveTypographyOptions` → `MudTheme.Typography` | `NerdMudThemeFactory.cs` | theme strategy fase 3 |
| HR-174 | P2 | done | Layout + shadows i `MudTheme` | `LayoutProperties` (border-radius, drawer width) + `Shadows` fra token pack | `NerdMudThemeFactory.cs` | theme strategy fase 3 |
| HR-175 | P2 | done | STRATEGY docs PseudoCss model | `STRATEGY.md` §8 opdateret: theme-first + PseudoCss; fjern multi-framework theme refs i Mud-sektion | `docs/STRATEGY.md` | theme strategy fase 3 |

## Open — Cross-framework adapters (HR-115–HR-118)

| ID | P | Status | Task | DoD (verifiable) | Evidence | Source |
|----|---|--------|------|------------------|----------|--------|
| HR-117 | P3 | done | Fluent UI Blazor adapter spike | `--nerd-intent-*` + Fluent `--colorBrand*` bridge CSS; `NerdBrandRootClasses` på shell; `reference/fluent/4.11.0/` | `NerdIntentCssGenerator`, `NerdFluentDesignTokenCssGenerator`, `NerdPlayBookFrameworkBridge` | STRATEGY §7 |
| HR-115 | P2 | done | Radzen adapter spike | `--rz-*` intent bridge CSS + `RadzenComponentRuleBuilder`; `reference/radzen/5.7.0/` | `NerdRadzenDesignTokenCssGenerator`, `RadzenComponentRuleBuilder` | STRATEGY §7 |
| HR-116 | P3 | done | Blazorise adapter spike | Reuses Bootstrap `--bs-*` bridge; `NerdBlazorisePaletteManifest`; PlayBook preview | `NerdBlazoriseDesignTokenCssGenerator`, `reference/blazorise/1.7.3/` | ideas #5 · TS-016 |
| HR-118 | P2 | done | PlayBook framework switcher | Mud + Fluent map + live Radzen `RadzenButton`/`RadzenCard` preview | `NerdPlayBookFrameworkBridge.razor` | TS-018 |

## Docs (OSS + Studio)

| ID | P | Status | Task | DoD | Evidence |
|----|---|--------|------|-----|----------|
| HR-112 | P1 | done | Token Studio quickstart | 15-min guide: clone, tier, first pack | nerd-token-studio `docs/QUICKSTART.md` |
| HR-113 | P2 | done | Docker / hosted Studio | `docker compose up`, `NerdTokenStudio__Tier` env | nerd-token-studio `Dockerfile`, `docker-compose.yml` |

---

## Commercial backlog (private repo only)

Licence gate, pricing, hosted Studio, multi-framework adapters (**HR-111–HR-118**) trackes i [Nerd Token Studio](https://github.com/janhjordie/nerd-token-studio) — se `docs/MUD-TOKEN-STUDIO-COMMERCIAL.md` i det private repo.

---

| Status | Count |
|--------|------:|
| **open** | **18** |
| **in_progress** | **0** |
| **partial** | **0** |
| **done** | **117** |
| **parked** | **1** |
| **Total** | **136** |

### Recommended next slices (MVP)

**Product (OSS):**
1. **HR-157–163** — Fase 1: Theme Provider First; slet CSS palette-duplikat + bulk rules
2. **HR-164–165, HR-167–169** + **HR-170 spike** — Fase 2: PseudoCss via single `GenerateTheme` (HR-166 parked)
3. **HR-171–175** — Fase 3: fuld `NerdMudThemeProvider` + polish
4. **TS-064–072** — Host + E2E (nerd-token-studio backlog)
5. **TS-072** — Luk platform gate; derefter Stripe (private)

Fuld rækkefølge: nerd-token-studio `docs/BACKLOG.md` + `docs/ideas.md`.

### Agent commands

```bash
# Næste 5 slices (fra nerd-rules install)
bash "$HOME/.nerd-rules/scripts/backlog-next.sh" docs/BACKLOG.md --count 5
```

```text
Implement HR-006 only from docs/BACKLOG.md. Backlog runner rules. BACKLOG REPORT.
```
