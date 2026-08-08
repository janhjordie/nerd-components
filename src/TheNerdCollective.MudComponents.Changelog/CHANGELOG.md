# Changelog

## 1.1.2

- NuGet republish alongside catalog auth/allowlist fixes.

## 1.1.1

- Changelog page marks `[AllowAnonymous]` for hosts with a default authorize policy.

## 1.1.0

- Search field filters title, description, version, and change type.
- Multi-select chips for major / minor / patch with “N of M” count.
- MudDataGrid Filterable + SortMode Multiple; clearer empty-filter state.
- Page subtitle shows total entry count.

## 1.0.1

- `ChangelogRoute` is overridable: public URL via options; canonical Blazor page remains `/nerd-changelog`.
- `UseNerdChangelogRouteOverride` rewrites the configured path for SSR; hosts should handle Interactive Router alias (see README).

## 1.0.0

- Initial package: `NerdChangelogService`, `NerdChangelog` MudDataGrid, `/nerd-changelog` page.
- Multi-file `changelog*.json` with 50-entry rotation helpers and semver calculation.
