# TheNerdCollective.MudComponents.DesignTokens

Customer-specific CSS design tokens for MudBlazor 9.6. Define meaningful
colors such as `sand`, `forest`, `sun`, and `sea`, then use the generated
classes directly on any MudBlazor component. No wrappers, JavaScript, or
MudBlazor fork is required.

## Setup

```csharp
builder.Services.AddNerdDesignTokens(options =>
{
    options.Prefix = "dnf";
    options.Add("sand", new NerdColorToken
    {
        Value = "#E8D8AD",
        ContrastText = "#2D2D2D",
        Hover = "#D8C58E"
    });
    options.Add("forest", new NerdColorToken
    {
        Value = "#365C3A",
        Light = "#4D7A50",
        Dark = "#203B25",
        Hover = "#2D4D30",
        Surface = "#F0F7F0",
        Content = "#19301D",
        Interactive = "#2D4D30"
    });
});
```

Add the style component once, after `MudThemeProvider`:

```razor
<MudThemeProvider />
<NerdDesignTokenStyles />
```

Use the customer vocabulary in markup:

```razor
<MudGrid Class="dnf-forest">
    <MudText Class="dnf-sand">Nature first</MudText>
    <MudButton Class="dnf-forest">Read more</MudButton>
</MudGrid>
```

The generator emits the MudBlazor palette variables used by all components
that consume the theme variables, so the token works generically on
`MudGrid`, `MudPaper`, `MudCard`, and other components. It also emits explicit
selectors for filled, outlined, and text buttons, chips, alerts, badges,
progress bars, icon buttons, links, typography, hover, focus, active, and
disabled states. This covers both inherited theme behavior and component
variants where MudBlazor uses a more specific selector.

Token names must be lowercase CSS identifiers, such as `sand`,
`forest-dark`, or `sea-2`. Each application can define a different set of
tokens and a different prefix.

`ContrastText` is optional for hex colors and is calculated automatically when
omitted. `Light` and `Dark` provide mode-specific values; dark values are
activated below an ancestor with `data-theme="dark"`. `Surface`, `Content`,
and `Interactive` are semantic roles that can be consumed by application CSS
without changing the token's component selectors.

The generated selectors are intentionally versioned against MudBlazor 9.6.
The package emits both MudBlazor palette variables and explicit selectors for
component variants and states, so updates to MudBlazor can be checked with the
CSS snapshot tests before changing the package dependency.

## Design-system extras

```csharp
options.Alias("primary-action", "forest");
options.AddRadius("card", "12px");
options.AddShadow("elevated", "0 4px 16px rgba(0,0,0,.16)");
```

This produces `dnf-primary-action`, `dnf-radius-card`, and
`dnf-shadow-elevated`. MudBlazor CSS is isolated in
`@layer nerd-design-tokens`; this does not import or mix Bootstrap or
Tailwind styles.

For static hosting, generate a CSS artifact at build time:

```csharp
NerdDesignTokenTools.WriteCss(options, "wwwroot/css/dnf-tokens.css");
```

Tokens can be exported with `NerdDesignTokenTools.ExportJson(options)`, and
`NerdDesignTokenTools.CheckAccessibility(options)` reports WCAG AA contrast
failures.

## CSS layers and scopes

CSS layers are opt-in because unlayered application CSS and MudBlazor CSS can
otherwise have different cascade precedence:

```csharp
options.UseCssLayer = true;
options.CssLayerName = "dnf-tokens";
options.ScopeSelector = "[data-brand='dnf']";
```

The package never imports Bootstrap or Tailwind. The layer only groups this
package's generated MudBlazor overrides. Scopes generate selectors such as
`[data-brand='dnf'] .dnf-forest`.

## Google Stitch

Tokens can be exported to a portable Stitch `DESIGN.md` handoff:

```csharp
File.WriteAllText(
    "DESIGN.md",
    NerdDesignTokenTools.ExportStitchDesignMd(options));
```

Google Stitch's open `DESIGN.md` format can then be imported into Stitch or
converted with Google's tooling, for example:

```bash
npx @google/design.md export --format dtcg DESIGN.md > tokens.json
npx @google/design.md export --format css-tailwind DESIGN.md > theme.css
```

The generated DTCG/Tailwind files can be reviewed alongside the generated
MudBlazor CSS, keeping the customer's design source portable across tools.

## Visual catalog

Register the catalog page assembly and browse all tokens at
`/nerd-design-tokens`:

```csharp
builder.Services.AddNerdDesignTokens(options =>
{
    options.Prefix = "dnf";
    options.CatalogRoute = "/nerd-design-tokens";
    options.EnableCatalogPage = true;
    options.RestrictCatalogToDevelopment = true;
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddNerdDesignTokenCatalog();
```

The catalog shows color swatches, light/dark preview, WCAG AA/AAA badges,
live MudBlazor component previews, aliases, radius, and shadow tokens.

To use a custom route, create a host page and render the shared catalog
component:

```razor
@page "/kunde/design-tokens"
@rendermode InteractiveServer
<NerdDesignTokensCatalog />
```
