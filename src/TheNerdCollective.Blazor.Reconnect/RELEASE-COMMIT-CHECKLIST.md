# Release Commit Checklist

This package is published by the SDK workflow at [sdk/.github/workflows/publish-packages.yml](../../.github/workflows/publish-packages.yml) whenever a commit to `main` changes files under `src/TheNerdCollective.Blazor.Reconnect/`.

## Minimum files for a publish-triggering commit

- `src/TheNerdCollective.Blazor.Reconnect/TheNerdCollective.Blazor.Reconnect.csproj`
  - bump `<Version>` before merge so NuGet gets a new package version
- `src/TheNerdCollective.Blazor.Reconnect/wwwroot/js/blazor-reconnect.js`
  - package runtime behavior and embedded version string
- `src/TheNerdCollective.Blazor.Reconnect/README.md`
  - installation/configuration contract for consumers

## Files included for the 1.11.0 release

- `src/TheNerdCollective.Blazor.Reconnect/TheNerdCollective.Blazor.Reconnect.csproj`
- `src/TheNerdCollective.Blazor.Reconnect/wwwroot/js/blazor-reconnect.js`
- `src/TheNerdCollective.Blazor.Reconnect/README.md`
- `src/TheNerdCollective.Blazor.Reconnect/CHANGELOG.md`
- `src/TheNerdCollective.Blazor.Reconnect/RELEASE-COMMIT-CHECKLIST.md`

## BilletSalg-specific host files updated in the consumer repo

These are not part of the SDK package publish itself, but they should stay in sync with the package behavior:

- `src/ServerWeb/Components/App.razor`
- `src/ServerAdmin/Components/App.razor`
- `src/ServerWeb/ServerWeb.csproj`
- `src/ServerAdmin/ServerAdmin.csproj`
- `e2e/reconnect-ui.spec.ts`

## Quick release sanity check

- Confirm the version is bumped in the `.csproj`.
- Confirm the JS `VERSION` constant matches the package version.
- Confirm README examples still use the supported config globals.
- Confirm focused reconnect tests pass before commit.