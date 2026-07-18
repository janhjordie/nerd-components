# Responsive Typography — eksempler

Konkrete, copy-paste klar eksempler. Se også live kataloget på
`/nerd-typography` (Device frames + breakpoint-tabel).

## 1. Minimal setup

```csharp
builder.Services.AddNerdResponsiveTypography(options =>
{
    options.EnableCatalogPage = true;
    options.Typography.H1 = ResponsiveFontSize.Clamp("2rem", "4vw", "4rem");
    options.Typography.Body1 = ResponsiveFontSize.Clamp("1rem", "2vw", "1.125rem");
});
```

```razor
<MudText Typo="Typo.h1">Fluid headline</MudText>
<MudText Typo="Typo.body1">Body der vokser blødt mellem min og max.</MudText>
```

## 2. Marketing preset (landing / hero)

```csharp
builder.Services.AddNerdResponsiveTypography(options =>
{
    NerdTypographyPresets.ApplyMarketing(options.Typography);
    // Override én role hvis nødvendigt:
    options.Typography.H1 = ResponsiveFontSize.Clamp("2.5rem", "5vw", "4.5rem");
});
```

Typisk brug:

| Viewport | Forventet H1-følelse |
|----------|----------------------|
| 375 px   | Tæt på min (~40px) |
| 768 px   | Midt i fluid-området |
| 1440 px  | Tæt på max (~72px) |

## 3. Dense app preset (admin / tables)

```csharp
builder.Services.AddNerdResponsiveTypography(options =>
{
    NerdTypographyPresets.ApplyDenseApp(options.Typography);
});
```

```razor
<MudText Typo="Typo.body1">Kompakt brødtekst til tætte grids.</MudText>
<MudText Typo="Typo.caption">Metadata og labels.</MudText>
```

## 4. Per-role line-height & letter-spacing

```csharp
options.Typography.H1 = ResponsiveFontSize.Clamp("2rem", "4vw", "4rem");
options.Typography.Roles.H1.LineHeight = "1.15";
options.Typography.Roles.Body1.LineHeight = "1.7";
options.Typography.LineHeight = "1.5"; // fallback
options.Typography.LetterSpacing = "0.01em";
```

## 5. Responsive “article” layout

```razor
<MudStack Spacing="3" Style="max-width: 42rem;">
    <MudText Typo="Typo.h2">Artikel-overskrift</MudText>
    <MudText Typo="Typo.subtitle1">Lead der skal være læsbar på mobil.</MudText>
    <MudText Typo="Typo.body1">
        Brødtekst med fluid font-size. På smalle skærme holder clamp()
        minimum; på store skærme stopper den ved maximum, så linjerne
        ikke bliver unaturligt store.
    </MudText>
    <MudText Typo="Typo.caption">Kilde · 18. jul 2026</MudText>
</MudStack>
```

## 6. Breakpoint mental model

`clamp(min, preferred, max)`:

```
320px ─── min ──────── preferred (vw) ──────── max ─── 1920px
              ▲                           ▲
         “for lille”                  “for stor”
```

Brug katalogets **Device frames** og **Breakpoint-tabel** til at verificere
computed px uden at gætte.

## 7. Anbefalede startværdier

| Role | Min | Preferred | Max |
|------|-----|-----------|-----|
| H1 | `2rem` | `5vw` | `4.5rem` |
| H2 | `1.75rem` | `3.5vw` | `3rem` |
| H3 | `1.5rem` | `3vw` | `2.25rem` |
| Body1 | `1rem` | `2.2vw` | `1.25rem` |
| Caption | `0.75rem` | `1.2vw` | `0.875rem` |

## 8. Custom catalog route

```csharp
options.CatalogRoute = "/kunde/typography";
```

```razor
@page "/kunde/typography"
@rendermode InteractiveServer
<NerdResponsiveTypographyCatalog />
```

## Relateret

- [STRATEGY.md](STRATEGY.md)
- [FEATURES.md](FEATURES.md)
- [../README.md](../README.md)
