# Radzen Blazor 5.7 — intent map

Maps nerd semantic intents to [Radzen Blazor](https://blazor.radzen.com/) theme CSS custom properties.

## Convention bindings (spike)

| Radzen variable | Nerd intent | Channel |
|-----------------|-------------|---------|
| `--rz-primary` | `primary-action` | surface |
| `--rz-secondary` | `secondary-action` | surface |
| `--rz-success` | `success` | surface |
| `--rz-info` | `info` | surface |
| `--rz-warning` | `highlight` | surface |
| `--rz-danger` | `danger` | surface |
| `--rz-body-background-color` | `page-surface` | surface |
| `--rz-text-color` | `page-surface` | content |
| `--rz-text-secondary-color` | `muted-content` | content |

Implementation: `NerdRadzenPaletteMap.CreateConventionBindings()`.

## CSS output example (TNC)

```css
.tnc-nerd-brand, .tnc-radzen-brand {
  --rz-primary: var(--nerd-intent-primary-action-surface);
  --rz-secondary: var(--nerd-intent-secondary-action-surface);
  --rz-body-background-color: var(--nerd-intent-page-surface-surface);
  --rz-text-color: var(--nerd-intent-page-surface-content);
  /* … */
}
```

## Component rules (next)

Picker and button states will map via `RadzenComponentRuleBuilder` — **not** by copying Mud CSS selectors.
