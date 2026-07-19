---
title: "Brand Packages — arkitekturplan"
status: Active
author: "@janhjordie"
last_updated: "18-07-2026 16:35"
---

# Brand Packages — arkitekturplan

## Mål

Adskil **generisk design-token motor** fra **brand-specifikke presets** (DNF, TNC, Acme, Demo), så en ny MudBlazor-app kun installerer det brand den skal bruge.

**Eksempel — DNF-produktionsapp:**

```csharp
builder.Services.AddMudServices();
builder.Services.AddNerdDnfBrand(); // farver + recipes + aliases + DNF pairing
```

**Eksempel — Demo / design-system (alle brands til catalog switcher):**

```csharp
builder.Services.AddNerdDesignTokenBrandPacks(
    new NerdDnfBrandPack(),
    new NerdAcmeBrandPack(),
    new NerdDemoBrandPack(),
    new NerdTncBrandPack());
builder.Services.AddNerdDesignTokens(options =>
    NerdBrandPackRegistry.Instance.Configure("tnc", options));
```

## Pakkestruktur

| Pakke | Ansvar |
|-------|--------|
| `MudComponents.Shared` | Semantiske UI-aliases, hub, `nerd-shared.js` |
| `MudComponents.DesignTokens` | Motor: options, CSS-generator, recipes-model, validation, export |
| `MudComponents.DesignTokens.Catalog` | Valgfri catalog-UI (`/nerd-design-tokens`, recipes studio) |
| `Brand.Dnf` | DNF farver, recipes, aliases, opacity, pairing, **typography** (`AddNerdDnfTypography`) |
| `Brand.Tnc` | TNC brand preset |
| `Brand.Acme` | Acme sample preset |
| `Brand.Demo` | Demo sample preset |

Catalog (`/nerd-design-tokens`, recipes) ligger i **`DesignTokens.Catalog`** — valgfri dev-pakke. Produktionsapps behøver ikke catalog; tilføj den i Development eller interne værktøjer.

## Extension points (motor)

- `INerdBrandPack` — `Id`, `DisplayName`, `IdentityVersion`, `Configure(options)`
- `INerdPairingPolicy` — brand-specifik pairing (kun DNF i v1)
- `NerdBrandPackRegistry` — registrering + `FromPreset` + catalog brand switcher
- `NerdDesignTokenOptions.PairingPolicy` — aktiv policy efter configure

## Migreringsfaser

| Fase | Backlog | Leverance |
|------|---------|-----------|
| 1 | HR-070 | Interfaces + registry i DesignTokens |
| 2 | HR-071–074 | Flyt presets til `Brand.*` pakker |
| 3 | HR-075 | Fjern hardcoded brands fra catalogs; brug registry |
| 4 | HR-076 | Demo + tests opdateret |
| 5 | HR-076 | Consumer README / install guide |
| 6 | HR-077 | Split `DesignTokens.Catalog` som valgfri pakke |
| 7 | HR-078 | Split `ResponsiveTypography.Catalog` som valgfri pakke |
| 8 | HR-079 | DNF typography i `Brand.Dnf` + `IdentityVersion` på brand packs |
| 9 | HR-080 | Pairing policies for TNC/Acme/Demo |

## Dependency-regel

```
Brand.*  →  DesignTokens  →  Shared
Demo     →  alle Brand.* + DesignTokens + ...
DNF app  →  Brand.Dnf + DesignTokens + Shared  (IKKE TNC/Acme/Demo)
```

## Acceptkriterier (epic done)

- [x] Ingen `NerdDnf*Presets` i `DesignTokens`-assembly
- [x] `dotnet build` + offline tests grønne
- [x] Demo brand-switcher virker via registry
- [x] `AddNerdDnfBrand()` one-liner dokumenteret
- [x] E2E portal + catalog tests passerer
- [x] Consumer guide: `docs/BRAND-PACKAGES.md`
