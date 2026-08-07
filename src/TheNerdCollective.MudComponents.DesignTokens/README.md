# TheNerdCollective.MudComponents.DesignTokens

Customer-specific CSS design tokens for **MudBlazor 9.8+**. Define meaningful
colors such as `sand`, `forest`, `sun`, and `sea`, then use the generated
classes directly on any MudBlazor component. No wrappers, JavaScript, or
MudBlazor fork is required.

See [docs/DESIGN-TOKENS.md](../../docs/DESIGN-TOKENS.md) for architecture,
design principles, and the brand-token model.

## Host checklist (what you must add)

`AddNerdDesignTokens` alone is **not** enough for a working MudBlazor host.
Use this table:

| Goal | Packages | DI | Layout / pipeline |
|------|----------|----|-------------------|
| **A. Brand CSS + theme only** (product host, e.g. Consent) | `DesignTokens` + one `Brand.*` (or embedded pack) | `AddNerdDnfBrand()` / `AddNerdDesignTokensFromBrand("dnf")` | `NerdDesignTokenStyles` + `NerdMudThemeProvider` |
| **B. + `/nerd-design-tokens` catalog** | A + `DesignTokens.Catalog` (+ usually `ResponsiveTypography`) | A + `AddNerdDesignTokenCatalog()` + `AddNerdBrandPackIntegration()` (or `AddNerdDnfDesignSystem`) | A + `MapRazorComponents(…).AddNerdDesignTokenCatalog(services)` + theme host cascade |
| **C. Multi-brand studio** | B + several `Brand.*` | `AddNerdDesignTokenBrandPacks(…)` + controller | B + `INerdMudThemeController` / brand switcher |

### 1. Static web assets (required)

Without this, `_content/MudBlazor/MudBlazor.min.css` / `_framework/blazor.web.js` return **404** and the UI looks like unstyled HTML (not MudBlazor):

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseStaticWebAssets(); // needed for `dotnet run` (esp. Release) with NuGet assets

// …
var app = builder.Build();
app.UseStaticFiles();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .WithStaticAssets(); // .NET 9+/10
```

Also reference Mud CSS/JS in `App.razor`:

```razor
<link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
<script src="_content/MudBlazor/MudBlazor.min.js"></script>
<script src="_framework/blazor.web.js"></script>
```

### 2. Theme + styles (recommended host shell)

Prefer `NerdMudThemeProvider` (maps brand aliases → Mud `Color.Primary`…`Error`) and emit token CSS once:

```razor
@inject NerdDesignTokenOptions TokenOptions

<NerdDesignTokenStyles />
<CascadingValue Name="@NerdMudThemeHost.CascadingName" Value="true">
    <NerdMudThemeProvider Theme="@theme" DesignTokenOptions="@TokenOptions" />
    <MudPopoverProvider />
    <MudDialogProvider />
    <MudSnackbarProvider />
    @Body
</CascadingValue>
```

`CascadingValue` / `NerdMudThemeHost.CascadingName` tells the Catalog to reuse the host theme (avoids nested `MudThemeProvider` issues on Mud 9.7).

**Product-only (no live brand switch):** build theme once with `NerdMudThemeFactory.Create(options)` (see Consent / HttpBridge).  
**Studio:** inject `INerdMudThemeController` and bind `Theme="@ThemeController.CurrentTheme"`.

### 3. Catalog URL is opt-in

`AddNerdDesignTokens` only sets hub **route strings** (`CatalogRoute` default `/nerd-design-tokens`). It does **not** mount pages.

To expose the catalog:

```xml
<PackageReference Include="TheNerdCollective.MudComponents.DesignTokens" Version="2.1.2" />
<PackageReference Include="TheNerdCollective.Brand.Dnf" Version="2.0.0" />
<!-- Catalog: publish to nuget.org when ready; until then pack locally -->
<PackageReference Include="TheNerdCollective.MudComponents.DesignTokens.Catalog" Version="2.0.0" />
<PackageReference Include="TheNerdCollective.MudComponents.ResponsiveTypography" Version="1.5.0" />
<PackageReference Include="MudBlazor" Version="9.8.0" />
```

```csharp
using TheNerdCollective.Brand.Dnf;
using TheNerdCollective.MudComponents.DesignTokens;
using TheNerdCollective.MudComponents.ResponsiveTypography;

builder.Services.AddMudServices();
builder.Services.AddNerdDnfDesignSystem(
    configureTokens: o =>
    {
        o.UseIntentPseudoCssThemes = true;
        o.EnableCatalogPage = true;
        o.RestrictCatalogToDevelopment = false; // true on customer Production
    },
    configureTypography: o => o.RestrictCatalogToDevelopment = false);
builder.Services.AddNerdBrandPackIntegration(); // registers INerdBrandPackSource for Catalog
builder.Services.AddNerdDesignTokenCatalog();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddNerdDesignTokenCatalog(app.Services)
    .AddNerdDesignSystemHub(app.Services)
    .WithStaticAssets();
```

**Product hosts** (Consent): motor + theme is enough — omit Catalog unless you want an internal design hub.  
**Auth middleware:** allow `/_content`, `/_framework`, `/_blazor`, and catalog routes as public if the host gates other paths.

### 4. Mud `Color` vs token `Class`

MudBlazor [`Color`](https://mudblazor.com/api/Color#fields) is a closed set of **semantic roles** (`Primary`, `Secondary`, `Error`, …) — not brand names.

- Global chrome after DI palette map → `Color.Primary` etc. is fine  
- Extra brand / section colors → `Class="dnf-forest"` or recipe/intent classes (`TokenClass` / `RecipeClass`)

**Do not** put `MutedContent` / filled `ContrastText` intents on `Variant.Text` or `Variant.Outlined` buttons — that paints light-on-light and fails WCAG. Use `BrandChrome` (or `PrimaryAction` **Filled** only) for controls on `page-surface`.

`Class="…-brand-chrome"` on an **Outlined** control must paint the BrandChrome **accent** (dark-on-light). Never `OnBrandChrome` / ContrastText — that is for text **on** a filled BrandChrome surface (AppBar) and becomes white-on-white on page-surface. Catalog shows a red alert if generated CSS regresses to that paint.

### 5. WCAG 2.1 contrast

Design Tokens already measures contrast:

| API / UI | What it does |
|----------|----------------|
| `NerdDesignTokenTools.CheckAccessibility(options)` | Per-token light/dark ratio vs AA |
| `GetAccessibilityWarnings` / startup validator | Gate or log failures |
| Catalog token cards | Light/Dark AA chips with ratios |
| `/nerd-wcag` (`NerdWcagGuide`) | Human-readable WCAG guide |
| `NerdStyleGuardTools.ValidatePlacements` | Catalog chrome label + outlined control vs page-surface (≥ 3:1 UI) |

Set `options.WarnOnAccessibilityFailuresAtStartup = true` (default) to log WCAG failures at host start.
Set `options.FailOnAccessibilityFailuresAtStartup = true` to throw and stop the host when tokens fail AA.
CI: call `AssertAccessibilityCompliance` + `AssertPlacementCompliance` (see Brand.Dnf tests).

**Roslyn NRDT001 (enabled by default):** DesignTokens NuGet ships the analyzer + `build`/`buildTransitive` props that feed `**/*.razor` as AdditionalFiles. NRDT001 is treated as a **build error** by default (`WarningsAsErrors`). Opt out:

```xml
<!-- turn analyzer off entirely -->
<DisableNerdDesignTokenRazorContrastAnalyzer>true</DisableNerdDesignTokenRazorContrastAnalyzer>
<!-- or keep analyzer as warning only -->
<NerdDesignTokenRazorContrastTreatAsError>false</NerdDesignTokenRazorContrastTreatAsError>
```

Flags Text/Outlined + muted-content / primary-action / page-surface / **info** / **success** / **highlight** Class misuse (and DNF `flod`). Prefer BrandChrome Outlined or Filled + status intent.

---

## Brand packs (recommended)

Predefined brands ship as separate NuGet packages. **Production apps install one brand only.**

| Package | DI | Prefix |
|---------|-----|--------|
| `TheNerdCollective.Brand.Dnf` | `AddNerdDnfBrand()` / `AddNerdDnfDesignSystem()` | `dnf` |
| `TheNerdCollective.Brand.Tnc` | `AddNerdTncBrand()` | `tnc` |
| `TheNerdCollective.Brand.Acme` | `AddNerdAcmeBrand()` | `acme` |
| `TheNerdCollective.Brand.Demo` | `AddNerdDemoBrand()` | `demo` |

```csharp
using TheNerdCollective.Brand.Dnf;

builder.Services.AddNerdDnfBrand();
// or full stack (tokens + typography): AddNerdDnfDesignSystem();
```

See [docs/BRAND-PACKAGES.md](../../docs/BRAND-PACKAGES.md) for dependency diagram and multi-brand demo setup.

## Setup (manual tokens)

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

Minimal shell (prefer the checklist layout above):

```razor
<NerdDesignTokenStyles />
<NerdMudThemeProvider Theme="@theme" DesignTokenOptions="@TokenOptions" />
```

Use the customer vocabulary in markup:

```razor
<MudGrid Class="dnf-forest">
    <MudText Class="dnf-sand">Nature first</MudText>
    <MudButton Class="dnf-forest">Read more</MudButton>
</MudGrid>
```

The generator maps MudBlazor `--mud-palette-*` variables per token,
so every component that reads the theme palette inherits the token color.
Pattern-based selectors cover filled, outlined, and text variants for buttons,
chips, alerts, FABs, avatars, badges, and progress indicators. Inputs
(text fields, selects, checkboxes, radios, switches, sliders, ratings),
navigation (tabs, nav links, breadcrumbs, menus), and structural components
(tables, data grids, cards, dialogs, drawers, app bars) are included.
Descendant selectors allow applying a token on a container such as
`MudGrid` and styling all nested MudBlazor components. Hover, focus, active,
checked, selected, and disabled states are covered.

Token names must be lowercase CSS identifiers, such as `sand`,
`forest-dark`, or `sea-2`. Each application can define a different set of
tokens and a different prefix.

`ContrastText` is optional for hex colors and is calculated automatically when
omitted. `Light` and `Dark` provide mode-specific values; dark values are
activated below an ancestor with `data-theme="dark"`. `Surface`, `Content`,
and `Interactive` are semantic roles that can be consumed by application CSS
without changing the token's component selectors.

The generated selectors are versioned against the MudBlazor pin in this package
(currently **9.7**). Check CSS snapshot / inventory tests before bumping Mud.

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

See **Host checklist §3**. Short form:

```csharp
builder.Services.AddNerdDesignTokenCatalog();
app.MapRazorComponents<App>()
    .AddNerdDesignTokenCatalog(app.Services);
```

`AddNerdDesignTokenCatalog(app.Services)` registers the catalog assembly when
`EnableCatalogPage` is `true`. The default route is `/nerd-design-tokens`.

The catalog shows color swatches, light/dark preview, WCAG 2.1 AA/AAA badges
with contrast ratios, live MudBlazor component previews, aliases, radius, and
shadow tokens. Failing tokens are highlighted with warning banners and
recommended foreground colors.

Startup warnings are logged when tokens fail WCAG AA:

```csharp
options.WarnOnAccessibilityFailuresAtStartup = true;
options.WcagVersion = "2.1";
```

To use a custom route, create a host page and render the shared catalog
component (the built-in default page is at `/nerd-design-tokens`):

```razor
@page "/kunde/design-tokens"
@rendermode InteractiveServer
<NerdDesignTokensCatalog />
```

Set `options.CatalogRoute` to match your route so hub links stay correct.

**Note:** `DesignTokens.Catalog` may not yet be on nuget.org — pack from this repo (`dotnet pack …Catalog.csproj`) or use the local feed documented in host repos (e.g. nerd-rules `packages/`).
