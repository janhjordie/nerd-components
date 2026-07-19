# Responsive Typography — roadmap

Plan for fluid type der er **nemt at tune**, synligt i kataloget, og gemmeligt
pr. klient.

## Fase 0 — Stabilisering (nu)

- [x] `clamp()` via MudTheme
- [x] Catalog med viewport slider — motor + `ResponsiveTypography.Catalog` (HR-078)
- [x] WCAG accessibility tools
- [x] Marketing / Dense / Editorial / Dashboard presets
- [x] Responsive breakpoint-eksempler i katalog + docs
- [x] Snapshot-tests for computed sizes ved 320 / 768 / 1280 / 1920 — HR-040
- [x] DNF typography in `Brand.Dnf` — `AddNerdDnfTypography()` (HR-079)

## Fase 1 — Bedre catalog UX

1. Device frames (phone / tablet / desktop) side om side
2. Breakpoint comparison table (computed px pr. role)
3. Type scale curve (visualizer)
4. Copy snippets: Razor + CSS clamp

**Accept:** UX kan se H1 ved 375 / 768 / 1440 uden at resize browseren.

## Fase 2 — Typography packs

1. `NerdTypographyPack` JSON DTO
2. Loaders: FromOptions / FromJson / FromPreset
3. `INerdTypographyPackStore` (fil / blob)
4. “Gem som klient” i Development

**Accept:** Gem `acme` pack → genstart → katalog viser Acme-scale.

## Fase 3 — Modular scale & presets

- Scale generator (base + ratio)
- Flere presets: Editorial, Dashboard, Print-ish
- Per-breakpoint overrides (valgfrit, stadig CSS-first)

## Fase 4 — Integration

- PlayBook typography panel
- Brand Pack = colors + typography + recipes
- Figma / Tokens Studio export for font-size tokens
- CI gate: min font-size + resize guidance

## Definition of Done

En UX-/frontend-person skal kunne:

1. Vælge preset eller tune clamp-værdier
2. Se responsive forskelle i kataloget
3. Kopiere korrekt `MudText` + CSS
4. Gemme som klient-pack
5. Stole på WCAG-warnings

uden at ændre C# i pakken.

## Relaterede dokumenter

- [STRATEGY.md](STRATEGY.md)
- [FEATURES.md](FEATURES.md)
- [EXAMPLES.md](EXAMPLES.md)
