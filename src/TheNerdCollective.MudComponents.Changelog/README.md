# TheNerdCollective.MudComponents.Changelog

MudBlazor viewer for multi-file product changelogs (`changelog.json`, `changelog-1.json`, …).

Agents update the JSON via the **`update changelog`** skill / `NR_CHANGELOG_*` rules.
This package is **read-only** for the UI. **Never** put PII in changelog JSON (`NR_CHANGELOG_NO_PII`).

## UI (1.1.0+)

`<NerdChangelog />` includes search, major/minor/patch filter chips, sortable/filterable MudDataGrid, and an “N of M” count.

## Setup

```csharp
builder.Services.AddNerdChangelog(options =>
{
    options.DataDirectory = Path.Combine(builder.Environment.ContentRootPath, "data");
    options.MaxEntriesPerFile = 50;
    // Public URL (hub / AppBar / auth). Canonical Blazor @page stays /nerd-changelog.
    options.ChangelogRoute = "/changelog";
});

var app = builder.Build();
app.UseNerdChangelogRouteOverride(); // rewrite configured route → /nerd-changelog (SSR)

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddNerdChangelog(app.Services);
```

Ensure `data/changelog*.json` is copied to the output directory.

### Interactive Router alias

Blazor Interactive routing uses the browser URL. When `ChangelogRoute` ≠ `/nerd-changelog`,
handle `Router` `NotFound` (or navigate) by rendering `<NerdChangelogPage />` when
`NerdChangelogEndpointExtensions.IsChangelogRouteAlias(path, options)` is true.

## JSON schema

```json
[
  {
    "changeType": "major|minor|patch",
    "date": "YYYY-MM-DD",
    "time": "HH:MM",
    "title": "Short headline",
    "description": "1–3 sentences summarizing the delta since the previous entry"
  }
]
```

Newest entry at index 0. Max **50** entries per file; then create `changelog-{N+1}.json`.

## Routes

| Path | Role |
|------|------|
| `/nerd-changelog` | Canonical Blazor `@page` (always) |
| `options.ChangelogRoute` | Public URL — default `/nerd-changelog`, overridable |

Embed `<NerdChangelog />` anywhere DI is registered.
