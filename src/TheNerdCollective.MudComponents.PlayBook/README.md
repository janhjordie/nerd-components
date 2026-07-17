# TheNerdCollective.MudComponents.PlayBook

Interactive MudBlazor 9.6 component playground. Browse every MudBlazor
component across all configured design tokens and responsive typography
presets, with direct links to the official MudBlazor API documentation.

## Setup

Register design tokens, responsive typography, and the PlayBook:

```csharp
builder.Services.AddNerdDesignTokens(options =>
{
    options.Prefix = "dnf";
    options.RestrictCatalogToDevelopment = false;
    options.Add("forest", new NerdColorToken { Value = "#365C3A", ContrastText = "#FFFFFF" });
});

builder.Services.AddNerdResponsiveTypography(options =>
{
    options.RestrictCatalogToDevelopment = false;
    NerdTypographyPresets.ApplyMarketing(options.Typography);
});

builder.Services.AddNerdPlayBook(options =>
{
    options.RestrictPlayBookToDevelopment = false;
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddNerdDesignTokenCatalog(app.Services)
    .AddNerdResponsiveTypographyCatalog(app.Services)
    .AddNerdPlayBook(app.Services)
    .AddNerdDesignSystemHub(app.Services);
```

Add styles in your layout:

```razor
<MudThemeProvider />
<NerdDesignTokenStyles />
```

Browse the PlayBook at `/nerd-playbook` (configurable via `PlayBookRoute`).

## Features

- **50+ components** organized by category (buttons, inputs, data display, feedback, surfaces, navigation, layout)
- **Token matrix** — each component rendered once per design token class
- **Typography presets** — switch between default, marketing, and dense app responsive typography
- **Dark mode preview** with `data-theme="dark"` token activation
- **Category and search filters** for large component libraries
- **MudBlazor API links** on every component card

## Custom route

```razor
@page "/kunde/playbook"
@rendermode InteractiveServer
<NerdPlayBook />
```

Set `options.PlayBookRoute` to match your route so hub links stay correct.
