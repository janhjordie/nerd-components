# Blazorise intent bridge (HR-116)

Blazorise 1.7.x on Bootstrap 5 uses the same theme custom properties as our Bootstrap bridge (HR-153).

## Brand root

`{prefix}-blazorise-brand` — apply on layout root alongside `{prefix}-bootstrap-brand` and `{prefix}-nerd-brand`.

## Generator

`NerdBlazoriseDesignTokenCssGenerator.AppendBlazoriseBrandPalette` delegates to
`NerdBootstrapDesignTokenCssGenerator` and documents the Blazorise root class.

## Markup

```razor
<Button Class="@($"{prefix}-primary-action")" Color="Color.Primary">Save</Button>
```

Intent class + Blazorise component; `--bs-primary` is driven by `primary-action` intent.
