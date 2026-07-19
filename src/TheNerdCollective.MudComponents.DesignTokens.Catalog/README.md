# TheNerdCollective.MudComponents.DesignTokens.Catalog

Optional visual catalog for design tokens and recipes. Production apps typically install only `DesignTokens` plus one `Brand.*` package; add this package when you want `/nerd-design-tokens` and the recipes studio in Development or internal tooling.

## Setup

```csharp
using TheNerdCollective.MudComponents.DesignTokens;

builder.Services.AddNerdDesignTokens(/* ... */);
builder.Services.AddNerdDesignTokenCatalog();

// ...

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddNerdDesignTokenCatalog(app.Services);
```

See [DesignTokens README](../TheNerdCollective.MudComponents.DesignTokens/README.md) for catalog options and routes.
