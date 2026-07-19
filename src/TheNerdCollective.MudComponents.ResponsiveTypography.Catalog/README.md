# TheNerdCollective.MudComponents.ResponsiveTypography.Catalog

Optional visual catalog for responsive typography. Production apps typically install only `ResponsiveTypography` plus optional brand typography packs; add this package for `/nerd-typography` in Development or internal tooling.

## Setup

```csharp
using TheNerdCollective.MudComponents.ResponsiveTypography;

builder.Services.AddNerdResponsiveTypography(/* ... */);

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddNerdResponsiveTypographyCatalog(app.Services);
```

See [ResponsiveTypography README](../TheNerdCollective.MudComponents.ResponsiveTypography/README.md) for catalog options and routes.
