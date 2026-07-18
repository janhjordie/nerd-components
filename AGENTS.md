# AGENTS.md

## Cursor Cloud specific instructions

This repo (`TheNerdCollective.Components`) is a **.NET 10** monorepo of ~25 Blazor/MudBlazor NuGet libraries plus a couple of runnable Blazor Server host apps. There is no database or long-running backend service; the only "services" are the demo/test host apps and some external Danish-government APIs used by the `Integrations.Dar` package.

### Toolchain / environment
- The .NET 10 SDK is installed at `~/.dotnet` (not via the system package manager). `~/.bashrc` already puts it on `PATH` and sets `DOTNET_ROOT`, so interactive shells get `dotnet` automatically. Non-interactive scripts that don't source `.bashrc` should call `"$HOME/.dotnet/dotnet"` or export `PATH="$HOME/.dotnet:$PATH"` first.
- The update script runs `dotnet restore TheNerdCollective.Components.sln` (and guard-installs the SDK if `~/.dotnet` is missing). Standard build/test/run commands are in `README.md` and `CONTRIBUTING.md`.

### Build / test / run (see CONTRIBUTING.md for the canonical list)
- Build: `dotnet build TheNerdCollective.Components.sln`
- Test: `dotnet test TheNerdCollective.Components.sln`
- There is no separate lint step; the build itself emits many `CS1591` (missing XML doc) and `NU1902` (transitive AngleSharp advisory in a test project) **warnings**. These are pre-existing and non-fatal — the solution builds with 0 errors.

### Non-obvious gotchas
- **Dar integration tests hit LIVE external APIs.** `tests/TheNerdCollective.Integrations.Dar.IntegrationTests` calls Datafordeler GraphQL, which requires the caller's outbound IP to be whitelisted. From a cloud VM these tests **fail with `DAF-AUTH-0005` (IP not allowlisted)** or self-skip. To run only the offline suite: `dotnet test TheNerdCollective.Components.sln --filter "FullyQualifiedName!~IntegrationTests"` (78 tests, all pass).
- **The `TheNerdCollective.Demo` app currently fails to start** with `System.InvalidOperationException: Assembly already defined.` This is a pre-existing code defect (not an environment issue): `Program.cs` chains `AddNerdDesignTokenCatalog` / `AddNerdResponsiveTypographyCatalog` / `AddNerdPlayBook` / `AddNerdDesignSystemHub`, and each internally calls `AddNerdDesignSystemAssets()` → `AddAdditionalAssemblies(Shared.dll)`, so the Shared assembly gets registered multiple times, which .NET 10 rejects. To run the demo you must dedupe those `AddAdditionalAssemblies` registrations first.
- **Use the working Blazor Server host for smoke tests:** `TheNerdCollective.Integrations.Dar.TestWeb` starts cleanly. Run it with `dotnet run --project src/TheNerdCollective.Integrations.Dar.TestWeb/TheNerdCollective.Integrations.Dar.TestWeb.csproj --launch-profile http` (serves on `http://localhost:5095`). The `/adresse` page's address autocomplete works end-to-end without any API key because it uses the free public Adressevælger API (`adressevaelger.dk`). Anything that needs the Datafordeler GraphQL data (BBR detail lookups, the Kommune/Postnummer GraphQL paths) needs a whitelisted IP + API key and will surface `DAF-AUTH-0005` otherwise.
- Prefer the `http` launch profile when running host apps in the VM; the `https` profile needs a trusted dev certificate.
