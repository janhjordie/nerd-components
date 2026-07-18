# Design Tokens — roadmap

Plan for at gøre systemet **super fleksibelt**: definér tokens → de vises i
Design System, PlayBook og eksport → gem som klient-pack.

## Fase 0 — Stabilisering (nu)

- [x] Class-only brand override
- [x] Generisk `MudBlazorComponentRuleBuilder`
- [x] Content vs ContrastText (lys/mørk tokens)
- [x] Recipes (`NerdDesignTokenRecipe`)
- [x] DNF-presets (12 farver)
- [ ] Portal/popover token-scope (pickers, select, menu) — færdiggøres
- [ ] Snapshot-tests for alle DNF tokens + recipes

## Fase 1 — Token source of truth

**Mål:** Ét format, flere indgange.

1. `NerdTokenPack` DTO (JSON-serialiserbar)
2. Loaders:
   - `FromOptions(Action<…>)` (nuværende)
   - `FromJson(Stream / path)`
   - `FromPreset(string name)` (`dnf`, `demo`, …)
3. Validation pipeline:
   - navne, hex/rgb/hsl
   - WCAG AA/AAA
   - recipe references (surface/content/action findes)
4. Schema (`token-pack.schema.json`) til IDE + CI

**Accept:** Tilføj `sunset` i JSON → katalog + PlayBook viser den uden Razor-ændring.

## Fase 2 — Klient-packs (gem pr. klient)

**Mål:** White-label / multi-tenant brands.

| API | Ansvar |
|-----|--------|
| `INerdTokenPackStore` | Load/Save/List/Delete |
| `INerdTokenPackResolver` | ClientId → pack (cache) |
| `NerdTokenPackService` | Merge preset + client + overrides |
| Catalog “Save as client” | UX-flow i Development |

Persistens (vælges pr. host):

- Default: fil under `App_Data/token-packs/`
- Optional: Azure Blob / EF Core adapter

Versionering:

- `pack.Version` bumpes ved save
- CSS cache-key = `clientId:version`
- Diff-view: “hvad ændrede klienten ift. preset?”

**Accept:** Gem `acme` pack → genstart → `/nerd-design-tokens` viser Acme-tokens.

## Fase 3 — Auto-discovery i UI

**Mål:** Ingen hardkodede token-lister i UI.

| Sted | Ændring |
|------|---------|
| Design Tokens catalog | Allerede options-drevet; udvid med recipes-tab + client pack UI |
| PlayBook | Token filter + recipes panel (påbegyndt) |
| Design System Hub | Dynamisk “Tokens (N)” + link til aktivt pack |
| Demo `/design-system` | Læs recipes fra DI i stedet for hardcode |

**Accept:** Tilføj 3 tokens i pack → Hub, Catalog og PlayBook viser 3 nye tiles.

## Fase 4 — Live editor & eksport

- Inline color picker + kontrast-live
- “Promote to preset” / “Save as client”
- Export: CSS, JSON, Stitch DESIGN.md, Figma Tokens Studio-format
- Import: paste JSON / upload pack

## Fase 5 — Recipes & layout-kits

- Recipe catalog (hero, CTA card, nav, footer, alert strip)
- DNF layout-kit baseret på brand-PDF
- PlayBook “Compositions” kategori

## Fase 6 — Portal & edge cases

- Generisk popover token propagation
- Dialog / drawer / tooltip parity
- Dark mode recipe variants

## Definition of Done (fleksibilitet)

En UX-/frontend-person skal kunne:

1. Oprette en ny farve `aurora`
2. Se den i kataloget med swatch + MudBlazor-preview
3. Se den i PlayBook-matrix
4. Bruge den i markup: `Class="kunde-aurora"`
5. Gemme pack som klient `kunde-acme`
6. Eksportere CSS til et andet repo

uden at ændre C# i DesignTokens-pakken.

## Afhængigheder

- Shared: `NerdColorParser`, download/clipboard
- PlayBook: recipe panel + token filter
- ThemeKit: valgfri synk af global Primary ↔ semantic alias
- Host app: `ClientId` resolution

## Relaterede dokumenter

- [STRATEGY.md](STRATEGY.md) — modeller og arkitektur
- [FEATURES.md](FEATURES.md) — 20 feature-forslag
- [../../docs/DESIGN-TOKENS.md](../../../docs/DESIGN-TOKENS.md) — nuværende arkitektur
