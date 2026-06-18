# TheNerdCollective.Blazor.ThemeKit

Theme catalog contracts, runtime state, preferences, and environment gating for MudBlazor apps.

## Registration

```csharp
services.AddSingleton<IMudThemeCatalog, MyThemeCatalog>();
services.AddMudThemeKit(options =>
{
    options.DefaultThemeId = "my-default";
    options.PlaybookMode = false;
});
```

## Configuration

| Key | Description |
| --- | --- |
| `Ui:ThemeEditor:Enabled` | Force-enable editor outside Development |
| `Ui:ThemeEditor:AllowSwitcherInProduction` | Show switcher in production |

Editor is also enabled in `Development`, `Test`, and `Staging` environments.

## JSON export (Playbook)

Playbook stores a local catalog under `src/Storybook/Themes/`:

- `themes.index.json` — theme list with version and `updatedAt`
- `*.theme.json` — v1 token dictionary

Use **Copy JSON export** in the token editor, save the file, update the index, and restart Playbook.
