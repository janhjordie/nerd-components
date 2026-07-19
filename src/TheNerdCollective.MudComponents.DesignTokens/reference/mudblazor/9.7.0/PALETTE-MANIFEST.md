# MudBlazor 9.7.0 — palette manifest

Source: `MudThemeProvider.GenerateTheme` @ [v9.7.0](https://github.com/MudBlazor/MudBlazor/blob/v9.7.0/src/MudBlazor/Components/ThemeProvider/MudThemeProvider.razor.cs)

Token Studio **must** emit every `--mud-palette-*` key below on brand root for 100% component fidelity.

## Channel colors (×8)

For each of `primary`, `secondary`, `tertiary`, `info`, `success`, `warning`, `error`, `dark`:

| Variable | Notes |
|----------|-------|
| `--mud-palette-{channel}` | Fill |
| `--mud-palette-{channel}-rgb` | RGB components |
| `--mud-palette-{channel}-text` | Contrast text |
| `--mud-palette-{channel}-darken` | Derived |
| `--mud-palette-{channel}-lighten` | Derived |
| `--mud-palette-{channel}-hover` | RGBA with HoverOpacity |

## Base

| Variable |
|----------|
| `--mud-palette-black` |
| `--mud-palette-white` |

## Semantic text

| Variable |
|----------|
| `--mud-palette-text-primary` |
| `--mud-palette-text-primary-rgb` |
| `--mud-palette-text-secondary` |
| `--mud-palette-text-secondary-rgb` |
| `--mud-palette-text-disabled` |
| `--mud-palette-text-disabled-rgb` |

## Actions

| Variable |
|----------|
| `--mud-palette-action-default` |
| `--mud-palette-action-default-hover` |
| `--mud-palette-action-disabled` |
| `--mud-palette-action-disabled-background` |

## Surfaces & chrome

| Variable |
|----------|
| `--mud-palette-surface` |
| `--mud-palette-surface-rgb` |
| `--mud-palette-background` |
| `--mud-palette-background-gray` |
| `--mud-palette-drawer-background` |
| `--mud-palette-drawer-text` |
| `--mud-palette-drawer-icon` |
| `--mud-palette-appbar-background` |
| `--mud-palette-appbar-text` |

## Lines & tables

| Variable |
|----------|
| `--mud-palette-lines-default` |
| `--mud-palette-lines-inputs` |
| `--mud-palette-table-lines` |
| `--mud-palette-table-striped` |
| `--mud-palette-table-hover` |
| `--mud-palette-divider` |
| `--mud-palette-divider-rgb` |
| `--mud-palette-divider-light` |

## Grays & overlay

| Variable |
|----------|
| `--mud-palette-skeleton` |
| `--mud-palette-gray-default` |
| `--mud-palette-gray-light` |
| `--mud-palette-gray-lighter` |
| `--mud-palette-gray-dark` |
| `--mud-palette-gray-darker` |
| `--mud-palette-overlay-dark` |
| `--mud-palette-overlay-light` |
| `--mud-palette-border-opacity` |

## Related (theme provider, not palette prefix)

| Variable | Source |
|----------|--------|
| `--mud-ripple-color` | Theme |
| `--mud-ripple-opacity` | Theme |
| `--mud-ripple-opacity-secondary` | Theme |
| `--mud-elevation-0` … `--mud-elevation-25` | Shadows |
| `--mud-default-borderradius` | Layout |
| `--mud-typography-*` | Typography |

## Token Studio mapping

See `NerdMudBrandPaletteMap` and pack `frameworkDefaults.mudblazor.palette`.
