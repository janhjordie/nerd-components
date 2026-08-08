# TheNerdCollective.MudComponents.DesignTokens.Catalog

Optional visual catalog for design tokens and recipes. Production apps typically install only `DesignTokens` plus one `Brand.*` package; add this package when you want `/nerd-design-tokens`, `/nerd-theme`, and the recipes studio in Development or internal tooling.

Published to NuGet as `TheNerdCollective.MudComponents.DesignTokens.Catalog`.

## Setup (complete)

```csharp
using TheNerdCollective.Brand.Dnf;
using TheNerdCollective.MudComponents.DesignTokens;
using TheNerdCollective.MudComponents.ResponsiveTypography;

builder.WebHost.UseStaticWebAssets(); // Mud/Blazor _content assets on dotnet run

builder.Services.AddMudServices();
builder.Services.AddNerdDnfDesignSystem(o =>
{
    o.EnableCatalogPage = true;
    o.RestrictCatalogToDevelopment = false; // gate on Production if needed
    o.UseIntentPseudoCssThemes = true;
});
builder.Services.AddNerdBrandPackIntegration(); // INerdBrandPackSource for Catalog
builder.Services.AddNerdDesignTokenCatalog();

var app = builder.Build();
app.UseStaticFiles();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddNerdDesignTokenCatalog(app.Services)
    .WithStaticAssets();
```

Layout must provide the theme host cascade (see DesignTokens README **Host checklist**):

```razor
<NerdDesignTokenStyles />
<CascadingValue Name="@NerdMudThemeHost.CascadingName" Value="true">
    <NerdMudThemeProvider Theme="@theme" DesignTokenOptions="@TokenOptions" />
    …
</CascadingValue>
```

`AddNerdDesignTokens` alone does **not** mount `/nerd-design-tokens`.

See [DesignTokens README](../TheNerdCollective.MudComponents.DesignTokens/README.md) for the full host checklist, Color vs Class, and package matrix.
