# TheNerdCollective.MudComponents.ThemeKit

MudBlazor components for theme switching, light/dark mode, and a simple token editor.

## Components

| Component | Purpose |
| --- | --- |
| `MudThemeProviderSync` | Binds `MudThemeProvider` to `IMudThemeStateService` (self-closing; place before page content) |
| `MudThemeToolbar` | Switcher + mode toggle + editor drawer (respects `IThemeEditorGate`) |
| `MudThemeTokenEditor` | Edit v1 design tokens (MudColorPicker + text fields) and copy C# export |

Requires `TheNerdCollective.Blazor.ThemeKit` services and a host-provided `IMudThemeCatalog`.
