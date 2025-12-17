Important Copilot Instructions

- Do not create new terminals. If an operation fails due to a port conflict or "address already in use" message, this indicates another terminal/process is already running and using the port.
- If you encounter such an error, do NOT attempt to spawn or kill processes yourself. Instead, immediately notify me with the exact error message so I can stop the existing process manually.

## Trusted Publishing Reminder

**CRITICAL: All NuGet package publishing MUST use Trusted Publishing (OIDC) via GitHub Actions.**

### ✅ CORRECT Approach (ALWAYS USE THIS):
```yaml
- name: Get OIDC Token
  id: oidc
  uses: actions/github-script@v7
  with:
    script: |
      const token = await core.getIDToken('https://api.nuget.org/v3/index.json');
      core.setSecret(token);
      core.setOutput('token', token);

- name: Push to NuGet
  env:
    NUGET_TOKEN: ${{ steps.oidc.outputs.token }}
  run: |
    for nupkg in artifacts/*.nupkg; do
      dotnet nuget push "$nupkg" -s https://api.nuget.org/v3/index.json -k "$NUGET_TOKEN" --skip-duplicate
    done
```

### ❌ NEVER use these approaches:
- `NuGet/login@v1` action - incompatible with OIDC
- `${{ secrets.NUGET_PUBLISH_KEY }}` as NUGET_AUTH_TOKEN environment variable
- Any hardcoded API keys in workflows
- `--api-key` parameter without the `-s` source parameter

### Key Points:
- Use `actions/github-script@v7` to get the OIDC token from GitHub
- Pass token as `-k "$NUGET_TOKEN"` parameter to `dotnet nuget push`
- Always use `-s https://api.nuget.org/v3/index.json` (v3 API, not v2)
- Include `--skip-duplicate` to handle re-runs gracefully
- This works for ALL new packages/workflows without any secrets needed

### ⚠️ CRITICAL: NuGet.config Must Exist
**The repository MUST have a `NuGet.config` at the root that forces v3 API:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
```
GitHub Actions runners have a default NuGet.config that uses v2 API. This file overrides it to ensure v3 is used. **WITHOUT THIS FILE, PUBLISHING WILL FAIL with 403 Forbidden errors.**

## Release Workflows

When user says "release MudComponent", "release Services", "release Components", or "release Helpers", execute the following workflow:

### Release MudComponent (TheNerdCollective.MudComponents.MudQuillEditor)
1. Bump version in: `src/TheNerdCollective.MudComponents.MudQuillEditor/TheNerdCollective.MudComponents.MudQuillEditor.csproj`
2. Commit: `git commit -m "chore: bump MudQuillEditor to vX.Y.Z - <description>"`
3. Create tag: `git tag -a vX.Y.Z -m "Release vX.Y.Z - <description>"`
4. Push: `git push origin main && git push origin vX.Y.Z`
5. Build: `cd src/TheNerdCollective.MudComponents.MudQuillEditor && dotnet pack -c Release`
6. Publish: `dotnet nuget push bin/Release/TheNerdCollective.MudComponents.X.Y.Z.nupkg --source https://api.nuget.org/v3/index.json` (uses trusted publishing)

### Release Services (TheNerdCollective.Services)
1. Bump version in: `src/TheNerdCollective.Services/TheNerdCollective.Services.csproj`
2. Commit: `git commit -m "chore: bump Services to vX.Y.Z - <description>"`
3. Create tag: `git tag -a services-vX.Y.Z -m "Release vX.Y.Z - <description>"`
4. Push: `git push origin main && git push origin services-vX.Y.Z`
5. Build: `cd src/TheNerdCollective.Services && dotnet pack -c Release`
6. Publish: `dotnet nuget push bin/Release/TheNerdCollective.Services.X.Y.Z.nupkg --source https://api.nuget.org/v3/index.json` (uses trusted publishing)

### Release Components (TheNerdCollective.Components)
1. Bump version in: `src/TheNerdCollective.Components/TheNerdCollective.Components.csproj`
2. Commit: `git commit -m "chore: bump Components to vX.Y.Z - <description>"`
3. Create tag: `git tag -a components-vX.Y.Z -m "Release vX.Y.Z - <description>"`
4. Push: `git push origin main && git push origin components-vX.Y.Z`
5. Build: `cd src/TheNerdCollective.Components && dotnet pack -c Release`
6. Publish: `dotnet nuget push bin/Release/TheNerdCollective.Components.X.Y.Z.nupkg --source https://api.nuget.org/v3/index.json` (uses trusted publishing)

### Release Helpers (TheNerdCollective.Helpers)
1. Bump version in: `src/TheNerdCollective.Helpers/TheNerdCollective.Helpers.csproj`
2. Commit: `git commit -m "chore: bump Helpers to vX.Y.Z - <description>"`
3. Create tag: `git tag -a helpers-vX.Y.Z -m "Release vX.Y.Z - <description>"`
4. Push: `git push origin main && git push origin helpers-vX.Y.Z`
5. Build: `cd src/TheNerdCollective.Helpers && dotnet pack -c Release`
6. Publish: `dotnet nuget push bin/Release/TheNerdCollective.Helpers.X.Y.Z.nupkg --source https://api.nuget.org/v3/index.json` (uses trusted publishing)

**Note:** Replace X.Y.Z with actual version number and <description> with meaningful release description. Always ask user for the version and description if not provided.
