# Design tokens

Dette dokument beskriver formålet, arkitekturen og den korrekte brug af
**TheNerdCollective.MudComponents.DesignTokens** — vores brand-farvesystem til
MudBlazor.

## Formål

MudBlazor leverer et globalt theme med `Primary`, `Secondary`, `Tertiary` osv.
Det er fint til ét samlet app-theme, men det er **ikke** design-venligt når et
produkt skal arbejde med mange brandfarver med meningsfulde navne som `forest`,
`sand`, `sea` eller `sunset`.

Design tokens løser det ved at give jer:

1. **Brand-navngivne farver** — et ordforråd I selv definerer, ikke MudBlazors
   semantiske palette-navne.
2. **Én CSS-klasse pr. token** — f.eks. `.dnf-forest` eller `.dnf-sand`.
3. **Fuld MudBlazor-dækning** — klassen overskriver alle relevante MudBlazor
   selectors, så komponenter ser korrekte ud uden at sætte `Color.Primary`.
4. **Ubegrænset antal tokens** — I kan registrere 2, 20 eller flere tokens i
   samme app; hver får sin egen klasse og sit eget sæt CSS-variabler.

Kort sagt: **`<MudButton Class="dnf-forest">` skal være nok** til at knappen
får jeres brandfarve med korrekt kontrast, hover, disabled og understøttelse af
alle MudBlazor-varianter (filled, outlined, text, checkbox, radio, switch,
rating, osv.).

## Hvad design tokens *ikke* er

| Koncept | Design tokens | MudTheme / ThemeKit |
|--------|---------------|---------------------|
| Formål | Mange brandfarver på tværs af UI | Ét globalt app-theme |
| API for udviklere | `Class="dnf-sand"` | `MudTheme`, `Color.Primary` |
| Navngivning | `forest`, `sand`, `sea-2` | `Primary`, `Secondary` |
| Scope | Per komponent eller container | Hele appen via `MudThemeProvider` |

Design tokens og MudTheme kan sameksistere. Det globale theme styrer standard-UI;
design tokens bruges når en specifik komponent eller sektion skal bære en
bestemt brandfarve.

## Arkitektur

Når I registrerer tokens i `Program.cs`, genereres der CSS via
`MudBlazorDesignTokenCssGenerator`. For hver token oprettes:

### 1. Brand-variabler

```css
.dnf-forest {
  --dnf-color-forest: #365C3A;
  --dnf-color-forest-text: #FFFFFF;
  --dnf-color-forest-hover: #2D4D30;
  --dnf-color-forest-surface: …;
  --dnf-color-forest-content: …;
}
```

Disse er jeres **design source of truth** — de kan eksporteres som JSON, CSS og
Stitch `DESIGN.md`.

### 2. Scoped MudBlazor palette

På samme klasse mappes brand-variablerne til MudBlazors interne
`--mud-palette-*` variabler (primary, secondary, text-primary, lines-inputs,
osv.). Det gør, at MudBlazor-komponenter inden for scope læser de rigtige
farver — uden at udvikleren skal kende palette-detaljerne.

### 3. Generiske komponent-selectors

`MudBlazorComponentRuleBuilder` tilføjer **ét sæt regler per token**, baseret på
MudBlazors klassekonventioner (`mud-button-filled`, `mud-checkbox`,
`mud-radio`, …). Reglerne er identiske for alle tokens; kun CSS-variablerne
skifter.

Der må **aldrig** være særregler for enkelttokens (f.eks. sær-CSS kun for
`forest`). Hvis `sand` og `forest` opfører sig forskelligt, er det en fejl i den
generiske generator — ikke i markup.

## Token-model (`NerdColorToken`)

| Felt | Betydning |
|------|-----------|
| `Value` | Standard brandfarve (hex, rgb, hsl) |
| `Light` / `Dark` | Mode-specifikke værdier; dark aktiveres under `[data-theme="dark"]` |
| `ContrastText` | Tekst/ikon på **fyldt** brandbaggrund (filled buttons, chips, alerts) |
| `DarkContrastText` | Contrast i dark mode |
| `Hover` | Hover/active accent (mørkere nuance eller interaktiv farve) |
| `Active` | Fokus/pressed (valgfri) |
| `Border` | Kantfarve til outlined inputs og borders |
| `Disabled` | Disabled-tilstand |
| `Surface` | Baggrund/surface inden for token-scope |
| `Content` | Brødtekst og labels (læsbar på surface) |
| `Interactive` | Interaktiv accent (fallback for hover) |

`ContrastText` beregnes automatisk for hex-farver, hvis den udelades.

### Kontrastprincip

- **Filled** komponenter: baggrund = brand (`Value`), tekst = `ContrastText`
- **Outlined / text** komponenter: tekst = `Content` (læsbar på hvid/surface),
  kant = `Border` (typisk brand), baggrund transparent
- **Labels og inputs**: tekst = `Content`, caret og accent = brand
- **Checkbox / radio / switch**: ikon-accent = brand, uden store ripple-cirkler

Alle tokens følger **samme** semantik — lys `sand` og mørk `forest` skal begge
virke korrekt uden særbehandling.

## Opsætning

```csharp
builder.Services.AddNerdDesignTokens(options =>
{
    options.Prefix = "dnf";

    options.Add("forest", new NerdColorToken
    {
        Value = "#365C3A",
        ContrastText = "#FFFFFF",
        Hover = "#2D4D30"
    });

    options.Add("sand", new NerdColorToken
    {
        Value = "#E8D8AD",
        ContrastText = "#2D2D2D",
        Hover = "#D8C58E"
    });
});
```

Tilføj styles én gang i layout (efter `MudThemeProvider`):

```razor
<MudThemeProvider />
<NerdDesignTokenStyles />
```

Registrer kataloget (valgfrit):

```csharp
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddNerdDesignTokenCatalog(app.Services);
```

## Brug i markup

### På enkelt komponent

```razor
<MudButton Class="dnf-forest" Variant="Variant.Filled">Gem</MudButton>
<MudButton Class="dnf-sand" Variant="Variant.Outlined">Annuller</MudButton>
<MudChip T="string" Class="dnf-forest">Natur</MudChip>
```

**Brug ikke** `Color="Color.Primary"` til at opnå brandfarve — det er
MudBlazors globale theme. Brandfarve kommer fra `Class`.

### På container (descendant scope)

```razor
<MudStack Class="dnf-forest">
    <MudButton Variant="Variant.Filled">Handling</MudButton>
    <MudTextField T="string" Label="Navn" Variant="Variant.Outlined" />
</MudStack>
```

Alle MudBlazor-komponenter under containeren arver token-scope, fordi CSS-reglerne
understøtter både `.{token}` på elementet selv og `.{token} .mud-*` på børn.

### Dark mode

Sæt `data-theme="dark"` på en ancestor (eller brug katalogets preview-toggle):

```razor
<div class="dnf-forest" data-theme="dark">
    …
</div>
```

Tokenets `Dark` og `DarkContrastText` aktiveres via CSS.

### Aliases

```csharp
options.Alias("primary-action", "forest");
```

Giver klassen `.dnf-primary-action` med samme styling som `.dnf-forest`.

## Katalog og kvalitetssikring

`/nerd-design-tokens` (konfigurerbart via `CatalogRoute`) viser:

- Swatches for light/dark
- Live MudBlazor-previews per token
- WCAG 2.1 AA/AAA badges og kontrastforhold
- Eksport af CSS, JSON og Stitch `DESIGN.md`

Kataloget er et **verifikationsværktøj** — det må ikke indeholde token-specifik
CSS eller patches. Hvis previews ser forkerte ud, rettes den generiske generator.

Ved startup kan `WarnOnAccessibilityFailuresAtStartup` logge tokens der ikke
opfylder WCAG AA.

## Eksport og build-time CSS

```csharp
NerdDesignTokenTools.WriteCss(options, "wwwroot/css/dnf-tokens.css");
```

Nyttigt til statisk hosting eller CI-genereret CSS-artefakt.

## Designprincipper for vedligeholdelse

1. **Én mekanisme** — brand-klasse + genererede CSS-regler; ingen inline-styles
   eller manuelle overrides i app-kode.
2. **Ingen særregler per token** — `forest` og `sand` skal bruge samme selector-
   logik.
3. **MudBlazor-klasser som kontrakt** — generatorn følger MudBlazors
   klassekonventioner; ved MudBlazor-opgradering tjekkes snapshot-tests.
4. **Tokens er brand-ordforråd** — navne skal give mening for designere og
   udviklere (`sand`, ikke `primary-variant-3`).
5. **Kontrast er data** — `ContrastText` og `Content` er en del af tokenet, ikke
   noget der hardcodes i komponenter.

## DNF reference palette

Danmarks Naturfredningsforening (DNF) uses two shared text colors across all brand
tiles:

| Name | Hex | Role |
|------|-----|------|
| **Kridt** | `#FDFAF3` | Text on dark brand backgrounds |
| **Skov** | `#002D26` | Text on light brand backgrounds |

Register the full palette in one call:

```csharp
builder.Services.AddNerdDesignTokens(options =>
{
    options.Prefix = "dnf";
    NerdDnfDesignTokenPresets.Apply(options);
});
```

This registers 12 tokens (`jord`, `ler`, `kridt`, `kridt-lys`, `sol`, `morgenrode`,
`hav`, `himmel`, `flod`, `skov`, `blad`, `graes`) with WCAG pairings matching the
DNF identity tiles.

## Relaterede pakker

| Pakke | Rolle |
|-------|-------|
| `TheNerdCollective.MudComponents.DesignTokens` | Token-registration, CSS-generering, katalog |
| `TheNerdCollective.Blazor.ThemeKit` | Globalt MudTheme-redigering (separat concern) |
| `TheNerdCollective.MudComponents.ResponsiveTypography` | Responsive typografi via MudTheme |

## Fejlfinding

| Symptom | Sandsynlig årsag |
|---------|------------------|
| Ingen brandfarve | `NerdDesignTokenStyles` mangler, eller klassenavn matcher ikke `Prefix` |
| Forkert kontrast på lys token | `ContrastText` / `Content` mangler eller er for lav kontrast |
| Kun nogle komponenter styled | Klassen sidder ikke på komponenten eller en ancestor |
| `forest` virker, `sand` ikke | Fejl i generisk generator — ikke token-specifik markup |
| Dark mode uændret | Mangler `data-theme="dark"` på ancestor med token-klassen |

## Versionering

CSS-selectors er versioneret mod **MudBlazor 9.6** (`MudBlazorVersion` i options).
Ved opgradering af MudBlazor: kør `TheNerdCollective.MudComponents.DesignTokens.Tests`
og verificer kataloget visuelt.
