Built with ❤️ for Sverre Lorenzen — en gave til hans 60-års fødselsdag.

# TheNerdCollective.Integrations.Dar

Typed .NET-klient til danske adresser og bygningsdata — bygget som erstatning for integrationer mod **DAWA**, som udfases.

Pakken bruger [Datafordeler](https://datafordeler.dk/) til struktureret adgang til **DAR** (adresser) og **BBR** (bygninger, enheder, etager m.m.) via GraphQL v3, og [adressevaelger.dk](https://adressevaelger.dk) til fri-tekst adresse-autocomplete i DAR. Adressevælger er Klimadatastyrelsens officielle erstatning for DAWA Autocomplete; Datafordeler er den nye adgang til de samme grunddata (DAR/BBR), som tidligere ofte hentedes via DAWA.

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

Sektion: **`TheNerdCollective:Dar`**.

Pakken bruger **to adgangskanaler til DAR** (Danmarks Adresseregister) — samme register, samme `id_lokalId`, men forskellige API'er:

| Kanal | Host | Config | Formål |
|---|---|---|---|
| [Datafordeler](https://datafordeler.dk/) GraphQL | `graphql.datafordeler.dk` | `ApiKey` | Struktureret DAR + BBR (opslag, bygning, etager …) |
| [Adressevælger](https://adressevaelger.dk) REST | `adressevaelger.dk` | `AutocompleteToken` | Fri-tekst søgning i DAR (DAWA-autocomplete-erstatning) |

**Adressevælger udstiller DAR** — ikke et separat register. Klimadatastyrelsen (som driver DAR) beskriver Adressevælger som et API der *«udelukkende vil tilbyde søgning i adresser og vejnavne … som det er angivet i Danmarks Adresseregister»* ([dokumentation](https://confluence.sdfi.dk/pages/viewpage.action?pageId=234782998)). Attributterne mapper direkte til DAR-entiteter (`DAR_Husnummer`, `DAR_Adresse`, `DAR_NavngivenVej` …), og opdateringer er tilgængelige dagen efter ændringer i DAR ([opdateringsfrekvens](https://confluence.sdfi.dk/pages/viewpage.action?pageId=234782998)).

DAR distribueres via flere kanaler — bl.a. Datafordeler og Dataforsyningen ([KDS](https://www.klimadatastyrelsen.dk/data/danmarks-adresseregister)). Adressevælger er KDS' **dedikerede søge-API** på eget domæne; Datafordeler GraphQL er den **strukturerede adgang** til DAR (og kryds-register til BBR m.m.). KDS anbefaler typisk: søg via Adressevælger → brug `id_lokalId` videre i Datafordeler GraphQL ([fonetisk søgning](https://confluence.sdfi.dk/pages/viewpage.action?pageId=244318431)).

**Fuld opsætning** (DAR/BBR + autocomplete):

```json
{
  "TheNerdCollective": {
    "Dar": {
      "ApiKey": "din-datafordeler-api-noegle",
      "AutocompleteToken": "adressevaelger123"
    }
  }
}
```

**Kun autocomplete** (ingen Datafordeler-nøgle):

```json
{
  "TheNerdCollective": {
    "Dar": {
      "AutocompleteToken": "adressevaelger123"
    }
  }
}
```

Nested form (`Autocomplete: { Token }`) virker også — nyttigt hvis der senere kommer flere autocomplete-indstillinger.

| Indstilling | Påkrævet | Default | Formål |
|---|---|---|---|
| `ApiKey` | Ja (for DAR/BBR) | — | Datafordeler GraphQL |
| `AutocompleteToken` | Ja (for autocomplete) | — | Adressevælger (DAR-søgning) |
| `BbrGraphQlUrl` | Nej | `https://graphql.datafordeler.dk/BBR/v3` | Override BBR-endpoint |
| `DarGraphQlUrl` | Nej | `https://graphql.datafordeler.dk/DAR/v3` | Override DAR-endpoint |

> **Om `"Token": "adressevaelger123"`**
>
> Dette er Klimadatastyrelsens **midlertidige offentlige demo-token** til Adressevælger — ikke en hemmelighed du skal ansøge om endnu.
>
> Datafordeler er **ikke klar** med rigtig brugerstyring og egne tokens til Adressevælger på nuværende tidspunkt. Ifølge [Klimadatastyrelsens dokumentation](https://confluence.sdfi.dk/pages/viewpage.action?pageId=234782998) bliver *Brugerstyring først tilgængelig efter, at DAWA er lukket*.
>
> Indtil da kan du bruge demo-tokenet `adressevaelger123`, som også fremgår af eksemplerne i [Adressevælger – fonetisk søgning](https://confluence.sdfi.dk/pages/viewpage.action?pageId=244318431). Du behøver altså **ikke** oprette en egen token endnu.
>
> Når rigtig token-udstedelse er tilgængelig, skal `AutocompleteToken` opdateres til jeres egen token.

Gem hemmeligheder i User Secrets, miljøvariabler eller `appsettings.local.json` — **ikke** i git:

```bash
export TheNerdCollective__Dar__ApiKey="din-datafordeler-api-noegle"
export TheNerdCollective__Dar__AutocompleteToken="adressevaelger123"
```

### 3. Brug

```csharp
using TheNerdCollective.Integrations.Dar;
using TheNerdCollective.Integrations.Dar.Configuration;

var options = new DarOptions
{
    ApiKey = configuration["TheNerdCollective:Dar:ApiKey"]!
};

using var httpClient = new HttpClient();
var services = DarClientFactory.Create(options, httpClient);

var adresse = await services.Dar.Adresseopslag.LookupAsync("Århusvej 69a", "3000", "Helsingør");
var bygninger = await services.Bbr.Bygning.GetAllByHusnummerIdAsync(adresse.HusnummerId);
var etager = await services.Bbr.Etage.GetByBygningIdAsync(bygninger[0].IdLokalId!);
```

**Autocomplete (uden Datafordeler-nøgle):**

```csharp
using var httpClient = new HttpClient();
var autocomplete = DarClientFactory.CreateAutocomplete(
    new DarAutocompleteOptions { Token = "adressevaelger123" },
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
builder.Services.Configure<DarOptions>(
    builder.Configuration.GetSection(DarOptions.SectionName));

builder.Services.AddHttpClient("Datafordeler");

builder.Services.AddScoped<DarServices>(sp =>
{
    var options = sp.GetRequiredService<IOptions<DarOptions>>().Value;
    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("Datafordeler");
    return DarClientFactory.Create(options, httpClient);
});
```

Minimal endpoint:

```csharp
app.MapGet("/bbr/etager", async (DarServices services, string vej, string postnr) =>
{
    var adresse = await services.Dar.Adresseopslag.LookupAsync(vej, postnr);
    var bygninger = await services.Bbr.Bygning.GetAllByHusnummerIdAsync(adresse.HusnummerId);
    var etager = new List<EtageDto>();
    foreach (var bygning in bygninger)
    {
        etager.AddRange(await services.Bbr.Etage.GetByBygningIdAsync(bygning.IdLokalId!));
    }

    return Results.Ok(new
    {
        adresse.Adgangsadresse,
        dar = adresse.Dar,
        Bygninger = bygninger.Select(b => b.Bygningsnummer),
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

## Azure Functions (.NET 8)

Eksempel med **isolated worker** (`dotnet-isolated`), DI og konfiguration via `local.settings.json` lokalt og **Application settings** i Azure.

### Pakker

```bash
dotnet add package Microsoft.Azure.Functions.Worker
dotnet add package Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore
dotnet add package Microsoft.Azure.Functions.Worker.Sdk
dotnet add package TheNerdCollective.Integrations.Dar
```

### `local.settings.json` (lokal udvikling — commit **ikke** filen)

Azure Functions læser indstillinger fra `Values`. Brug `__` som nesting-separator (svarer til `TheNerdCollective:Dar`):

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "TheNerdCollective__Dar__ApiKey": "din-datafordeler-api-noegle",
    "TheNerdCollective__Dar__AutocompleteToken": "adressevaelger123"
  }
}
```

Tilføj `local.settings.json` til `.gitignore`. Opret evt. `local.settings.json.example` med placeholders til teamet.

I **Azure Portal** (Function App → Configuration → Application settings) sættes de samme nøgler — eller brug Key Vault-references.

### `Program.cs` (DI)

```csharp
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TheNerdCollective.Integrations.Dar;
using TheNerdCollective.Integrations.Dar.Configuration;
using TheNerdCollective.Integrations.Dar.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.Configure<DarOptions>(
            context.Configuration.GetSection(DarOptions.SectionName));

        services.AddHttpClient("Datafordeler");
        services.AddHttpClient("Adressevaelger", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        services.AddScoped<DarServices>(sp =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<DarOptions>>().Value;
            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("Datafordeler");
            return DarClientFactory.Create(options, httpClient);
        });
    })
    .Build();

host.Run();
```

### HTTP-trigger eksempel

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using TheNerdCollective.Integrations.Dar.Services;

namespace MyDarFunctionApp;

public sealed class BbrLookupFunction(DarServices darServices)
{
    [Function("BbrEtager")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "bbr/etager")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        var vej = req.Query["vej"].ToString();
        var postnr = req.Query["postnr"].ToString();

        if (string.IsNullOrWhiteSpace(vej) || string.IsNullOrWhiteSpace(postnr))
        {
            return new BadRequestObjectResult("Query params 'vej' og 'postnr' er påkrævet.");
        }

        var adresse = await darServices.Dar.Adresseopslag.LookupAsync(vej, postnr, cancellationToken: cancellationToken);
        var husnummerId = adresse.Dar.Husnummer.IdLokalId!;
        var bygninger = await darServices.Bbr.Bygning.GetAllByHusnummerIdAsync(husnummerId, cancellationToken);
        var bygning = bygninger.FirstOrDefault();
        if (bygning == null)
        {
            return new NotFoundObjectResult("Ingen bygning fundet på adressen.");
        }

        var etager = await darServices.Bbr.Etage.GetByBygningIdAsync(bygning.IdLokalId!, cancellationToken);

        return new OkObjectResult(new
        {
            adresse.Adgangsadresse,
            husnummerId,
            bygning.Bygningsnummer,
            Etager = etager.Select(e => new
            {
                e.BygningensEtagebetegnelse,
                e.SamletArealAfEtage,
                e.IsKaelder
            })
        });
    }
}
```

Kald lokalt efter `func start`:

```bash
curl "http://localhost:7071/api/bbr/etager?vej=Århusvej%2069a&postnr=3000"
```

**Bemærk:** Function App'ens **udgående IP** skal whitelists i [Datafordeler Administration](https://administration.datafordeler.dk/) — brug de offentlige outbound IP'er for consumption/premium plan, eller NAT gateway / static IP på VNet-integreret app.

---

## Arkitektur

`DarClientFactory.Create()` returnerer `DarServices` — en facade opdelt efter register:

```
DarServices
├── Dar
│   ├── Autocomplete    → fri-tekst adressesøgning (Adressevælger / DAR REST)
│   ├── Adresseopslag   → native DAR-resultat + valgfri KvHxInput (DAWA legacy)
│   └── Husnummer       → native DAR husnummer uden KvHxInput
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

var husnummerId = adresse.Dar.Husnummer.IdLokalId!;
// samme UUID som DAWA id / KvHxInput.Id — brug Dar i ny kode

// 2. BBR: hent alle bygninger på adressen
var bygninger = await services.Bbr.Bygning.GetAllByHusnummerIdAsync(husnummerId);

foreach (var bygning in bygninger)
{
    var bygningId = bygning.IdLokalId!;

    // 3. Hent kun det du skal bruge per bygning
    var enheder = await services.Bbr.Enhed.GetByBygningIdAsync(bygningId);
    var etager = await services.Bbr.Etage.GetByBygningIdAsync(bygningId);
    var kaelder = etager.Where(e => e.IsKaelder);

    var grund = !string.IsNullOrWhiteSpace(bygning.Grund)
        ? await services.Bbr.Grund.GetByIdAsync(bygning.Grund)
        : null;

    var bygningRelationer = await services.Bbr.Ejendomsrelation.GetByBygningIdAsync(bygningId);
    var ejendomsrelationer = await services.Bbr.Ejendomsrelation.ResolveAsync(bygningRelationer, grund);
}
```

---

## API-reference

### DAR

#### `services.Dar.Autocomplete`

Fri-tekst adressesøgning i DAR via [Adressevælger](https://adressevaelger.dk) (Klimadatastyrelsens REST-API — samme tilgang som Voices247).

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

Returnerer `AdresseopslagResult` med bl.a. `Dar` (native DAR), `HusnummerId`, `BygningId` og valgfri `KvHxInput` (DAWA legacy).

#### `services.Dar.Husnummer`

| Metode | Returnerer |
|---|---|
| `FindByAddressAsync(streetAndNumber, postalCode, ct?)` | `HusnummerLookupResult` med native `Dar` (uden KvHxInput) |

### DAR-resultat (anbefalet)

`AdresseopslagResult.Dar` (`DarAdresseopslagDto`) er det **native DAR-resultat** fra Datafordeler — brug dette i nye integrationer:

- `Dar.Husnummer` — fuld `DAR_Husnummer`-entitet med `id_lokalId`, `adgangsadressebetegnelse`, `husnummertekst`, `status` m.m.
- `Dar.Vejnavn` — resolved vejnavn fra `DAR_NavngivenVej`
- `HusnummerId` / `Dar.Husnummer.IdLokalId` — samme UUID som tidligere DAWA `id`

Eksempel:

```json
{
  "husnummer": {
    "id_lokalId": "0a3f507b-4642-32b8-e044-0003ba298018",
    "adgangsadressebetegnelse": "Askeholm 12, 8700 Horsens",
    "husnummertekst": "12",
    "kommunekode": "0615",
    "status": "3",
    "adgangTilBygning": "..."
  },
  "vejnavn": "Askeholm"
}
```

### KvHxInput (DAWA-format, legacy)

`AdresseopslagResult.KvHxInput` mappes via Mapperly fra DAR-data og findes **kun til bagudkompatibilitet** med eksisterende downstream-systemer der forventer DAWA/KVHX-format. **Foretræk `Dar` i ny kode** — property kan fjernes når DAWA er helt udfaset.

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

`kvhxId` bygges i DAWA-format (19 tegn). Property-navnet `komunekode` følger eksisterende downstream-kontrakt. `Id` svarer til DAR `id_lokalId`.

### BBR

Alle BBR-services tager `bygningId` (`id_lokalId`) som udgangspunkt, undtagen `Grund.GetByIdAsync` og `Ejendomsrelation.ResolveAsync`.

| Service | Metode | Returnerer |
|---|---|---|
| `Bygning` | `GetByIdAsync(bygningId)` | `BygningDto` |
| `Bygning` | `GetAllByHusnummerIdAsync(husnummerId)` | `IReadOnlyList<BygningDto>` — **alle** bygninger på adressen |
| `Bygning` | `GetByHusnummerIdAsync(husnummerId)` | `BygningDto` — første bygning (legacy) |
| `Enhed` | `GetByBygningIdAsync(bygningId)` | `IReadOnlyList<EnhedDto>` |
| `Etage` | `GetByBygningIdAsync(bygningId)` | `IReadOnlyList<EtageDto>` |
| `Opgang` | `GetByBygningIdAsync(bygningId)` | `IReadOnlyList<OpgangDto>` |
| `TekniskAnlaeg` | `GetByBygningIdAsync(bygningId)` | `IReadOnlyList<TekniskAnlaegDto>` |
| `Grund` | `GetByIdAsync(grundId)` | `GrundDto` |
| `Grund` | `GetJordstykkerByGrundIdAsync(grundId)` | `IReadOnlyList<GrundJordstykkeDto>` |
| `Ejendomsrelation` | `GetByBygningIdAsync(bygningId)` | `IReadOnlyList<BygningEjendomsrelationDto>` |
| `Ejendomsrelation` | `ResolveAsync(relationer, grund?)` | `IReadOnlyList<EjendomsrelationDto>` |

`ResolveAsync` samler BFE-numre ud fra bygning-relationer og evt. `grund.bestemtFastEjendom`.

### DTO-modeller

Namespace: `TheNerdCollective.Integrations.Dar.Models`

| DTO | Beskrivelse |
|---|---|
| `DanishAddressAutocompleteResult` | Autocomplete-forslag fra Adressevælger |
| `AdresseopslagResult` | Adresseopslag med native `Dar` + valgfri `KvHxInput` (legacy) |
| `DarAdresseopslagDto` | Native DAR-resultat (`Husnummer` + `Vejnavn`) |
| `HusnummerLookupResult` | Husnummer med native `Dar` (uden KvHxInput) |
| `KvHxInputDto` | KVHX i DAWA-format (legacy bagudkompatibilitet) |
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

**Nuværende version:** `1.2.1`

Publiceres til [NuGet.org](https://www.nuget.org/packages/TheNerdCollective.Integrations.Dar) via GitHub Actions ved push til `main`.

Bump `<Version>` i `TheNerdCollective.Integrations.Dar.csproj` før release. Git-tag: `TheNerdCollective.Integrations.Dar-v{version}`.
