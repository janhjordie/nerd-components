# Responsive Typography — fleksibilitetsanalyse

Dette dokument analyserer, hvordan **TheNerdCollective.MudComponents.ResponsiveTypography**
kan blive det typografi-system frontend- og UX-folk faktisk bruger:
fluid type via `clamp()`, presets, katalog med viewport-simulation, og klient-packs.

## Nuværende styrker

| Kapabilitet | Status | Bemærkning |
|-------------|--------|------------|
| CSS `clamp()` i MudTheme | ✅ | Ingen JS / viewport service |
| Alle MudBlazor Typo-roller | ✅ | H1–H6, Body, Caption, … |
| Presets (Marketing / Dense) | ✅ | `NerdTypographyPresets` |
| Katalog + viewport slider | ✅ | `/nerd-typography` |
| WCAG resize / min-size checks | ✅ | Startup + catalog warnings |
| Line-height / letter-spacing | ✅ | Per-role overrides |
| Klient-packs / JSON | ❌ | Mangler (parallelt med Design Tokens) |
| Breakpoint-sammenligning i UI | 🟡 | Forbedres i kataloget |
| Scale-kurve / visualizer | ❌ | Feature-backlog |

## Målarkitektur

```
┌──────────────────────────────────────────────────────────────┐
│  Typography Source of Truth                                   │
│  C# presets / JSON packs / klient-overrides / Figma import    │
└──────────────────────────┬───────────────────────────────────┘
                           │
                           ▼
┌──────────────────────────────────────────────────────────────┐
│  ResponsiveTypographyOptions → MudTheme.Typography            │
│  clamp() + line-height + letter-spacing + accessibility       │
└───────┬─────────────┬─────────────┬─────────────┬────────────┘
        ▼             ▼             ▼             ▼
   Catalog UI    PlayBook     Design Hub     Export/API
   breakpoints   type scale   hub links      CSS/JSON
```

**Princippet:** Konfigurér roller ét sted → MudText, katalog og eksport følger med.

## Fleksibilitetsmodeller

### 1. Code-first (nuværende)

```csharp
options.Typography.H1 = ResponsiveFontSize.Clamp("2rem", "4vw", "4rem");
NerdTypographyPresets.ApplyMarketing(options.Typography);
```

- **For:** Type-sikkert, versioneret, CI
- **Imod:** Kræver deploy for fine-tuning
- **Bedst til:** Shared packages og agency-standarder

### 2. Config-first (JSON)

```json
{
  "preset": "marketing",
  "roles": {
    "h1": { "min": "2rem", "preferred": "5vw", "max": "4.5rem", "lineHeight": "1.15" }
  }
}
```

- **For:** UX kan tune uden C#
- **Imod:** Kræver schema + validation
- **Bedst til:** White-label / multi-tenant

### 3. Client-persisted packs

Samme mønster som Design Tokens:

| Felt | Eksempel |
|------|----------|
| `ClientId` | `dnf`, `acme` |
| `Preset` | `marketing` |
| `Roles` | overrides pr. Typo |
| `Version` | cache-bust |

Gem under `App_Data/typography-packs/` eller Blob — resolver pr. ClientId.

### 4. Modular scale (vision)

Definér base + ratio (`1.25` major third) → generér H1–Caption automatisk,
derefter wrap hver role i `clamp(min, fluid, max)`.

## Discovery-krav

Når en ny role konfigureres, skal den dukke op i:

1. `/nerd-typography` (katalog)
2. Design System Hub
3. PlayBook typography panel (fremtid)
4. CSS/JSON export

Uden manuelt Razor pr. role.

## Kobling til Design Tokens

Typography packs og color packs bør kunne versioneres sammen som et
**Brand Pack** (`colors` + `typography` + `recipes`).

Se Design Tokens: [STRATEGY.md](../../TheNerdCollective.MudComponents.DesignTokens/docs/STRATEGY.md).

## Relaterede dokumenter

- [ROADMAP.md](ROADMAP.md)
- [FEATURES.md](FEATURES.md)
- [EXAMPLES.md](EXAMPLES.md)
- [../README.md](../README.md)
