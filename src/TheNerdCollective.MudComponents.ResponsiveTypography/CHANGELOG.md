# Changelog

## 1.3.0

- Typography catalog uses shared `NerdCatalogThemeProvider` and `NerdDesignSystemScripts`.
- Default catalog route moved to `Pages/NerdTypographyPage.razor` for custom-route support.
- Hub links sync `CatalogRoute` at DI registration time.

## 1.2.0

- Added `TheNerdCollective.MudComponents.Shared` dependency for clipboard copy and design-system hub links.
- Added `NerdClampEvaluator` for viewport-based `clamp()` size previews in the catalog.
- Added `NerdTypographyPresets` (`Marketing`, `DenseApp`) and global `LineHeight`, `LetterSpacing`, `FontWeight`.
- Catalog shows viewport slider (320–1920px), configured-role filter, computed sizes, and copy-to-clipboard.
- `AddNerdResponsiveTypographyCatalog` registers the catalog assembly at `/nerd-typography`.
- `AddNerdResponsiveTypography` registers a `MudTheme` singleton for catalog previews.
- Extended WCAG checks for line-height and letter-spacing guidance.

## 1.1.0

- Added visual typography catalog at `/nerd-typography` (configurable).
- Added WCAG 2.1 resize and minimum-size badges with startup warnings.
- Added `AddNerdResponsiveTypography` and optional `AddNerdResponsiveTypographyCatalog(app.Services)`.

## 1.0.0

- Added CSS-native responsive typography for MudBlazor 9.6 themes.
- Added `ResponsiveFontSize.Clamp`.
- Added support for all MudBlazor typography roles.
- Added XML documentation and input validation.
