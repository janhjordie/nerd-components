# TheNerdCollective.MudComponents.ResponsiveTypography

CSS-native responsive font sizes for MudBlazor 9.6 themes. The package uses
`clamp()` through `MudTheme.Typography`, so it needs no JavaScript, viewport
service, breakpoint detection, or resize-triggered renders.

```csharp
using MudBlazor;
using TheNerdCollective.MudComponents.ResponsiveTypography;

var theme = new MudTheme();

theme.UseResponsiveTypography(options =>
{
    options.H1 = ResponsiveFontSize.Clamp("2rem", "4vw", "4rem");
    options.H3 = ResponsiveFontSize.Clamp("1.75rem", "3vw", "2.5rem");
});
```

Only configured roles are changed. Existing values for omitted roles remain
unchanged, and normal `MudText` `Typo` semantics continue to apply.

## Supported roles

`Default`, `H1`–`H6`, `Subtitle1`, `Subtitle2`, `Body1`, `Body2`, `Button`,
`Caption`, and `Overline` are supported. Leave a property `null` to preserve
the value already present in the theme.

## Validation

`ResponsiveFontSize.Clamp` rejects empty or whitespace arguments and arguments
containing commas. Pass each of the three CSS values separately; for example,
`calc(1rem + 1vw)` is valid, while a value containing a comma is rejected
because it would make the generated `clamp()` expression ambiguous.

The generated value is assigned directly to `MudTheme.Typography`, for example:

```csharp
var theme = new MudTheme();
theme.UseResponsiveTypography(options =>
{
    options.Default = "1rem";
    options.H3 = ResponsiveFontSize.Clamp("1.75rem", "3vw", "2.5rem");
});

// theme.Typography.H3.FontSize == "clamp(1.75rem, 3vw, 2.5rem)"
```

No JavaScript, `IBrowserViewportService`, breakpoint detection, or
resize-triggered render is involved.

## Visual catalog

Register typography in DI and optionally enable the catalog page:

```csharp
builder.Services.AddNerdResponsiveTypography(options =>
{
    options.EnableCatalogPage = true; // default
    options.CatalogRoute = "/nerd-typography";
    options.Typography.H1 = ResponsiveFontSize.Clamp("2rem", "4vw", "4rem");
    options.Typography.H3 = ResponsiveFontSize.Clamp("1.75rem", "3vw", "2.5rem");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddNerdResponsiveTypographyCatalog(app.Services)
    .AddNerdDesignSystemHub(app.Services);
```

`AddNerdResponsiveTypographyCatalog(app.Services)` registers the catalog when
`EnableCatalogPage` is `true`. The default route is `/nerd-typography`.

Presets and shared spacing:

```csharp
builder.Services.AddNerdResponsiveTypography(options =>
{
    NerdTypographyPresets.ApplyMarketing(options.Typography);
    options.Typography.H3 = ResponsiveFontSize.Clamp("1.75rem", "3vw", "2.5rem");
});
```

`NerdTypographyPresets.ApplyDenseApp` is available for compact application UIs.

To disable the page:

```csharp
options.EnableCatalogPage = false;
```

Custom route:

```razor
@page "/kunde/typography"
@rendermode InteractiveServer
<NerdResponsiveTypographyCatalog />
```

Set `options.CatalogRoute` to match your route so hub links stay correct.
