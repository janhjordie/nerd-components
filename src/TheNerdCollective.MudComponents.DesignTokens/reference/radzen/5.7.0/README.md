# Radzen Blazor — nerd intent bridge (HR-115)

Version-pinned reference for the **Radzen adapter spike** (TS-015).

```text
radzen/
  5.7.0/
    README.md
    INTENT-MAP.md
```

## Architecture

```text
token pack aliases  →  --nerd-intent-*  →  --rz-primary / --rz-body-background-color
        │                      │                        │
   NerdIntentCssGenerator      │           NerdRadzenDesignTokenCssGenerator
                               │
                    Mud adapter (palette-first)
```

Wrap host layout with brand roots for side-by-side previews (TS-018):

```html
<div class="tnc-nerd-brand tnc-mud-brand tnc-radzen-brand">
```

Load Radzen theme CSS **before** nerd token styles so `--rz-*` overrides win.

## Status

| Piece | Status |
|-------|--------|
| `--nerd-intent-*` emission | ✅ |
| Radzen theme bridge (`--rz-primary`, …) | ✅ spike in `NerdRadzenDesignTokenCssGenerator` |
| Radzen NuGet + live preview components | 📋 TS-015 follow-up |
| `RadzenComponentRuleBuilder` | 📋 HR-115 follow-up |

## When bumping Radzen

1. Add `reference/radzen/{version}/`
2. Diff upstream theme variables vs `INTENT-MAP.md`
3. Update `NerdRadzenPaletteMap` convention bindings
4. Refresh Playwright when Radzen preview ships
