# Fluent UI Blazor 4.11 — intent map

Maps nerd semantic intents to [Fluent UI Web](https://react.fluentui.dev/) CSS custom properties consumed by **Fluent UI Blazor**.

## Convention bindings (spike)

| Fluent token | Nerd intent | Channel |
|--------------|-------------|---------|
| `--colorBrandBackground` | `primary-action` | surface |
| `--colorBrandForeground1` | `on-primary-action` | content |
| `--colorNeutralBackground1` | `page-surface` | surface |
| `--colorNeutralForeground1` | `page-surface` | content |
| `--colorNeutralForeground2` | `muted-content` | content |
| `--colorNeutralStroke1` | `input-border` | border |
| `--colorStrokeFocus1` | `focus-ring` | interactive |

Implementation: `NerdFluentBlazorPaletteMap.CreateConventionBindings()`.

## Component families (next)

Picker parts from `reference/component-families/picker.yaml` will map to Fluent-specific selectors in a future `FluentComponentRuleBuilder` — **not** by copying Mud CSS.

| Family part | Intent |
|-------------|--------|
| `picker.day-selected` | `primary-action` |
| `picker.popover-surface` | `page-surface` |
| `picker.field-border` | `input-border` |

## CSS output example (TNC)

```css
.tnc-nerd-brand {
  --nerd-intent-primary-action-surface: var(--tnc-color-primary-action-surface);
  /* …all aliases… */
}

.tnc-nerd-brand, .tnc-fluent-brand {
  --colorBrandBackground: var(--nerd-intent-primary-action-surface);
  --colorBrandForeground1: var(--nerd-intent-on-primary-action-content);
  /* … */
}
```
