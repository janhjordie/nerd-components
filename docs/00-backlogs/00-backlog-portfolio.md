---
title: "Backlog Portfolio — Design System"
status: Active
author: "@janhjordie"
last_updated: "18-07-2026 22:30"
---

# Backlog Portfolio — Design System

Control board for **Design Tokens** og **Responsive Typography** i TheNerdCollective.Components.  
Master register: [../BACKLOG.md](../BACKLOG.md)

---

## Control Board

| Stream | Epic focus | Open | Partial | Done | Next slice |
|--------|------------|-----:|--------:|-----:|------------|
| Design Tokens | Fleksible brand-packs + UX catalog | 0 | 0 | 37 | — |
| Responsive Typography | Fluid type + client packs + catalog split | 0 | 0 | 10 | — |
| Brand packages | Per-brand NuGet + pairing + DNF typography | 0 | 0 | 12 | — |
| Cross-cutting | Hub, PlayBook, Demo, docs | 0 | 0 | 15 | — |
| **Mud Token Studio** | Tokens Studio-inspired UX uden Figma — taxonomy + recipes | **13** | 0 | 0 | **HR-098** taxonomy |

---

## Portfolio Metrics

| Metric | Target | Current |
|--------|--------|---------|
| Features fra package docs i backlog | 30/30 | 30/30 |
| P0 slices remaining | 0 | 0 |
| P1 slices open | — | **0** |
| Client pack (colors + type) | Shippable | **Yes** — `NerdBrandPack` JSON/ZIP export |
| Catalog packages split (motor vs dev UI) | Both split | **Yes** — `DesignTokens.Catalog` + `ResponsiveTypography.Catalog` |
| Brand pairing policies | All sample brands | **Yes** — DNF, TNC, Acme, Demo |
| Demo starts cleanly | Yes | **Yes** (HR-054) |
| Token dogfooding in catalogs | Yes | **Yes** (HR-064) |
| JSON-first brand packs (schema v2) | Yes | **Yes** (HR-084–092) |
| Brand workbook full editor | Agency-ready | **Yes** (HR-093) |
| Design-system E2E (all brands) | CI green | **Yes** (HR-095) |
| JSON schema validation on import | Yes | **Yes** (HR-096) |

---

## Dependency Map

```text
HR-021 NerdTokenPack DTO
  └── HR-002 Save as Client Pack (tokens)
        └── HR-019 Multi-Brand Switcher
        └── HR-010 Token Diff

HR-033 Typography Pack
  └── HR-038 Brand Pack Bundle (colors + type) ✓
  └── HR-079 DNF typography in Brand.Dnf ✓

HR-077 DesignTokens.Catalog split ✓
HR-078 ResponsiveTypography.Catalog split ✓

HR-080 Pairing policies (TNC/Acme/Demo) ✓

HR-006 Portal-aware Pickers ✓
  └── HR-016 Layout Kits (trustworthy PlayBook)

HR-054 Demo dedupe ✓
  └── HR-095 Design-system E2E matrix (4×5 brands/pages)

HR-084–092 JSON-first brand UX ✓
  └── HR-093 Workbook inline editor
  └── HR-094 Hub import
  └── HR-096 Schema CI validation
  └── HR-097 Recipe metadata + pairings step
```

---

## Decision Log

| Date | ID | Decision |
|------|-----|----------|
| 18-07-2026 | PORT-001 | Master backlog i `docs/BACKLOG.md` med `HR-###` IDs per nerd-rules host template |
| 18-07-2026 | PORT-002 | Features importeret fra package `docs/FEATURES.md` + roadmap stabilisering |
| 18-07-2026 | PORT-003 | HR-030/HR-031 baseline markeret done efter catalog-forbedring |
| 18-07-2026 | PORT-004 | HR-014 og HR-037 copy-snippets markeret done efter class/Razor/CSS/options-flow |
| 18-07-2026 | PORT-005 | HR-006 portal-scope: E2E opt-in pga. Blazor Server/MudTabs flakiness |
| 18-07-2026 | PORT-009 | HR-006 done: `nerd-shared.js` attribute observer + Playwright `portal-pickers.spec.ts` |
| 18-07-2026 | PORT-011 | Brand packages: `Brand.Dnf/Tnc/Acme/Demo` + `docs/BRAND-PACKAGES.md` |
| 18-07-2026 | PORT-006 | HR-064: semantiske UI-aliases + fuld alias-CSS; ingen `Color="Color.*"` i design-system UI |
| 18-07-2026 | PORT-008 | HR-015/018/020/039 shipped: opacity overlays, brand health, dev comments, typography Tokens Studio |
| 18-07-2026 | PORT-014 | HR-093–097: workbook editor, hub import, schema validation, pairings step, E2E hardening |
| 18-07-2026 | PORT-015 | **Mud Token Studio** retning: fuld token-taxonomi + Tokens Studio-inspireret UX; **tokens + recipes** er kernen — direkte Figma-integration er ikke mål (HR-098–110) |
| 18-07-2026 | PORT-016 | Commercial MVP flyttet til privat [token-studio](https://github.com/janhjordie/token-studio) — Free/Pro/Agency, licence gate, multi-framework adapters (HR-111–118) |

---

## Source documents

| Package | Strategy | Roadmap | Features |
|---------|----------|---------|----------|
| Design Tokens | [STRATEGY.md](../src/TheNerdCollective.MudComponents.DesignTokens/docs/STRATEGY.md) | [ROADMAP.md](../src/TheNerdCollective.MudComponents.DesignTokens/docs/ROADMAP.md) | [FEATURES.md](../src/TheNerdCollective.MudComponents.DesignTokens/docs/FEATURES.md) |
| Responsive Typography | [STRATEGY.md](../src/TheNerdCollective.MudComponents.ResponsiveTypography/docs/STRATEGY.md) | [ROADMAP.md](../src/TheNerdCollective.MudComponents.ResponsiveTypography/docs/ROADMAP.md) | [FEATURES.md](../src/TheNerdCollective.MudComponents.ResponsiveTypography/docs/FEATURES.md) |

---

## Now / Next / Later

### Now (P0–P1, unblockers)

- **HR-098** — Full token taxonomy
- **HR-105** — Recipes som semantic layer

### Next (P2)

- **HR-099** Token tree navigator
- **HR-100–104, HR-106, HR-109–110** (taxonomy & export slices)

### Later (P3)

- **HR-102, HR-107–108**

### Commercial (private [token-studio](https://github.com/janhjordie/token-studio))

HR-111–HR-118 (licence, Docker, adapters) — se `docs/MUD-TOKEN-STUDIO-COMMERCIAL.md` i privat repo.
