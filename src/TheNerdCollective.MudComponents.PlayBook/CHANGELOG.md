# Changelog

## 1.2.13

- NuGet republish alongside catalog auth/allowlist fixes.

## 1.2.12

- PlayBook page marks `[AllowAnonymous]` for hosts with a default authorize policy.

## 1.2.11

- Intent inspector uses Outlined `info` (Filled painted a full-width dark green band on page-surface).

## 1.2.10

- MudTabs no longer take BrandChrome Class (it painted whole tab panels dark). Default Mud theme tabs like Colors catalog.
- Tabs matrix preview also omits chrome Class on MudTabs roots.
- Intent inspector uses token `info` Filled (no bare Severity).

## 1.2.9

- MudDrawer preview is a static MudPaper mock — real Open/Fixed drawers covered the PlayBook viewport (Playwright confirmed).

## 1.2.8

- MudDrawer preview uses nested `MudLayout` so Persistent+Open drawers no longer hijack the host shell (blank left margin / dark drawer strip on PlayBook).

## 1.2.7

- MudTabs use BrandChrome (not PrimaryAction) so active tab text/slider stay readable on page-surface.

## 1.2.6

- PlayBook uses canonical catalog ExtraLarge frame (aligned with hub / WCAG / tokens).

## 1.2.5

- Outlined hub nav BrandChrome; MudBlazor 9.8 pin.

## 1.2.4

- MudBlazor **9.8.0** package pin.


## 1.2.3

- PlayBook header uses shared catalog hub nav (aligned with Tokens/Typography/WCAG).


## 1.2.2

- Route chip `/nerd-playbook` uses `BrandChrome` Outlined instead of `Info` (`flod` cyan) so chrome on page-surface stays WCAG-readable.

## 1.2.0

- Integrated ThemeKit with `PlaybookMode`, `MudThemeToolbar`, and JSON theme persistence via `FileThemeJsonFilePersistence`.
- Shipped default `Themes/` catalog (`nerd-default`, `nerd-brand`, `playbook-sandbox`).
- Added per-component property playgrounds with live preview and configuration summary drawers.
- Playground props registry covers all 53 MudBlazor components.

## 1.0.0

- Added MudBlazor PlayBook at `/nerd-playbook` with design token matrix, typography presets, and dark mode preview.
- Added catalog of 50+ MudBlazor 9.6 components with official API documentation links.
- Added category and search filters, token selector, and hub integration.
