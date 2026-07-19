# Recipe ≠ component — onboarding model

**Backlog:** HR-152 · **Product idea:** nerd-token-studio `docs/ideas.md` §3

Nerd Token Studio bruger **tre lag** mellem brand JSON og UI-framework. Det er bevidst *ikke* én recipe per MudBlazor-komponent.

---

## De tre lag

| Lag | Antal (typisk) | Hvad det er | Markup-eksempel |
|-----|----------------|-------------|-----------------|
| **Brand tokens** | 8–16 farver | Rå palette (`skov`, `coral`, `himmel`) | — (designere) |
| **Component intents** | ~15 aliases | Semantiske roller uafhængige af framework | `class="tnc-primary-action"` |
| **Shell recipes** | ~12 regioner | Layout-zoner: surface + content + action | `class="dnf-recipe-hero-photo"` |

**Framework defaults** (`frameworkDefaults.mudblazor`, Radzen, Fluent) er et **fjerde** lag: adapteren mapper intents/recipes til framework-selectors — ikke noget udviklere hardcoder i host-markup.

---

## Standard intents (~15)

Defineret i `NerdIntentCatalogTools.StandardIntents` og `NerdDesignSystemUi`:

| Intent | Typisk brug |
|--------|-------------|
| `primary-action` | Filled CTA-knapper |
| `on-primary-action` | Tekst på filled primary |
| `secondary-action` | Outlined / sekundær handling |
| `page-surface` | Hovedindhold, catalog baggrund |
| `brand-chrome` | App bar, top chrome |
| `nav-surface` | Drawer / side-nav baggrund |
| `nav-item` / `nav-item-active` | Nav links |
| `input-surface` / `input-border` / `focus-ring` | Formularfelter |
| `muted-content` | Hjælpetekst, captions |
| `info` / `success` / `danger` / `highlight` | Feedback-kanaler |

**Regel:** Brug intent-klasser i app-markup — ikke rå token-navne (`tnc-coral`) på komponenter.

---

## Shell recipes (~12)

Layout-kit recipes i reference packs (DNF/TNC). Eksempler:

| Recipe | Formål |
|--------|--------|
| `sidebar` | App drawer navigation |
| `hero` / `hero-photo` / `hero-organic` / `hero-light` | Hero-varianter fra brand manual |
| `feature-panel` | Stakkede feature-rækker |
| `partner-row` | Logo/partner-stribe |
| `footer` / `footer-minimal` | Sidefod |
| `formular` | Enkeltkolonne formular |
| `cta-strip` / `link-card` | Kampagne-strips |

Recipes er **regioner**, ikke `MudButton` / `MudCard`. Én recipe kan indeholde mange komponenter med forskellige intents.

---

## frameworkDefaults (adapter-lag)

I `token-pack.json`:

```json
"frameworkDefaults": {
  "mudblazor": {
    "palette": { "...": "primary-action" },
    "button": { "filled": "primary-action", "outlined": "secondary-action" },
    "textField": { "intent": "input-surface" },
    "navLink": { "default": "nav-item", "active": "nav-item-active" }
  }
}
```

- **Mud adapter** læser dette + harvest inventory → CSS state rules.
- **Radzen / Fluent** bruger `--nerd-intent-*` bridge CSS (samme intents).
- Nye frameworks tilføjer en adapter — ikke nye recipes pr. komponent.

---

## Component families (Core)

`reference/component-families/*.yaml` mapper framework-neutrale dele til intents, fx:

- `picker.day-selected` → `primary-action`
- Bruges af Mud harvest nu; Fluent/Radzen senere.

Dette hører i **DesignTokens.Core** (HR-114) — ikke i Mud-specifik CSS.

---

## Workbook-flow

1. **Palette** — rå farver  
2. **Aliases** — intents → tokens  
3. **Intents** — vælg intent, se live MudButton-preview (HR-148)  
4. **Recipes** — shell-regioner  
5. **Pairings** — godkendte WCAG-kombinationer  
6. **Theme sets** — light/dark  
7. **Spacing / transforms** — skalaer  
8. **Export** — CSS, JSON, DTCG, Stitch  

---

## Anti-patterns

| Gør ikke | Gør i stedet |
|----------|--------------|
| `Color="Color.Primary"` i branded apps | `Class="@Ui(NerdDesignSystemUi.PrimaryAction)"` |
| Ny recipe per Mud-komponent | Intent + `frameworkDefaults` |
| Hardcode hex i Razor | Token eller alias |
| 66 Mud recipes | ~12 shell + ~15 intents |

---

## Relateret

- [STRATEGY.md](./STRATEGY.md) §7–10  
- [FEATURES.md](./FEATURES.md)  
- Token Studio [QUICKSTART.md](../../../../../../docs/QUICKSTART.md) §6
