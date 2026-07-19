# TheNerdCollective.Brand.Demo

Internal demo/sample brand pack (violet, sky, paper, slate).

## Install

```xml
<PackageReference Include="TheNerdCollective.MudComponents.Shared" />
<PackageReference Include="TheNerdCollective.MudComponents.DesignTokens" />
<PackageReference Include="TheNerdCollective.Brand.Demo" />
```

```csharp
using TheNerdCollective.Brand.Demo;

builder.Services.AddMudServices();
builder.Services.AddNerdDemoBrand();
```

Registers prefix `demo` with a `cta-strip` recipe. Used by design-system demos and pack-diff tests.

- **Identity version** — `1.0.0` (`NerdDemoBrandPack.IdentityVersion`)
- **Pairing policy** — internal demo pairs for catalog validation (`NerdDemoPairingPolicy`)
- **Typography** — marketing clamp scale (`NerdDemoTypographyPresets`, catalog brand switch)

Consumer guide: [docs/BRAND-PACKAGES.md](../../docs/BRAND-PACKAGES.md)
