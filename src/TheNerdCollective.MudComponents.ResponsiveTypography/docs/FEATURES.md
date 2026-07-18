# Responsive Typography — 10 features frontend/UX vil elske

Features der løfter fluid type fra “teknisk clamp-helper” til et værktøj
designere og frontend faktisk åbner hver dag.

Prioritet: 🔥 høj · ⭐ medium · 💡 vision

---

## 1. Device Frame Studio 🔥

Tre frames side om side: 375 · 768 · 1440 px med samme tekstprøve.

**Hvorfor:** Instant forståelse af fluid type — ingen browser-resize.

## 2. Breakpoint Comparison Table 🔥

Tabel: Role × Viewport → computed px + clamp-string.

**Hvorfor:** UX og frontend taler i konkrete tal, ikke “det ser større ud”.

## 3. Type Scale Curve ⭐

Graf over H1→Caption ved aktuel viewport + overlay ved andre viewports.

**Hvorfor:** Visuel hierarki-QA på sekunder.

## 4. Save as Client Typography Pack 🔥

Samme flow som Design Tokens: gem JSON-pack pr. ClientId.

**Hvorfor:** White-label type uden kodeændring.

## 5. Modular Scale Generator ⭐

Input: base `1rem` + ratio `1.25` → generér alle roller + wrap i clamp.

**Hvorfor:** Professionel scale uden manuel trial-and-error.

## 6. Live Clamp Editor 🔥

Sliders for min / preferred / max med live MudText preview.

**Hvorfor:** UX kan fine-tune uden IDE.

## 7. Accessibility Storyboard ⭐

Resize 200%, min 16px body, line-height — pr. role med pass/fail chips.

**Hvorfor:** A11y er synligt, ikke kun startup-logs.

## 8. Copy Snippets (Razor + CSS + Options) 🔥

```razor
<MudText Typo="Typo.h1">Overskrift</MudText>
```

```csharp
options.H1 = ResponsiveFontSize.Clamp("2rem", "4vw", "4rem");
```

**Hvorfor:** Korrekt snippet første gang.

## 9. Brand Pack Bundle 💡

Én zip/JSON: colors + typography + recipes.

**Hvorfor:** Hele brandet følger klienten.

## 10. Figma / Tokens Studio Sync 💡

Eksportér font-size tokens; importér tilbage til packs.

**Hvorfor:** Bro mellem design-tool og MudBlazor.

---

## Prioriteret MVP

1. Device frames + breakpoint table (catalog)
2. Live clamp editor + copy snippets
3. Client typography packs
4. Modular scale generator

## Succeskriterier

- UX forstår responsive type uden at åbne DevTools
- Frontend kopierer snippets der matcher kataloget
- Klient-packs kan skiftes som Design Tokens
