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
┌──────────────────────────────────────────────────────────────────────────┐
│  Token pack (JSON) — source of truth                                        │
│  colors · aliases · recipes · shell · frameworkDefaults · pairings          │
└───────────────────────────────┬──────────────────────────────────────────┘
                                │
                                ▼
┌──────────────────────────────────────────────────────────────────────────┐
│  DesignTokens.Core (framework-agnostic)                                     │
│  NerdDesignTokenOptions · recipes · WCAG · component intents · shell model  │
└───────┬──────────────┬──────────────┬──────────────┬─────────────────────┘
        ▼              ▼              ▼              ▼
   MudBlazor      Radzen        Blazorise      Fluent UI / Bootstrap Blazor
   adapter        adapter       adapter        adapter
   (CSS rules)    (CSS rules)   (CSS rules)    (CSS rules)
        └──────────────┴──────────────┴──────────────┘
                                │
                                ▼
              Catalog · PlayBook · Workbook · Token Studio Host
```

**Princippet:** Registrér token → resten opdateres automatisk. Ingen manuel
katalog-markup pr. farve.

**Nyt (2026-07):** Adskil tre lag der kan bruges på tværs af alle Blazor UI-frameworks:

| Lag | Eksempel | Hvem bruger det |
|-----|----------|-----------------|
| **Brand tokens** | `skov`, `graes`, `himmel` | Designere, workbook |
| **Component intents** (semantic aliases) | `primary-action`, `nav-item-active` | Udviklere i markup — samme API i Mud/Radzen/Fluent |
| **Shell recipes** | `sidebar`, `hero`, `footer` | Layout regions — surface + content + action |
| **Framework defaults** | `frameworkDefaults.mudblazor.navMenu` | Adapter — mapper intents/recipes til framework-selectors |

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

**Navngivning:** Brug rolle-navne (`sidebar`, `app-nav`), aldrig farvenavne (`coral-navmenu`,
`dnf-graes-sidebar`). Farven lever i pack JSON som `action: "graes"`, ikke i CSS-klassen.

### 7. Cross-framework component intents (ny)

**Problem:** `tnc-coral` på en `MudButton` giver ikke automatisk samme look på Radzen
`RadzenButton`, Blazorise `Button`, Fluent `FluentButton` eller Bootstrap Blazor.

**Løsning:** Ét stabilt **intent-vocabulary** i Core — framework adapters oversætter til
komponent-selectors.

| Intent (alias) | Typisk brug | DNF-pack | TNC-pack |
|----------------|-------------|----------|----------|
| `primary-action` | Filled CTA, primær knap | `graes` / `himmel` | `coral` |
| `secondary-action` | Outlined/secondary knap | `himmel` | `navy` |
| `danger-action` | Destruktiv handling | `morgenrode` | `coral` |
| `muted-content` | Sekundær tekst, labels | `hav` | `ink` |
| `page-surface` | Main content baggrund | `kridt-lys` | `snow` |
| `brand-chrome` | App bar, top chrome | `skov` | `navy` |
| `on-brand-chrome` | Tekst/ikoner på chrome | `kridt-lys` | `chalk` |
| `nav-surface` | Drawer / side-nav baggrund | `kridt-lys` | `snow` |
| `nav-item` | Default nav-link | `skov` | `ink` |
| `nav-item-active` | Aktiv/hover nav accent | `graes` / `sol` | `coral` |
| `input-surface` | TextField, Select baggrund | `kridt-lys` | `snow` |
| `input-border` | Input outline | `hav` | `ink` (subtle) |
| `focus-ring` | Focus-visible | `himmel` | `coral` |

Markup (identisk på tværs af frameworks):

```razor
@* MudBlazor *@
<MudButton Class="@Ui(NerdDesignSystemUi.PrimaryAction)" Variant="Variant.Filled">Støt nu</MudButton>
<MudTextField Class="@Ui(NerdDesignSystemUi.InputSurface)" />

@* Radzen / Blazorise / Fluent — samme intent-klasse, anden adapter genererer CSS *@
```

**Regel:** Udviklere sætter **intent**, ikke `Color="Color.Primary"` og ikke `dnf-graes`
i applikationskode (undtagen brand-showcase/catalog).

### 8. Framework adapter control (MudBlazor theme-first + PseudoCss)

Sådan “tæmmer” man MudBlazor uden at genopfinde det:

1. **Theme Provider First** — `NerdMudThemeProvider` ejer global `:root` brand-palette via
   `NerdMudThemeFactory` + `MudThemeProvider.GenerateTheme`. Token CSS duplikerer ikke
   `--mud-palette-*` på brand root (HR-158).
2. **PseudoCss per intent/recipe** — `:root .{prefix}-{intent}` scopes fra
   `NerdMudIntentThemeFactory` / `NerdMudRecipeThemeFactory` i **ét** `<style>` (HR-170).
   Slå intent palette CSS fra i token stylesheet når `UseIntentPseudoCssThemes` er true (HR-167).
3. **Bridges only i token CSS** — `MudBlazorSwitchBridge` m.m. for switch/tabs/inputs; ingen
   bulk `[class*="mud-button-filled"]` på semantic intents (HR-161).
4. **Én provider i host** — `MainLayout` / `NerdAppShell` bruger `NerdMudThemeProvider`;
   catalogs passerer igennem via `NerdCatalogThemeProvider` når `NerdMudThemeHost` cascade er sat (HR-171).
5. **Brand switch** — `INerdMudThemeController.ApplyBrandPack` opdaterer `MudTheme` uden at
   regenerere hele token CSS unødigt (HR-172).
6. **Typography + layout i MudTheme** — `NerdResponsiveTypographyOptions` → `MudTheme.Typography`;
   pack `radii` / `shadows` / `spacing.drawer-width` → `LayoutProperties` + `Shadows` (HR-173–174).
7. **Portal scope** — pickers, selects, menus arver token via `PopoverClass` + `nerd-shared.js`.
8. **PlayBook som regressions-matrix** — hver intent × komponent med state storyboard.
9. **WCAG gate** — pairings fra designmanual som `approvedPairings` + CI.

**Undgå:** Flere nested `MudThemeProvider` (HR-166 parked). Én `GenerateTheme` emitter alle scopes.

**Inspiration fra industrien:**

| System | Mønster | Læring for os |
|--------|---------|---------------|
| Material Design 3 | Komponent-tokens med states (`NavigationDrawerItemColors`) | Nav kræver hover/active/selected — ikke kun surface |
| Atlassian | `elevation.surface.sunken` til side-nav | Navngiv efter kontekst, ikke hex |
| Style Dictionary | Primitive → semantic → component | Core intents; adapters = “platforms” |
| Tokens Studio Pro | Composition tokens | Vores recipes = runtime composition tokens |
| Carbon | Layer tokens til nav vs. page | `nav-surface` kan adskilles fra `page-surface` |

**Undgå:** Én recipe per MudBlazor-komponent (80+). Det er `frameworkDefaults` + intents.

### 9. Shell recipes + DNF designmanual (reference)

**Referencebrand:** Danmarks Naturfredningsforening (DNF) identity 2025 — PDF +
layout-variationer dækker bredt UI-spektrum og er allerede delvist kodet i `Brand.Dnf`.

DNF-manualen viser (ud over farvepar) disse **layout-mønstre** vi bør kunne udtrykke
som shell recipes i JSON:

| Recipe (forslag) | DNF-manual / skærmbillede | surface · content · action |
|------------------|---------------------------|----------------------------|
| `hero-photo` | Hero med foto + mørk overlay + stor headline | `jord`/`skov` · `kridt` · `graes` |
| `hero-organic` | Mørk skov + organisk blad-watermark | `skov`/`blad` · `kridt` · `graes` |
| `hero-light` | Lys himmelblå hero + skov tekst | `himmel` · `skov` · `graes` |
| `header-chrome` | Transparent/mørk top-nav over hero | `skov` (transparent) · `kridt` · `graes` |
| `sidebar` | App drawer / vertikal nav | `kridt-lys` · `skov` · `graes` |
| `cta-strip` | Mørk band + lime/græs CTA (eksisterer) | `skov` · `kridt` · `sol`/`graes` |
| `link-card` | Kridt kort med pil (eksisterer) | `kridt` · `skov` · `hav` |
| `footer` | Flerkolonne footer + sociale ikoner (eksisterer) | `jord` · `kridt` · `himmel` |
| `footer-minimal` | Enkelt legal-bar | `skov` · `kridt` · — |
| `feature-panel` | Skov panel med 3 emner på hero | `skov` · `kridt` · — |
| `partner-row` | “I samarbejde med” logo-række | `jord` · `kridt` · — |
| `formular` | Formular på lys surface | `kridt-lys` · `skov` · `himmel` |

Farvepar og watermark-opacity kommer fra PDF (WCAG AA dokumenteret) — allerede i
`approvedPairings` og `NerdDnfPairingPolicy`.

**Shell bindings** (fremtidig pack-felt):

```json
{
  "shell": {
    "appBar": { "alias": "brand-chrome" },
    "drawer": { "alias": "nav-surface" },
    "navMenu": { "recipe": "sidebar" },
    "main": { "alias": "page-surface" }
  },
  "frameworkDefaults": {
    "mudblazor": {
      "button": { "filled": "primary-action", "outlined": "secondary-action", "text": "muted-content" },
      "textField": { "intent": "input-surface" },
      "datePicker": { "popover": "page-surface" },
      "navLink": { "default": "nav-item", "active": "nav-item-active" }
    }
  }
}
```

Valgfri wrapper `<NerdAppShell>` / `<NerdNavMenu>` læser `shell` + defaults — markup i
host-apps forbliver minimal.

### 10. Multi-framework adapters (Blazor-økosystemet)

| Framework | Token-mekanisme | Adapter-ansvar |
|-----------|-----------------|----------------|
| **MudBlazor** | CSS-klasser + `--mud-palette-*` override | `MudBlazorComponentRuleBuilder` (eksisterer) |
| **Radzen** | `rz-*` + theme CSS variables | `RadzenComponentRuleBuilder` (HR-115) |
| **Blazorise** | Bootstrap 5 + `b-*` / utility classes | Map intents → Bootstrap custom properties |
| **Fluent UI Blazor** | Fluent design tokens / CSS | Map intents → `--accent-fill-rest` etc. |
| **Bootstrap Blazor** | Bootstrap-baseret (kinesisk økosystem) | Samme som Blazorise-linje — bootstrap bridge |

**Fælles kontrakt i Core:**

- `NerdDesignSystemUi` intent-konstanter
- `NerdDesignTokenRecipe` for shell
- `INerdFrameworkTokenAdapter` → `GenerateCss(options) : string`
- PlayBook framework switcher (HR-118) viser samme pack side-by-side

### 11. Portal-aware tokens

MudBlazor popovers (pickers, selects, menus) renderes uden for DOM-scope.
Fleksibilitet kræver:

- Token-klasse på `PopoverClass` **eller**
- Generisk portal-bridge (`data-nerd-token` + JS observer)

Uden dette vil kataloget “lyve” om picker-farver.

## Scope-grænser (undgå skråplan)

| Tilladt | Ikke nu |
|---------|---------|
| ~12 shell recipes (DNF manual + TNC) | Recipe per MudBlazor-komponent |
| ~15 stabile component intents | Farvenavne i CSS-klasser (`tnc-coral-*`) |
| `frameworkDefaults` pr. adapter | Fuld composition schema for alle props |
| Adapter-specifikke CSS rule builders | Mud-logik i Core-pakken |
| DNF som reference + TNC/Acme som varianter | Hård kodning af én kundes layout i generator |

## Anbefalet retning (prioriteret, opdateret 2026-07)

1. **Component intents udvidet** — `nav-surface`, `nav-item`, `nav-item-active`, `input-*` i
   aliases + `NerdDesignSystemUi` (HR-119)
2. **Sidebar shell recipe** — JSON + Mud nav-link state CSS i recipe scope (HR-120, HR-121)
3. **Dogfood i Token Studio Host** — `brand-chrome` app bar, `recipe-sidebar` nav (TS-019)
4. **DNF layout kit udvidelse** — hero-varianter, footer-minimal, feature-panel fra manual
   (HR-123)
5. **`frameworkDefaults` i pack schema** — mudblazor først, andre adapters senere (HR-122)
6. **Config + preset merge** — JSON packs oven på C#-presets (fase 1 done)
7. **DesignTokens.Core extract** — intents + recipes uden Mud-reference (HR-114 / TS-014) — *wave 1 shipped*
8. **Radzen + framework switcher** — bevis samme intents (HR-115, HR-118) — *done*

**Onboarding:** [RECIPE-INTENT-MODEL.md](RECIPE-INTENT-MODEL.md) — ~12 shell recipes + ~15 intents; ikke én recipe per Mud-komponent (HR-152).
9. **Portal token propagation** — pickers/selects/menus (HR-006 done; udvid til flere)
10. **Live editor + workbook** — shell bindings visuelt (HR-089 done; udvid step)

Se [ROADMAP.md](ROADMAP.md) og [FEATURES.md](FEATURES.md).

## Relaterede backlog-items

| ID | Beskrivelse |
|----|-------------|
| HR-119 | Component intent vocabulary (core) |
| HR-120 | Sidebar shell recipe |
| HR-121 | Mud nav-link rules i recipe scope |
| HR-122 | `frameworkDefaults` + `shell` i pack schema |
| HR-123 | DNF manual layout kit (hero/footer varianter) |
| HR-124 | `NerdAppShell` / shell binding consumer |
| TS-019 | Token Studio Host dogfood shell |
| TS-020 | DNF showcase route i PlayBook |
