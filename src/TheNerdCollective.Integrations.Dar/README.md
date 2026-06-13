# TheNerdCollective.Integrations.Dar

Typed .NET-klient til **DAR** (adresser) og **BBR** (bygninger, enheder, etager m.m.) via [Datafordeler](https://datafordeler.dk/) GraphQL v3.

Pakken targeter **.NET Standard 2.0** og kan bruges fra .NET Framework 4.6.1+ og moderne .NET.

## Hvad kan pakken?

- **Adresse-autocomplete** via Adressevælger (fri-tekst dansk adressesøgning)
- **Adresseopslag** i DAR med KVHX/DAWA-format output
- **BBR-data** via separate services (bygning, enheder, etager, opgange, grund, ejendomsrelationer)
- **Lazy by design** — kald kun de services du har brug for; intet hentes automatisk
- **Typed DTO'er** med fleksibel JSON-deserialisering mod Datafordeler

---

## Quick start

### 1. Installer

```bash
dotnet add package TheNerdCollective.Integrations.Dar
```

### 2. Konfigurer

**DAR/BBR (Datafordeler GraphQL)** — kun API-nøgle påkrævet:

```json
{
  "TheNerdCollective": {
    "Dar": {
      "ApiKey": "din-datafordeler-api-noegle",
      "Adressevaelger": {
        "BaseUrl": "https://adressevaelger.dk",
        "Token": "adressevaelger123"
      }
    }
  }
}
```

| Indstilling | Påkrævet | Default | Formål |
|---|---|---|---|
| `ApiKey` | Ja (for DAR/BBR) | — | Datafordeler GraphQL |
| `BbrGraphQlUrl` | Nej | `https://graphql.datafordeler.dk/BBR/v3` | Override BBR-endpoint |
| `DarGraphQlUrl` | Nej | `https://graphql.datafordeler.dk/DAR/v3` | Override DAR-endpoint |
| `Adressevaelger:Token` | Ja (for autocomplete) | — | Adressevælger REST API |
| `Adressevaelger:BaseUrl` | Nej | `https://adressevaelger.dk` | Override autocomplete-endpoint |

Konfigurationssektion: **`TheNerdCollective:Dar`**.

Autocomplete kan bruges **uden** Datafordeler API-nøgle — kun `Adressevaelger:Token` er nødvendig.

> **Om `"Token": "adressevaelger123"`**
>
> Dette er Klimadatastyrelsens **midlertidige offentlige demo-token** til [Adressevælger](https://adressevaelger.dk) — ikke en hemmelighed du skal ansøge om endnu.
>
> Datafordeler er **ikke klar** med rigtig brugerstyring og egne tokens til Adressevælger på nuværende tidspunkt. Ifølge [Klimadatastyrelsens dokumentation](https://confluence.sdfi.dk/pages/viewpage.action?pageId=234782998) bliver *Brugerstyring først tilgængelig efter, at DAWA er lukket*.
>
> Indtil da kan du bruge demo-tokenet `adressevaelger123`, som også fremgår af eksemplerne i [Adressevælger – fonetisk søgning](https://confluence.sdfi.dk/pages/viewpage.action?pageId=244318431). Du behøver altså **ikke** oprette en egen token hos Datafordeler endnu — sæt blot demo-tokenet i config (eller lad TestWeb-defaults stå).
>
> Når rigtig token-udstedelse er tilgængelig, skal `Adressevaelger:Token` opdateres til jeres egen token; API'et forventes at fungere på samme måde.

Gem hemmeligheder i User Secrets, miljøvariabler eller `appsettings.local.json` — **ikke** i git:

```bash
export TheNerdCollective__Dar__ApiKey="din-datafordeler-api-noegle"
export TheNerdCollective__Dar__Adressevaelger__Token="adressevaelger123"
```

### 3. Brug

```csharp
using TheNerdCollective.Integrations.Dar;
using TheNerdCollective.Integrations.Dar.Configuration;

var options = new DatafordelerOptions
{
    ApiKey = configuration["TheNerdCollective:Dar:ApiKey"]!
};

using var httpClient = new HttpClient();
var services = DarClientFactory.Create(options, httpClient);

var adresse = await services.Dar.Adresseopslag.LookupAsync("Århusvej 69a", "3000", "Helsingør");
var bygning = await services.Bbr.Bygning.GetByHusnummerIdAsync(adresse.HusnummerId);
var etager = await services.Bbr.Etage.GetByBygningIdAsync(bygning.IdLokalId!);
```

**Autocomplete (uden Datafordeler-nøgle):**

```csharp
using var httpClient = new HttpClient();
var autocomplete = DarClientFactory.CreateAutocomplete(
    new AdressevaelgerOptions { Token = "adressevaelger123" },
    httpClient);

var forslag = await autocomplete.SearchAsync("Århusvej 69");
foreach (var adresse in forslag)
{
    Console.WriteLine($"{adresse.DisplayName} ({adresse.ResultType})");
}
```

---

## Forudsætninger

- Datafordeler API-nøgle med adgang til DAR og BBR
- Serverens **udgående IP** skal være whitelisted i [Datafordeler Administration](https://administration.datafordeler.dk/)

---

## ASP.NET Core

```csharp
builder.Services.Configure<DatafordelerOptions>(
    builder.Configuration.GetSection(DatafordelerOptions.SectionName));

builder.Services.AddHttpClient("Datafordeler");

builder.Services.AddScoped<DarServices>(sp =>
{
    var options = sp.GetRequiredService<IOptions<DatafordelerOptions>>().Value;
    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("Datafordeler");
    return DarClientFactory.Create(options, httpClient);
});
```

Minimal endpoint:

```csharp
app.MapGet("/bbr/etager", async (DarServices services, string vej, string postnr) =>
{
    var adresse = await services.Dar.Adresseopslag.LookupAsync(vej, postnr);
    var bygning = await services.Bbr.Bygning.GetByHusnummerIdAsync(adresse.HusnummerId);
    var etager = await services.Bbr.Etage.GetByBygningIdAsync(bygning.IdLokalId!);

    return Results.Ok(new
    {
        adresse.Adgangsadresse,
        kvHx = adresse.KvHxInput,
        bygning.Bygningsnummer,
        Etager = etager.Select(e => new
        {
            e.BygningensEtagebetegnelse,
            e.SamletArealAfEtage,
            e.IsKaelder
        })
    });
});
```

---

## Arkitektur

`DarClientFactory.Create()` returnerer `DarServices` — en facade opdelt efter register:

```
DarServices
├── Dar
│   ├── Autocomplete    → fri-tekst adressesøgning (Adressevælger)
│   ├── Adresseopslag   → fuldt DAR-opslag + KvHxInput
│   └── Husnummer       → DAR husnummer uden KvHxInput
└── Bbr
    ├── Bygning
    ├── Enhed
    ├── Etage
    ├── Opgang
    ├── TekniskAnlaeg
    ├── Grund
    └── Ejendomsrelation
```

---

## Typisk flow

```csharp
// 1. DAR: adresseopslag
var adresse = await services.Dar.Adresseopslag.LookupAsync("Århusvej 69a", "3000", "Helsingør");
// eller: await services.Dar.Adresseopslag.LookupAsync("Århusvej 69a, 3000 Helsingør");

var kvHx = adresse.KvHxInput;
// fx kvHx.KvhxId == "02179781__69A______"

// 2. BBR: hent bygning
var bygning = !string.IsNullOrWhiteSpace(adresse.BygningId)
    ? await services.Bbr.Bygning.GetByIdAsync(adresse.BygningId)
    : await services.Bbr.Bygning.GetByHusnummerIdAsync(adresse.HusnummerId);

var bygningId = bygning.IdLokalId!;

// 3. Hent kun det du skal bruge
var enheder = await services.Bbr.Enhed.GetByBygningIdAsync(bygningId);
var etager = await services.Bbr.Etage.GetByBygningIdAsync(bygningId);
var kaelder = etager.Where(e => e.IsKaelder);

var grund = !string.IsNullOrWhiteSpace(bygning.Grund)
    ? await services.Bbr.Grund.GetByIdAsync(bygning.Grund)
    : null;

var bygningRelationer = await services.Bbr.Ejendomsrelation.GetByBygningIdAsync(bygningId);
var ejendomsrelationer = await services.Bbr.Ejendomsrelation.ResolveAsync(bygningRelationer, grund);
```

---

## API-reference

### DAR

#### `services.Dar.Autocomplete`

Fri-tekst adressesøgning via [Adressevælger](https://adressevaelger.dk) (samme tilgang som Voices247).

| Metode | Beskrivelse |
|---|---|
| `SearchAsync(searchText, ct?)` | Returnerer op til 10 autocomplete-forslag |

Returnerer `DanishAddressAutocompleteResult` med bl.a. `DisplayName`, `AddressLine1`, `PostalCode`, `City`, `IsCompleteAddress` og `LocalId`.

Typisk brug i UI: debounce 300 ms, minimum 2 tegn, vis kun forslag hvor `IsCompleteAddress == true` hvis adressen skal bruges til videre DAR/BBR-opslag.

#### `services.Dar.Adresseopslag`

| Metode | Beskrivelse |
|---|---|
| `LookupAsync(streetAndNumber, postalCode, city?, ct?)` | Opslag på vej/husnummer + postnummer |
| `LookupAsync(fullAddress, ct?)` | Opslag på fuld adresse, fx `"Århusvej 69a, 3000 Helsingør"` |

Returnerer `AdresseopslagResult` med bl.a. `Adgangsadresse`, `HusnummerId`, `BygningId` og `KvHxInput`.

#### `services.Dar.Husnummer`

| Metode | Returnerer |
|---|---|
| `FindByAddressAsync(streetAndNumber, postalCode, ct?)` | `HusnummerLookupResult` (uden KvHxInput) |

### BBR

Alle BBR-services tager `bygningId` (`id_lokalId`) som udgangspunkt, undtagen `Grund.GetByIdAsync` og `Ejendomsrelation.ResolveAsync`.

| Service | Metode | Returnerer |
|---|---|---|
| `Bygning` | `GetByIdAsync(bygningId)` | `BygningDto` |
| `Bygning` | `GetByHusnummerIdAsync(husnummerId)` | `BygningDto` |
| `Enhed` | `GetByBygningIdAsync(bygningId)` | `IReadOnlyList<EnhedDto>` |
| `Etage` | `GetByBygningIdAsync(bygningId)` | `IReadOnlyList<EtageDto>` |
| `Opgang` | `GetByBygningIdAsync(bygningId)` | `IReadOnlyList<OpgangDto>` |
| `TekniskAnlaeg` | `GetByBygningIdAsync(bygningId)` | `IReadOnlyList<TekniskAnlaegDto>` |
| `Grund` | `GetByIdAsync(grundId)` | `GrundDto` |
| `Grund` | `GetJordstykkerByGrundIdAsync(grundId)` | `IReadOnlyList<GrundJordstykkeDto>` |
| `Ejendomsrelation` | `GetByBygningIdAsync(bygningId)` | `IReadOnlyList<BygningEjendomsrelationDto>` |
| `Ejendomsrelation` | `ResolveAsync(relationer, grund?)` | `IReadOnlyList<EjendomsrelationDto>` |

`ResolveAsync` samler BFE-numre ud fra bygning-relationer og evt. `grund.bestemtFastEjendom`.

### KvHxInput (DAWA-format)

`AdresseopslagResult.KvHxInput` mappes via Mapperly fra DAR-data og er klar til downstream-systemer.

Eksempel for **Askeholm 12, 8700 Horsens**:

```json
{
  "adressebetegnelse": "Askeholm 12, 8700 Horsens",
  "esrejendomsnr": "0",
  "husnummer": "12",
  "id": "0a3f50bb-33f7-32b8-e044-0003ba298018",
  "komunekode": "0615",
  "kvhxId": "06150330__12_______",
  "postnummer": "8700",
  "vejkode": "0330",
  "vejnavn": "Askeholm"
}
```

`kvhxId` bygges i DAWA-format (19 tegn). Property-navnet `komunekode` følger eksisterende downstream-kontrakt.

### DTO-modeller

Namespace: `TheNerdCollective.Integrations.Dar.Models`

| DTO | Beskrivelse |
|---|---|
| `DanishAddressAutocompleteResult` | Autocomplete-forslag fra Adressevælger |
| `AdresseopslagResult` | Adresseopslag inkl. `KvHxInput` |
| `HusnummerLookupResult` | Husnummer uden KvHxInput |
| `KvHxInputDto` | KVHX i DAWA-format |
| `HusnummerDto` | DAR husnummer |
| `BygningDto` | Bygning, arealer, grund-reference |
| `EnhedDto` | Boliger/enheder |
| `EtageDto` | Etager (`IsKaelder` for kælder) |
| `OpgangDto` | Opgange |
| `TekniskAnlaegDto` | Tekniske anlæg |
| `GrundDto` | Grund inkl. BFE via `bestemtFastEjendom` |
| `GrundJordstykkeDto` | Jordstykker |
| `BygningEjendomsrelationDto` | Bygning-ejendomsrelationer |
| `EjendomsrelationDto` | Ejendomsrelationer med BFE-nummer |
| `DarEntityDto` | Fælles metadata |
| `KoordinatDto` | Koordinater (WKT) |

---

## Fejlhåndtering

```csharp
using TheNerdCollective.Integrations.Dar.GraphQL;

try
{
    var adresse = await services.Dar.Adresseopslag.LookupAsync(vej, postnr);
}
catch (DatafordelerApiException ex) when (ex.ResponseBody.Contains("DAF-AUTH-0005"))
{
    // IP ikke whitelisted i Datafordeler Administration
}
catch (InvalidOperationException)
{
    // Adresse ikke fundet, manglende BBR-data eller manglende vejmidte
}
```

| Fejl | Betydning |
|---|---|
| `DAF-AUTH-0005` | Udgående IP ikke whitelisted |
| `InvalidOperationException` | Adresse ikke fundet eller manglende data |
| `DatafordelerApiException` | Øvrige HTTP/GraphQL-fejl fra Datafordeler |

---

## BBR-kodeværdier

Mange felter returnerer kodeværdier (fx `BygningensAnvendelse`, `Etagetype`). Se [datafordeler.dk](https://datafordeler.dk/) for officielle kodelister.

`EtageDto.IsKaelder` håndterer kælder (`Etagetype` `1`/`2`).

---

## Udvikling i dette repo

| Projekt | Formål |
|---|---|
| `src/TheNerdCollective.Integrations.Dar` | NuGet-pakken |
| `src/TheNerdCollective.Integrations.Dar.TestWeb` | Blazor test-UI |
| `tests/TheNerdCollective.Integrations.Dar.IntegrationTests` | Integrationstests |

### Lokal ProjectReference

```xml
<ProjectReference Include="..\TheNerdCollective.Integrations.Dar\TheNerdCollective.Integrations.Dar.csproj" />
```

### Pak lokalt

```bash
dotnet pack src/TheNerdCollective.Integrations.Dar/TheNerdCollective.Integrations.Dar.csproj -c Release -o artifacts
```

### TestWeb

```bash
./src/TheNerdCollective.Integrations.Dar.TestWeb/run.sh
# eller på Windows: run.bat
```

Åbn **http://localhost:5095**.

Opsæt API-nøgle:

```bash
cp src/TheNerdCollective.Integrations.Dar.TestWeb/appsettings.local.json.example \
   src/TheNerdCollective.Integrations.Dar.TestWeb/appsettings.local.json
```

`appsettings.local.json` er i `.gitignore` og committes ikke.

TestWeb kalder alle services i én operation (adresseopslag → husnummer → bygning → enheder → etager → opgange → tekniske anlæg → grund → ejendomsrelationer). Standard testadresse: **Århusvej 69a, 3000 Helsingør**.

### Integrationstests

```bash
dotnet test tests/TheNerdCollective.Integrations.Dar.IntegrationTests
```

API-nøgle læses i prioriteret rækkefølge:

1. `TheNerdCollective__Dar__ApiKey` eller `DATAFORDELER_API_KEY`
2. `appsettings.local.json` i TestWeb
3. Fallback i `IntegrationTestEnvironment`

Kræver whitelisted IP — ellers springes testen over ved `DAF-AUTH-0005`.

---

## Versionering

**Nuværende version:** `1.0.0`

Publiceres til [NuGet.org](https://www.nuget.org/packages/TheNerdCollective.Integrations.Dar) via GitHub Actions ved push til `main`.

Bump `<Version>` i `TheNerdCollective.Integrations.Dar.csproj` før release. Git-tag: `TheNerdCollective.Integrations.Dar-v{version}`.
