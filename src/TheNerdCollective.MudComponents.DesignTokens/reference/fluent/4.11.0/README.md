# Fluent UI Blazor — nerd intent bridge (HR-117)

Version-pinned reference for the **Fluent adapter spike** (TS-017). MudBlazor uses `reference/mudblazor/`; Fluent uses the same ritual:

```text
fluent/
  4.11.0/
    README.md
    INTENT-MAP.md
```

## Architecture

```text
token pack aliases  →  --nerd-intent-*  →  --colorBrand* / --colorNeutral*
        │                      │                        │
   NerdIntentCssGenerator      │           NerdFluentDesignTokenCssGenerator
                               │
                    Mud adapter (palette-first)
```

Wrap host layout with **both** brand roots when running side-by-side previews (TS-018):

```html
<div class="tnc-nerd-brand tnc-mud-brand tnc-fluent-brand">
```

## Status

| Piece | Status |
|-------|--------|
| `--nerd-intent-{alias}-{channel}` emission | ✅ in `NerdIntentCssGenerator` |
| Fluent token bridge (`--colorBrandBackground`, …) | ✅ spike in `NerdFluentDesignTokenCssGenerator` |
| Fluent NuGet + live preview page | 📋 TS-017 follow-up |
| Component-family rules (picker.day-selected) | 📋 uses `reference/component-families/` |

## When bumping Fluent UI

1. Add `reference/fluent/{version}/`
2. Diff upstream design tokens vs `INTENT-MAP.md`
3. Update `NerdFluentBlazorPaletteMap` convention bindings
4. Refresh Playwright when Fluent preview ships
