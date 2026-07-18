---
title: "Backlog Portfolio — Design System"
status: Active
author: "@janhjordie"
last_updated: "18-07-2026 11:40"
---

# Backlog Portfolio — Design System

Control board for **Design Tokens** og **Responsive Typography** i TheNerdCollective.Components.  
Master register: [../BACKLOG.md](../BACKLOG.md)

---

## Control Board

| Stream | Epic focus | Open | Partial | Done | Next slice |
|--------|------------|-----:|--------:|-----:|------------|
| Design Tokens | Fleksible brand-packs + UX catalog | 24 | 1 | 2 | **HR-006** Portal pickers (in progress) |
| Responsive Typography | Fluid type + client packs | 9 | 2 | 2 | **HR-035** Live clamp editor |
| Cross-cutting | Hub, PlayBook, Demo | 5 | 0 | 1 | **HR-054** Demo dedupe |

---

## Portfolio Metrics

| Metric | Target | Current |
|--------|--------|---------|
| Features fra package docs i backlog | 30/30 | 30/30 |
| P0 slices remaining | 0 | 2 (HR-002, HR-006) |
| Client pack (colors + type) | Shippable | Not started |
| Demo starts cleanly | Yes | No (HR-054) |

---

## Dependency Map

```text
HR-021 NerdTokenPack DTO
  └── HR-002 Save as Client Pack (tokens)
        └── HR-019 Multi-Brand Switcher
        └── HR-010 Token Diff

HR-033 Typography Pack
  └── HR-038 Brand Pack Bundle (colors + type)

HR-006 Portal-aware Pickers
  └── HR-016 Layout Kits (troworthy PlayBook)

HR-054 Demo dedupe
  └── All catalog E2E verification
```

---

## Decision Log

| Date | ID | Decision |
|------|-----|----------|
| 18-07-2026 | PORT-001 | Master backlog i `docs/BACKLOG.md` med `HR-###` IDs per nerd-rules host template |
| 18-07-2026 | PORT-002 | Features importeret fra package `docs/FEATURES.md` + roadmap stabilisering |
| 18-07-2026 | PORT-003 | HR-030/HR-031 baseline markeret partial/done efter catalog-forbedring |
| 18-07-2026 | PORT-004 | HR-006 startet: portal-scope for token-klasser på `.mud-popover-open` |

---

## Source documents

| Package | Strategy | Roadmap | Features |
|---------|----------|---------|----------|
| Design Tokens | [STRATEGY.md](../src/TheNerdCollective.MudComponents.DesignTokens/docs/STRATEGY.md) | [ROADMAP.md](../src/TheNerdCollective.MudComponents.DesignTokens/docs/ROADMAP.md) | [FEATURES.md](../src/TheNerdCollective.MudComponents.DesignTokens/docs/FEATURES.md) |
| Responsive Typography | [STRATEGY.md](../src/TheNerdCollective.MudComponents.ResponsiveTypography/docs/STRATEGY.md) | [ROADMAP.md](../src/TheNerdCollective.MudComponents.ResponsiveTypography/docs/ROADMAP.md) | [FEATURES.md](../src/TheNerdCollective.MudComponents.ResponsiveTypography/docs/FEATURES.md) |

---

## Now / Next / Later

### Now (P0–P1, unblockers)

- HR-006 Portal-aware pickers (in progress)
- HR-002 Client token packs
- HR-054 Demo startup fix
- HR-021 Token pack DTO (in progress)

### Next (P1 catalog UX)

- HR-001 Live Token Studio
- HR-004 Recipe Composer
- HR-012 Dark mode dual preview
- HR-035 Live clamp editor
- HR-033 Typography client packs

### Later (P2–P3)

- HR-007 Figma export
- HR-018 Brand health score
- HR-020 Collaborative comments
- HR-039 Typography Figma sync
