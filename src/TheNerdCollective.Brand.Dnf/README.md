# TheNerdCollective.Brand.Dnf

Danmarks Naturfredningsforening (DNF) brand pack for MudBlazor design tokens **and** responsive typography.

## Install (DNF MudBlazor app)

```xml
<PackageReference Include="TheNerdCollective.MudComponents.Shared" />
<PackageReference Include="TheNerdCollective.MudComponents.DesignTokens" />
<PackageReference Include="TheNerdCollective.Brand.Dnf" />
```

### Colors only

```csharp
using TheNerdCollective.Brand.Dnf;

builder.Services.AddMudServices();
builder.Services.AddNerdDnfBrand();
```

### Full design system (colors + typography)

```csharp
builder.Services.AddNerdDnfDesignSystem();
```

Or separately:

```csharp
builder.Services.AddNerdDnfBrand();
builder.Services.AddNerdDnfTypography();
```

Typography requires `TheNerdCollective.MudComponents.ResponsiveTypography` (referenced transitively by this package).

In `App.razor`:

```razor
<MudThemeProvider />
<NerdDesignTokenStyles />
```

## What you get

- **Identity version** — `2025.1` (`NerdDnfBrandPack.IdentityVersion`)
- **12 colors** — jord, ler, kridt, skov, himmel, … (2025 identity palette)
- **Recipes** — `hero`, `cta-strip`, `link-card`, `footer`, `kridt-himmel`
- **Semantic aliases** — `primary-action`, `page-surface`, `brand-chrome`, …
- **Opacity tokens** — `watermark`, `hero-overlay`
- **Pairing policy** — 12 godkendte content-on-surface par (identity guide)
- **Typography** — editorial clamp scale tuned for DNF (`NerdDnfTypographyPresets`)

## Development catalog (optional)

```xml
<PackageReference Include="TheNerdCollective.MudComponents.DesignTokens.Catalog" />
```

```csharp
builder.Services.AddNerdDnfBrand(options =>
{
    options.EnableCatalogPage = true;
    options.RestrictCatalogToDevelopment = true;
});
builder.Services.AddNerdDesignTokenCatalog();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddNerdDesignTokenCatalog(app.Services)
    .AddNerdDesignSystemHub(app.Services);
```

Full install guide: [docs/BRAND-PACKAGES.md](../../docs/BRAND-PACKAGES.md)
