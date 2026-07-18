# Design Tokens — fleksibilitetsanalyse

Dette dokument analyserer, hvordan **TheNerdCollective.MudComponents.DesignTokens**
kan blive et fuldt fleksibelt brand-system: nye tokens defineres ét sted og
dukker automatisk op i Design System Hub, Design Tokens-kataloget, PlayBook,
eksport og klient-presets.

## Nuværende styrker

| Kapabilitet | Status | Bemærkning |
|-------------|--------|------------|
| Brand-navngivne farver | ✅ | `dnf-skov`, `dnf-kridt`, … |
| Class-only override | ✅ | Ingen `Color.Primary` nødvendigt |
| Generisk MudBlazor CSS | ✅ | Én rule-builder for alle tokens |
| Contrast / Content / Surface | ✅ | Semantiske roller |
| Recipes (kombinationer) | ✅ | `NerdDesignTokenRecipe` |
| Katalog + WCAG | ✅ | `/nerd-design-tokens` |
| Eksport CSS / JSON / Stitch | ✅ | `NerdDesignTokenTools` |
| DNF-presets | ✅ | `NerdDnfDesignTokenPresets` |
| Portal/popover token-scope | 🟡 | Under arbejde via `nerd-shared.js` |

## Målarkitektur

```
┌─────────────────────────────────────────────────────────────┐
│  Token Source of Truth                                       │
│  JSON / C# presets / klient-lagring / import fra Figma/Stitch│
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│  NerdDesignTokenOptions + MudBlazorDesignTokenCssGenerator   │
│  → CSS-klasser, palette-map, recipes, accessibility          │
└───────┬─────────────┬─────────────┬─────────────┬───────────┘
        ▼             ▼             ▼             ▼
   Catalog UI    PlayBook     Design Hub     Export/API
   /nerd-…       recipes      hub links      CSS/JSON/MD
```

**Princippet:** Registrér token → resten opdateres automatisk. Ingen manuel
katalog-markup pr. farve.

## Fleksibilitetsmodeller (mulige veje)

### 1. Code-first (nuværende)

```csharp
options.Add("hav", new NerdColorToken { Value = "#0C2E3A", ContrastText = "#FDFAF3" });
```

- **For:** Type-sikkert, CI-venligt, versioneret i git
- **Imod:** Kræver deploy for visuelle brand-ændringer
- **Bedst til:** Shared packages, DNF-presets, agency-biblioteker

### 2. Config-first (JSON / YAML)

```json
{
  "prefix": "kunde",
  "colors": {
    "hav": { "value": "#0C2E3A", "contrastText": "#FDFAF3" }
  },
  "recipes": {
    "card-cta": { "surface": "kridt", "content": "skov", "action": "himmel" }
  }
}
```

- **For:** Designere/UX kan ændre uden C#-build; hot-reload i Development
- **Imod:** Mindre compile-time safety (løses med schema + validation)
- **Bedst til:** Multi-tenant apps, white-label, klient-presets

### 3. Client-persisted packs

Gem et **token pack** pr. klient:

| Felt | Eksempel |
|------|----------|
| `ClientId` | `dnf`, `kunde-acme` |
| `Prefix` | `dnf` |
| `Colors` | dictionary af `NerdColorToken` |
| `Recipes` | surface/content/action/border |
| `Radii` / `Shadows` | valgfrit |
| `Version` / `UpdatedAt` | audit |

Storage-muligheder:

1. **Filsystem** — `App_Data/token-packs/{clientId}.json` (simpelt, on-prem)
2. **Blob / Azure Table** — multi-instance hosting
3. **EF Core** — hvis host allerede har klient-DB
4. **Hybrid** — defaults i kode, overrides i storage

Runtime-flow:

1. Host resolverer `ClientId` (tenant / hostname / query)
2. `INerdTokenPackStore.LoadAsync(clientId)`
3. Merge: preset → client pack → request overrides
4. Regenerér CSS (cache med ETag / version)
5. Catalog + PlayBook læser samme `NerdDesignTokenOptions`

### 4. Discovery-driven UI (auto-sync)

Når tokens tilføjes, skal UI **ikke** hardcodes:

| Overflade | Binding |
|-----------|---------|
| Design Tokens catalog | `Options.Colors` / `Recipes` |
| PlayBook token matrix | `TokenOptions.Colors.Keys` |
| PlayBook recipes panel | `TokenOptions.Recipes` |
| Design System Hub | dynamisk badge: “N tokens” |
| Export | genereret fra samme options |

Nye tokens → genstart eller hot-reload → synlige overalt.

### 5. Semantic layer + brand layer

Adskil to lag:

1. **Brand tokens** — `skov`, `himmel`, `sol` (rå farver)
2. **Semantic aliases** — `primary-action`, `page-surface`, `danger`

```csharp
options.Alias("primary-action", "himmel");
options.Alias("page-surface", "kridt");
```

UX-folk arbejder i brand-navne; udviklere kan bruge stabile semantic aliases
i layouts uden at binde sig til ét brand-navn.

### 6. Recipe-first composition

Recipes er den rigtige model for “kridt surface + himmel action”:

```csharp
options.AddRecipe("card-cta", new NerdDesignTokenRecipe(
    Surface: "kridt",
    Content: "skov",
    Action: "himmel",
    Border: "himmel"));
```

Markup:

```razor
<MudCard Class="dnf-recipe-card-cta">…</MudCard>
```

Udvidelsespunkter: hero, footer, nav, alert-strip, link-card, watermark.

### 7. Portal-aware tokens

MudBlazor popovers (pickers, selects, menus) renderes uden for DOM-scope.
Fleksibilitet kræver:

- Token-klasse på `PopoverClass` **eller**
- Generisk portal-bridge (`data-nerd-token` + JS observer)

Uden dette vil kataloget “lyve” om picker-farver.

## Anbefalet retning (prioriteret)

1. **Config + preset merge** — JSON packs oven på C#-presets  
2. **Client pack store** — gem/load pr. klient  
3. **Auto-discovery UI** — catalog/PlayBook/Hub binder kun til options  
4. **Recipe library** — standard DNF/MUI-lignende layouts  
5. **Portal token propagation** — pickers/selects/menus  
6. **Live editor** — UX kan tune tokens i Development og gemme som klient-pack  

Se [ROADMAP.md](ROADMAP.md) og [FEATURES.md](FEATURES.md).
