# Design Tokens — 20 features frontend/UX vil elske

Features der gør **MudBlazor Design Tokens** til det værktøj designere,
UX’ere og frontend-udviklere faktisk vil bruge hver dag — ikke kun et CSS-hack.

Prioritet: 🔥 høj · ⭐ medium · 💡 vision

---

## 1. Live Token Studio (in-browser) 🔥

Visuel editor på `/nerd-design-tokens`:

- Farve-picker + hex/rgb/hsl
- Live MudButton / MudChip / MudTextField preview
- WCAG-meter der opdateres mens man trækker
- Undo/redo

**Hvorfor:** UX kan tune brand uden IDE.

## 2. Save as Client Pack 🔥

Knappen **“Gem som klient”**:

- Navn + ClientId
- Gemmer JSON-pack
- Skifter aktivt pack i sessionen

**Hvorfor:** White-label og agency-arbejde i ét klik.

## 3. Token Discovery Everywhere 🔥

Tilføj token → automatisk i:

- Catalog swatches
- PlayBook matrix
- Hub badges
- Export

**Hvorfor:** Ingen “glemte” tokens i UI.

## 4. Recipe Composer 🔥

Drag surface + content + action:

```
Surface: kridt  ·  Content: skov  ·  Action: himmel
→ .dnf-recipe-card-cta
```

Preview: MudCard + MudButton + MudText.

**Hvorfor:** Matcher rigtige designs (DNF-PDF: cards, CTA, hero).

## 5. Contrast Pair Matrix ⭐

Tabel ala DNF brand-PDF:

| Forgrund | Baggrund | Ratio | AA | AAA |
|----------|----------|-------|----|-----|
| Kridt | Jord | 11.8 | ✅ | ✅ |

**Hvorfor:** Designers sprog = dokumenterede par, ikke kun hex.

## 6. Portal-aware Pickers 🔥

Date/Time/Select/Menu-popovers arver token-farve.

**Hvorfor:** Ellers “lyver” PlayBook om farven.

## 7. Figma / Tokens Studio Export ⭐

Eksport til Tokens Studio JSON + import tilbage.

**Hvorfor:** Bro mellem Figma og MudBlazor.

## 8. Stitch / DESIGN.md Sync ⭐

Allerede delvist der — udvid med recipes, radii, shadows, typography hooks.

**Hvorfor:** AI-designværktøjer får komplet brand-kontekst.

## 9. Accessibility Gate in CI 🔥

`dotnet test` + optional build-fail hvis AA fejler.

**Hvorfor:** Brand-fejl fanges før merge.

## 10. Token Diff & Changelog ⭐

Vis diff mellem preset og klient-pack:

```
+ aurora #7C5CFF
~ himmel #81ABFF → #6B9BFF
- legacy-sand
```

**Hvorfor:** Klientændringer bliver reviewbare.

## 11. Semantic Alias Browser ⭐

UI der viser `primary-action → himmel` med “hvor bruges den?”.

**Hvorfor:** Layouts kan være brand-agnostiske.

## 12. Dark Mode Dual Preview 🔥

Split-view: light | dark side om side for samme token.

**Hvorfor:** Hurtigere mode-QA end toggle.

## 13. Hover / Focus / Disabled Storyboard ⭐

Lille state-strip pr. komponent: default → hover → focus → disabled.

**Hvorfor:** UX ser alle tilstande uden at “prøve sig frem”.

## 14. Copy Snippets (Razor + CSS + Class) 🔥

Én knap → tre formater:

```razor
<MudButton Class="dnf-himmel" Variant="Variant.Filled">Støt nu</MudButton>
```

**Hvorfor:** Frontend kopierer korrekt første gang.

## 15. Watermark / Opacity Tokens 💡

Tokens til watermark-farver med opacity (fra DNF-PDF).

**Hvorfor:** Brand-PDF’er bruger ofte semi-transparente overlays.

## 16. Layout Kits (Hero, Nav, Footer) 🔥

Færdige composition-previews i PlayBook:

- Hero banner
- Link card
- CTA strip
- Footer columns

bygget på recipes + tokens.

**Hvorfor:** Fra farve → færdig sektion.

## 17. Token Search & Favorites ⭐

Søg `him`, favoritstjern, “senest brugt”.

**Hvorfor:** Skalerer til 20–50 tokens.

## 18. Brand Health Score 💡

Samlet score: kontrast, naming, recipe coverage, unused tokens.

**Hvorfor:** Giver PM/UX et enkelt tal at følge.

## 19. Multi-Brand Switcher 🔥

Dropdown: DNF | Acme | Demo — regenererer CSS live i Development.

**Hvorfor:** Agency kan skifte klient på sekunder.

## 20. Collaborative Comments (Dev-only) 💡

Annotationer på tokens: “brug ikke til body text”, “CTA only”.

**Hvorfor:** Design-beslutninger lever ved siden af farven.

---

## Prioriteret MVP (næste 3 sprints)

1. Token packs (JSON) + client save/load  
2. Live Token Studio (basic) + dual preview  
3. Portal-aware pickers + Recipe Composer  
4. Figma/Tokens Studio export + CI accessibility gate  

## Succeskriterier

- En UX’er kan oprette og gemme et klient-brand uden C#
- En frontend kan kopiere snippeds og stole på PlayBook-farver (inkl. pickers)
- Et designteam kan eksportere til Figma/Stitch uden manuel mapping
