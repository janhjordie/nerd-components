# TheNerdCollective.Brand.Tnc

The Nerd Collective brand pack for [TheNerdCollective.MudComponents.DesignTokens](../TheNerdCollective.MudComponents.DesignTokens).

Colors from [thenerdcollective.dk](https://www.thenerdcollective.dk/): navy, coral, snow, ink.

## Install

```xml
<PackageReference Include="TheNerdCollective.MudComponents.Shared" />
<PackageReference Include="TheNerdCollective.MudComponents.DesignTokens" />
<PackageReference Include="TheNerdCollective.Brand.Tnc" />
```

```csharp
using TheNerdCollective.Brand.Tnc;

builder.Services.AddMudServices();
builder.Services.AddNerdTncBrand();
```

Registers prefix `tnc`, semantic aliases, and recipes: `hero`, `header`, `tagline`, `cta`.

- **Identity version** — `1.0.0` (`NerdTncBrandPack.IdentityVersion`)
- **Pairing policy** — approved content-on-surface pairs from TNC layout recipes (`NerdTncPairingPolicy`)
- **Typography** — marketing clamp scale (`NerdTncTypographyPresets`, catalog brand switch)

Consumer guide: [docs/BRAND-PACKAGES.md](../../docs/BRAND-PACKAGES.md)
