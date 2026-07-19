# WCAG guide — Nerd design system

Praktisk reference for WCAG 2.1 AA i vores MudBlazor-pakker (typografi, farver, kontrast).

## Typografi (1.4.4 + 1.4.12)

WCAG **kræver ikke** 16px på alle roller. Kravene er:

| Kriterium | Krav | Vores storyboard |
|-----------|------|------------------|
| **1.4.4 Resize text** | Tekst kan forstørres til 200% uden tab af funktion | Brug `rem`, `em`, `clamp()`, `vw` — aldrig fast `px` til brødtekst |
| **1.4.12 Text spacing** | Line-height ≥ 1,5 · letter-spacing ≥ 0,12em · word-spacing ≥ 0,16em | Håndhæves i typography-packs |
| **Kontrast (1.4.3)** | 4,5:1 normal tekst · 3:1 large text (≥18pt / ~24px eller 14pt fed) | Design token catalog |

### Anbefalede minimum-størrelser @ 320px viewport

Disse er **best practice** for læsbarhed — ikke direkte WCAG-AA pixel-regler:

| Tier | Roller | Min @ 320px |
|------|--------|-------------|
| **Primary** | Default, Body1, Subtitle1, H1–H6 | 16px (1rem) |
| **Secondary** | Body2, Subtitle2, Button | 14px (0,875rem) |
| **Auxiliary** | Caption, Overline | 12px (0,75rem) — matcher MudBlazor default |

### Typiske fejl

- **Caption på 16px+** — unødvendigt stort; 12–13px med god kontrast er normalt.
- **`1rem` = 20px** — hvis `html { font-size: 20px }` eller browser-zoom 125%, bliver alle `rem`-værdier større end beregnet.
- **"Min 16px" badge** — viser storyboard-gulv for rollen, ikke desktop-computed size.

## Farver og kontrast (1.4.3)

| Teksttype | AA | AAA |
|-----------|----|-----|
| Normal | 4,5:1 | 7:1 |
| Large (≥24px / 18pt, eller ≥18,67px fed) | 3:1 | 4,5:1 |

Tjek token-palette i `/nerd-design-tokens` — advarsler ved opstart hvis kontrast fejler.

## Hvad vi checker automatisk

- Relative font-enheder per rolle
- Rolle-specifikt minimum @ 320px
- Line-height ≥ 1,5
- Letter-spacing ≥ 0,12em
- Token-kontrast mod WCAG AA

## Links

- [Responsive typography catalog](/nerd-typography)
- [Design tokens](/nerd-design-tokens)
- [WCAG 2.1 — Understanding SC 1.4.4](https://www.w3.org/WAI/WCAG21/Understanding/resize-text.html)
- [WCAG 2.1 — Understanding SC 1.4.12](https://www.w3.org/WAI/WCAG21/Understanding/text-spacing.html)
