# TheNerdCollective.MudComponents.Shared

Shared WCAG helpers, clipboard utilities, and the `/nerd-design-system` hub
used by the MudBlazor design token and responsive typography packages.

## Setup

```csharp
builder.Services.AddNerdDesignSystem(options =>
{
    options.HubRoute = "/nerd-design-system";
    options.DesignTokensRoute = "/nerd-design-tokens";
    options.TypographyRoute = "/nerd-typography";
});
```

Register the hub assembly during `MapRazorComponents` when catalogs are enabled:

```csharp
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddNerdDesignSystemHub(app.Services);
```

`AddNerdDesignTokens` and `AddNerdResponsiveTypography` call `AddNerdDesignSystem`
automatically and update hub links when catalogs are registered.

## Assets

Catalog pages call `AddNerdDesignSystemAssets()` to register this assembly and
load `wwwroot/nerd-shared.js` for clipboard copy and file download helpers.
