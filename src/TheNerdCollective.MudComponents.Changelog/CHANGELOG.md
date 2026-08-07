# Changelog

## 1.0.1

- `ChangelogRoute` is overridable: public URL via options; canonical Blazor page remains `/nerd-changelog`.
- `UseNerdChangelogRouteOverride` rewrites the configured path for SSR; hosts should handle Interactive Router alias (see README).

## 1.0.0

- Initial package: `NerdChangelogService`, `NerdChangelog` MudDataGrid, `/nerd-changelog` page.
- Multi-file `changelog*.json` with 50-entry rotation helpers and semver calculation.
