# Bootstrap 5 intent bridge (HR-153)

Maps nerd `--nerd-intent-*` variables to Bootstrap 5.3 theme custom properties.

## Convention bindings

| Bootstrap variable | Nerd intent channel |
|--------------------|---------------------|
| `--bs-primary` | `primary-action.surface` |
| `--bs-secondary` | `secondary-action.surface` |
| `--bs-body-bg` | `page-surface.surface` |
| `--bs-body-color` | `page-surface.content` |
| `--bs-border-color` | `input-border.border` |
| `--bs-link-color` | `primary-action.interactive` |

## Brand root class

`{prefix}-bootstrap-brand` — apply alongside `{prefix}-nerd-brand` on layout shells.

Shared with future **Blazorise** adapter (HR-116) — same Bootstrap 5 custom property surface.

## Generator

`NerdBootstrapDesignTokenCssGenerator.AppendBootstrapBrandPalette` is emitted when
`EmitFrameworkNeutralIntents` is true (same gate as Fluent/Radzen).
