# MudBlazor 9.7.0 — CSS analysis (HR-130)

## 1. Scope

| Field | Value |
|-------|-------|
| MudBlazor NuGet | 9.7.0 |
| GitHub tag | v9.7.0 |
| Analysis date | 2026-07-19 |
| Adapter | `MudBlazorDesignTokenCssGenerator` + `MudBlazorComponentRuleBuilder` |

## 2. Harvest procedure

```bash
# Source SCSS (verify against tag v9.7.0)
# https://github.com/MudBlazor/MudBlazor/tree/v9.7.0/src/MudBlazor/Styles/components

# Compiled CSS (optional hash in sources/)
# https://github.com/MudBlazor/MudBlazor/blob/v9.7.0/src/MudBlazor/wwwroot/MudBlazor.min.css
```

Full automated harvest: **HR-133** (pending). This document is the committed baseline for adapter work on 9.7.0.

## 3. Palette contract

MudBlazor maps theme through `--mud-palette-*` CSS variables. Our generator mirrors these via `AppendPaletteVariables` in `MudBlazorDesignTokenCssGenerator.cs`.

Key channels used by stateful components in this analysis:

- `--mud-palette-primary` — active tab text, tab slider, switch checked track (via our channel token)
- `--mud-palette-text-primary` — inactive tab text (via our content token)
- Hardcoded switch thumb in Mud SCSS: `#fafafa` (known trap — must override in toolbar/catalog contexts)

## 4. P1 components — state model (verified)

### Tabs (`_tabs.scss`)

| State | DOM selector | Notes |
|-------|--------------|-------|
| Inactive | `.mud-tab:not(.mud-tab-active)` | Text uses content color |
| Active | `.mud-tab.mud-tab-active` | Text uses channel color |
| Slider | `.mud-tab-slider` | `background-color` = primary |
| Hover | `.mud-tab:hover` | Subtle background; must NOT use filled-button hover |

**Adapter fix (HR-131):** Remove `mud-tab` from `AccentTextPatterns`. Dedicated `AppendSwitchAndTabStateRules`.

### Switch (`_switch.scss`)

| State | DOM selector | Notes |
|-------|--------------|-------|
| Checked class | `.mud-switch-base.mud-checked` | **Not** on `.mud-switch` root |
| Track (checked) | `.mud-switch-base.mud-checked + .mud-switch-track` | Sibling selector |
| Thumb | `.mud-switch-thumb-medium` / inner `.mud-button-root` | Often hardcoded white |
| Label | `.mud-switch + .mud-typography` | Content token |

**Adapter fix (HR-132):** Remove `mud-switch` from `InputPatterns`. Fix track selector. Catalog toolbar band-aid remains until HR-135.

### DatePicker composite (`_picker.scss`, `_datepicker.scss`, `_popover.scss`)

See `reference/component-families/picker.yaml`. Portal content renders outside input DOM — requires popover surface rules + day-cell states (**HR-137**).

## 5. Breaking vs stable patterns

| Pattern | Stable in 9.7? | Adapter note |
|---------|----------------|--------------|
| `.mud-tab-active` on tab | Yes | Use for active state |
| `.mud-checked` on switch base | Yes | Never target `.mud-switch.mud-checked` |
| Popover portal siblings | Yes | Scope popover rules under token root |
| `mud-primary-text` class | Yes | Channel color, not content |

## 6. Adapter implications (9.7.0)

1. Split `ChannelTextPatterns` from `ContentTextPatterns` (mud-primary-text ≠ mud-typography).
2. State bridge method for tabs + switch labels.
3. Catalog chrome: inactive tab = page content, active = accent, slider = accent.
4. Do not apply filled-button hover to `.mud-tab:hover`.

## 7. Known traps

- Switch white thumb on white track (toolbar/catalog) — `AppendCatalogToolbarRules`
- All tabs same accent when flattened via AccentTextPatterns
- DatePicker popup not covered by input-only rules

## 8. Family index

| Component | Family |
|-----------|--------|
| MudTabs | navigation |
| MudSwitch | toggle |
| MudDatePicker, MudTimePicker, MudColorPicker, MudSelect | picker |
| MudTextField, MudNumericField | input |

## 9. Inventory files

- `inventory/_tabs.yaml`
- `inventory/_switch.yaml`

Remaining 64 SCSS files: see `coverage-matrix.md` (HR-136).
