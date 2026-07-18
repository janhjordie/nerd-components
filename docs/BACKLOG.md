---
title: "TheNerdCollective.Components — Master Backlog"
status: Active
author: "@janhjordie"
last_updated: "18-07-2026 09:42"
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
| HR-001 | P1 | open | Live Token Studio (in-browser) | Color picker + hex/rgb/hsl på `/nerd-design-tokens`; live MudButton/Chip/TextField preview; WCAG-meter opdateres live; undo/redo | | DT-FEATURE-01 |
| HR-002 | P0 | open | Save as Client Pack (Design Tokens) | `INerdTokenPackStore` + JSON DTO; “Gem som klient” i catalog; gem/load under `App_Data/token-packs/`; aktivt pack efter genstart | | DT-FEATURE-02 · ROADMAP-F2 |
| HR-003 | P1 | open | Token Discovery Everywhere | Ny token i pack → swatches i catalog, matrix i PlayBook, badge i Hub, export — uden hardcoded Razor-lister | | DT-FEATURE-03 |
| HR-004 | P1 | open | Recipe Composer | UI: vælg surface + content + action → generér recipe-klasse; preview MudCard + MudButton + MudText | | DT-FEATURE-04 |
| HR-005 | P2 | open | Contrast Pair Matrix | Tabel forgrund × baggrund med ratio + AA/AAA i catalog (DNF-style) | | DT-FEATURE-05 |
| HR-006 | P0 | in_progress | Portal-aware Pickers | Date/Time/Select/Menu-popovers arver token-klasse; Playwright bekræfter `dnf-*` på `.mud-popover-open` | Shared portal bridge + CSS portal-scope + unit tests; E2E-verifikation mangler | DT-FEATURE-06 · ROADMAP-F0 |
| HR-007 | P2 | open | Figma / Tokens Studio Export | Eksport + import Tokens Studio JSON for farver | | DT-FEATURE-07 |
| HR-008 | P2 | open | Stitch / DESIGN.md Sync | Udvid eksport med recipes, radii, shadows, typography hooks | | DT-FEATURE-08 |
| HR-009 | P1 | open | Accessibility Gate in CI | Test eller build-step fejler ved WCAG AA-brud på tokens | | DT-FEATURE-09 |
| HR-010 | P2 | open | Token Diff & Changelog | Diff-view preset vs klient-pack (`+` `~` `-`) i catalog | | DT-FEATURE-10 |
| HR-011 | P2 | open | Semantic Alias Browser | UI: `primary-action → himmel` + “hvor bruges den?” | | DT-FEATURE-11 |
| HR-012 | P1 | open | Dark Mode Dual Preview | Split-view light \| dark for samme token i catalog | | DT-FEATURE-12 |
| HR-013 | P2 | open | Hover / Focus / Disabled Storyboard | State-strip pr. komponent: default → hover → focus → disabled | | DT-FEATURE-13 |
| HR-015 | P3 | open | Watermark / Opacity Tokens | Tokens med opacity til watermark/overlays (DNF-PDF) | | DT-FEATURE-15 |
| HR-016 | P1 | open | Layout Kits (Hero, Nav, Footer) | PlayBook compositions: hero, link card, CTA strip, footer — recipes + tokens | | DT-FEATURE-16 |
| HR-017 | P2 | open | Token Search & Favorites | Søg, favoritstjerne, senest brugt i catalog | | DT-FEATURE-17 |
| HR-018 | P3 | open | Brand Health Score | Samlet score: kontrast, naming, recipe coverage, unused tokens | | DT-FEATURE-18 |
| HR-019 | P1 | open | Multi-Brand Switcher | Dropdown DNF \| Acme \| Demo — regenerer CSS live i Development | | DT-FEATURE-19 |
| HR-020 | P3 | open | Collaborative Comments (Dev-only) | Annotationer på tokens i catalog | | DT-FEATURE-20 |

---

## Open — Design Tokens infrastructure (HR-021–HR-025)

| ID | P | Status | Task | DoD (verifiable) | Evidence | Source |
|----|---|--------|------|------------------|----------|--------|
| HR-021 | P1 | done | `NerdTokenPack` JSON DTO + schema | Serialiserbar pack; `token-pack.schema.json`; validation pipeline (navne, hex, WCAG, recipe refs) | DTO, schema, reference validation and accessibility evaluation; 37 unit tests | DT-ROADMAP-F1 |
| HR-022 | P1 | done | Token pack loaders | `FromJson`, `FromPreset("dnf")`, merge med `FromOptions` | `NerdTokenPack` loaders and merge; 37 unit tests | DT-ROADMAP-F1 |
| HR-023 | P1 | done | DNF snapshot tests | bUnit/CSS snapshot for alle 12 DNF tokens + recipes | Deterministisk DNF baseline-test for 12 tokens, recipe og genereret CSS | DT-ROADMAP-F0 |
| HR-024 | P2 | open | Live editor & export (fase 4) | Inline picker + “Promote to preset”; export CSS/JSON/Stitch/Figma | | DT-ROADMAP-F4 |
| HR-025 | P2 | open | Dark mode recipe variants | Recipes med light/dark varianter i generator + catalog | | DT-ROADMAP-F6 |

---

## Open — Responsive Typography (HR-030–HR-040)

| ID | P | Status | Task | DoD (verifiable) | Evidence | Source |
|----|---|--------|------|------------------|----------|--------|
| HR-032 | P2 | open | Type Scale Curve | Graf H1→Caption ved viewport + overlay | | RT-FEATURE-03 |
| HR-033 | P1 | in_progress | Save as Client Typography Pack | `INerdTypographyPackStore` + JSON; “Gem som klient” i catalog | `NerdTypographyPack` DTO + file store + DI registration + save/load/list tests; catalog save flow pending | RT-FEATURE-04 · RT-ROADMAP-F2 |
| HR-034 | P2 | open | Modular Scale Generator | Input base + ratio → generér alle roller + clamp wrap | | RT-FEATURE-05 |
| HR-035 | P1 | in_progress | Live Clamp Editor | Sliders min/preferred/max + live MudText preview i catalog | Catalog editor UI implemented; persistence/configured-role binding pending | RT-FEATURE-06 |
| HR-036 | P2 | open | Accessibility Storyboard | Resize 200%, min 16px body, line-height — pass/fail pr. role i UI | | RT-FEATURE-07 |
| HR-038 | P2 | open | Brand Pack Bundle | Én JSON/zip: colors + typography + recipes | | RT-FEATURE-09 |
| HR-039 | P3 | open | Figma / Tokens Studio Sync (typography) | Eksport/import font-size tokens | | RT-FEATURE-10 |
| HR-040 | P1 | done | Typography snapshot tests | bUnit: computed sizes ved 320/768/1280/1920 for presets | Deterministiske viewport-tests for H1 og Body1 ved 320/768/1280/1920 | RT-ROADMAP-F0 |

---

## Open — Cross-cutting (HR-050–HR-054)

| ID | P | Status | Task | DoD (verifiable) | Evidence | Source |
|----|---|--------|------|------------------|----------|--------|
| HR-050 | P1 | open | PlayBook typography panel | Typo-scale preview i PlayBook med aktive packs | | RT-ROADMAP-F4 |
| HR-051 | P1 | open | Hub dynamic token/typo counts | Design System Hub viser “Tokens (N)” + link til aktivt pack | | DT-ROADMAP-F3 |
| HR-052 | P2 | open | Demo design-system recipes from DI | `/design-system` læser recipes fra DI, ikke hardcode | | DT-ROADMAP-F3 |
| HR-053 | P2 | done | Editorial / Dashboard typography presets | `NerdTypographyPresets` udvidet med 2 nye presets | `ApplyEditorial` og `ApplyDashboard` med tests | RT-ROADMAP-F3 |

---

## Partial

| ID | Status | Task | Remaining |
|----|--------|------|-----------|
| HR-014 | partial | Copy Snippets (Design Tokens) | Unified 3-format copy (class + Razor + CSS) |
| HR-030 | partial | Device Frame Studio | Vælg sample-tekst; evt. flere devices; “studio” polish |
| HR-037 | partial | Copy Snippets (Typography) | Tilføj C# options-snippet ved siden af Razor/CSS |

---

## Parked

| ID | Status | Task |
|----|--------|------|
| | | |

---

## Shipped (done)

| ID | Status | Task | Evidence |
|----|--------|------|----------|
| HR-031 | done | Breakpoint Comparison Table | `NerdResponsiveTypographyCatalog.razor` MudTable; commit d56080c |
| HR-060 | done | Design Tokens strategy/roadmap/features docs | `src/.../DesignTokens/docs/` |
| HR-061 | done | Responsive Typography strategy/roadmap/features/docs | `src/.../ResponsiveTypography/docs/` |
| HR-062 | done | DNF presets + recipes (baseline) | `NerdDnfDesignTokenPresets`, `NerdDesignTokenRecipe` |
| HR-063 | done | Responsive catalog device frames (baseline) | `NerdResponsiveTypographyCatalog` device frames |
| HR-054 | done | Fix Demo `AddAdditionalAssemblies` dedupe | Demo smoke test via HTTP launch profile starter uden “Assembly already defined” |

---

## Summary

| Status | Count |
|--------|------:|
| **open** | **35** |
| **in_progress** | **1** |
| **partial** | **3** |
| **done** | **6** |
| **parked** | **0** |
| **Total** | **45** |

### Recommended next slices (MVP)

1. **HR-006** — Portal-aware pickers (blokerer troværdig PlayBook)
2. **HR-002** + **HR-021** — Client token packs (JSON + save/load)
3. **HR-035** — Live clamp editor
4. **HR-054** — Demo startup fix

### Agent commands

```bash
# Næste 5 slices (fra nerd-rules install)
bash "$HOME/.nerd-rules/scripts/backlog-next.sh" docs/BACKLOG.md --count 5
```

```text
Implement HR-006 only from docs/BACKLOG.md. Backlog runner rules. BACKLOG REPORT.
```
