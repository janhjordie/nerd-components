# TheNerdCollective.Brand.Acme

Sample Acme brand pack for demos and tests.

## Install

```xml
<PackageReference Include="TheNerdCollective.MudComponents.Shared" />
<PackageReference Include="TheNerdCollective.MudComponents.DesignTokens" />
<PackageReference Include="TheNerdCollective.Brand.Acme" />
```

```csharp
using TheNerdCollective.Brand.Acme;

builder.Services.AddMudServices();
builder.Services.AddNerdAcmeBrand();
```

Registers prefix `acme` with tokens `forest`, `sunrise`, `cloud`, `ink` and a `hero` recipe.

- **Identity version** — `1.0.0` (`NerdAcmeBrandPack.IdentityVersion`)
- **Pairing policy** — sample approved pairs for demos (`NerdAcmePairingPolicy`)
- **Typography** — compact dashboard scale (`NerdAcmeTypographyPresets`, catalog brand switch)

Consumer guide: [docs/BRAND-PACKAGES.md](../../docs/BRAND-PACKAGES.md)
