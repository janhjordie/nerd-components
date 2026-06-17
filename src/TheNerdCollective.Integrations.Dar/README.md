Built with ❤️ for Sverre Lorenzen — en gave til hans 60-års fødselsdag.

# TheNerdCollective.Integrations.Dar

Typed .NET-klient til danske adresser og bygningsdata — bygget som erstatning for integrationer mod **DAWA**, som udfases.

Pakken bruger [Datafordeler](https://datafordeler.dk/) til struktureret adgang til **DAR** (adresser) og **BBR** (bygninger, enheder, etager m.m.) via GraphQL v3, og [adressevaelger.dk](https://adressevaelger.dk) til fri-tekst adresse-autocomplete i DAR. Adressevælger er Klimadatastyrelsens officielle erstatning for DAWA Autocomplete; Datafordeler er den nye adgang til de samme grunddata (DAR/BBR), som tidligere ofte hentedes via DAWA.

Pakken targeter **.NET Standard 2.0** og kan bruges fra .NET Framework 4.6.1+ og moderne .NET.

## Hvad kan pakken?

- **Adresse-autocomplete** via Adressevælger (fri-tekst dansk adressesøgning)
- **Adressedetaljer efter autocomplete** — koordinater (WGS84 + ETRS89), kommune m.m. via Adressevælger id-opslag
- **Adresseopslag** i DAR med KVHX/DAWA-format output
- **Kommuner (DAGI)** — liste af alle kommuner og opslag ud fra WGS84-koordinater (`GetAllAsync`, `FindByCoordinatesAsync`); **DAWA-fallback** mens Datafordeler DAGI GraphQL returnerer tomme resultater
- **Postnumre (DAR)** — aktive postnumre, filtrering på kommunekode og batch-opslag med kommune-metadata (`GetAllActiveAsync`, `GetByMunicipalityCodeAsync`, `GetByPostalCodesAsync`)
- **BBR-data** via separate services (bygning, enheder, etager, opgange, grund, ejendomsrelationer)
- **Lazy by design** — kald kun de services du har brug for; intet hentes automatisk
- **Typed DTO'er** med fleksibel JSON-deserialisering mod Datafordeler
- **Azure Functions guide** med HTTP API og Postman collection — se [AZURE-FUNCTIONS-API.md](./AZURE-FUNCTIONS-API.md)

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
| [Adressevælger](https://adressevaelger.dk) REST | `adressevaelger.dk` | `AutocompleteToken` | Fonetisk søgning (`/husnumre/soeg`, evt. `/adresser/soeg`) + id-opslag via `/husnumre/{id}` |

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

**Autocomplete med koordinater (uden Datafordeler-nøgle):**

```csharp
using var httpClient = new HttpClient();
var autocomplete = DarClientFactory.CreateAutocomplete(
    new DarAutocompleteOptions { Token = "adressevaelger123" },
    httpClient);

// 1. Fonetisk søgning
var forslag = await autocomplete.SearchAsync("Århusvej 69a 3000");
var valgt = forslag.First(f => f.IsCompleteAddress);

// 2. Id-opslag på valgt forslag — koordinater + DAR husnummer-id
var detaljer = await autocomplete.GetDetailsAsync(valgt);

Console.WriteLine($"{detaljer.Betegnelse}: {detaljer.Latitude}, {detaljer.Longitude}");
Console.WriteLine($"HusnummerId (til DAR/BBR): {detaljer.HusnummerId}");
```

`GetDetailsAsync` virker for autocomplete-typerne `husnummer` og `adresse` (ikke vejnavn). Id-opslag går **altid** via `GET /husnumre/{id}` — for type `adresse` bruges `HusnummerId` fra søgeresultatet, ikke `LocalId`. Kræver kun Adressevælger-token — ikke Datafordeler API-nøgle.

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

## Azure Functions API (HTTP / Postman)

Vil du eksponere **samme data som TestWeb** via HTTP — fx til Postman eller downstream-systemer?

Se **[AZURE-FUNCTIONS-API.md](./AZURE-FUNCTIONS-API.md)** for komplet guide med:

- **16 HTTP-endpoints** — ét per panel på [TestWeb Home-siden](./TheNerdCollective.Integrations.Dar.TestWeb/Components/Pages/Home.razor) (DAR, BBR, opsummering + samlet opslag)
- Kopiér-klar sample-kode til .NET 8 isolated worker
- Konfiguration via `local.settings.json` og Azure Application settings
- **[Postman collection](./postman/TheNerdCollective.Integrations.Dar.postman_collection.json)** med alle requests

Hovedendpoint **`GET /api/lookup/full?vej=...&postnr=...&by=...`** returnerer samme JSON som *Test alle services*. Granulære endpoints som `/api/bbr/etager` returnerer det samme som det tilsvarende panel.

> Function App'ens **udgående IP** skal whitelists i [Datafordeler Administration](https://administration.datafordeler.dk/).

---

## Arkitektur

`DarClientFactory.Create()` returnerer `DarServices` — en facade opdelt efter register:

```
DarServices
├── Dar
│   ├── Autocomplete    → fonetisk søgning + id-opslag (koordinater) via Adressevælger
│   ├── Adresseopslag   → native DAR-resultat + valgfri KvHxInput (DAWA legacy)
│   ├── Husnummer       → native DAR husnummer uden KvHxInput
│   ├── Kommune         → DAGI kommuner (GraphQL + DAWA/WFS/REST-fallback)
│   └── Postnummer      → aktive postnumre (GraphQL) + kommune via DAWA/REST
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

### UI med autocomplete (anbefalet)

```csharp
// 1. Bruger vælger adresse i autocomplete
var valgt = (await services.Dar.Autocomplete.SearchAsync("Århusvej 69a 3000"))
    .First(f => f.IsCompleteAddress);

// 2. Koordinater + husnummer-id (kun Adressevælger-token)
var detaljer = await services.Dar.Autocomplete.GetDetailsAsync(valgt);
var husnummerId = detaljer.HusnummerId;

// 3. BBR via Datafordeler (kræver ApiKey + whitelisted IP)
var bygninger = await services.Bbr.Bygning.GetAllByHusnummerIdAsync(husnummerId);
```

Ved konkret husnummer (fx `Baldersgade 45`) eller enhedssøgning (fx `2. th`) supplerer `SearchAsync` med `/adresser/soeg` og viser enheder (`ResultType` `adresse`). Brug altid `GetDetailsAsync(valgt)` — servicen resolver husnummer-id automatisk.

### Direkte adresseopslag (uden autocomplete)

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
| `SearchAsync(searchText, ct?)` | Fonetisk søgning — returnerer op til 10 forslag |
| `GetDetailsAsync(selection, ct?)` | Id-opslag på et autocomplete-resultat (anbefalet) |
| `GetDetailsAsync(localId, resultType?, husnummerId?, ct?)` | Id-opslag med felter fra søgning — tom `resultType` behandles som `husnummer` |

**Søgning (`SearchAsync`)**

| Adressevælger-endpoint | Hvornår | Resultat-typer |
|---|---|---|
| `GET /husnumre/soeg` | Altid (primær søgning) | `husnummer`, `vejnavn`, `navngivenvejpostnummer` |
| `GET /adresser/soeg` | Når søgningen rammer et husnummer, eller teksten ligner enhed (fx `1. th`, `2. tv`) | `adresse` (med `husnummerId` i svaret) |

`SearchAsync` returnerer `DanishAddressAutocompleteResult`:

| Felt | Beskrivelse |
|---|---|
| `LocalId` | Adressevælger-id for forslaget (`husnummer`- eller `adresse`-id) |
| `HusnummerId` | DAR husnummer-id — sat for type `adresse`; ellers typisk samme som `LocalId` for `husnummer` |
| `DisplayName`, `AddressLine1`, `PostalCode`, `City`, `AddressLine2` | Parsed fra Adressevælger `titel` |
| `IsCompleteAddress` | `true` for `husnummer` og `adresse` — `false` for vejnavn-forslag |
| `ResultType` | `husnummer`, `adresse`, `vejnavn` eller `navngivenvejpostnummer` |

**Id-opslag (`GetDetailsAsync`)**

Kalder Adressevælger [id-opslag](https://confluence.sdfi.dk/pages/viewpage.action?pageId=246743156) via **`GET /husnumre/{id}`** (ikke `/adresser/{id}`) og returnerer `DanishAddressDetailResult`:

| Felt | Beskrivelse |
|---|---|
| `HusnummerId` | DAR `id_lokalId` — brug videre i Datafordeler/BBR |
| `Betegnelse`, `Vejnavn`, `Husnummer`, `Postnummer`, `Postdistrikt` | Adressebetegnelser fra Adressevælger |
| `Kommunekode` | Fra `navngivenvejkommunedel` |
| `Easting`, `Northing`, `CoordinateSystem` | Adgangspunkt i ETRS89 (EPSG:25832) |
| `Latitude`, `Longitude` | WGS84 (EPSG:4326), beregnet fra ETRS89 |

Eksempel (husnummer):

```csharp
var forslag = await services.Dar.Autocomplete.SearchAsync("Århusvej 69a 3000");
var valgt = forslag.First(f => f.IsCompleteAddress);

var detaljer = await services.Dar.Autocomplete.GetDetailsAsync(valgt);
// detaljer.HusnummerId, detaljer.Latitude, detaljer.Longitude, detaljer.Easting, detaljer.Northing

// Fortsæt til BBR med samme husnummer-id
var bygninger = await services.Bbr.Bygning.GetAllByHusnummerIdAsync(detaljer.HusnummerId);
```

Eksempel (enhed — `ResultType` `adresse`):

```csharp
var forslag = await services.Dar.Autocomplete.SearchAsync("Århusvej 69a 1. th 3000");
var valgt = forslag.First(f =>
    f.IsCompleteAddress
    && string.Equals(f.ResultType, "adresse", StringComparison.OrdinalIgnoreCase));

// LocalId er adresse-id; HusnummerId bruges internt til id-opslag
var detaljer = await services.Dar.Autocomplete.GetDetailsAsync(valgt);
Assert.Equal(valgt.HusnummerId, detaljer.HusnummerId);
```

`GetDetailsAsync` virker for `ResultType` `husnummer` og `adresse` — ikke for vejnavn-forslag. Tom eller manglende `resultType` behandles som `husnummer`; hvis `husnummerId` afviger fra `localId`, infereres `adresse`. Kræver kun Adressevælger-token.

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

#### `services.Dar.Kommune`

Kommuner via **DAGI** (`DAGI_Kommuneinddeling`, [GraphQL v2](https://graphql.datafordeler.dk/DAGI/v2)) — se [GraphQL (DAGI)](https://confluence.sdfi.dk/pages/viewpage.action?pageId=199984259). GraphQL bruger **samme API-nøgle** som DAR/BBR.

> **DAWA-fallback (vigtigt)**  
> Datafordeler DAGI GraphQL returnerer i praksis ofte **tomme resultater**, indtil registeret er fuldt udrullet på GraphQL.  
> Indtil da bruger pakken automatisk **[DAWA](https://dawadocs.dataforsyningen.dk/dok/api/kommune)** (`api.dataforsyningen.dk`) som fallback — **gratis, ingen API-nøgle**, samme DAGI-grunddata (`dagi_id`, `kode`, `navn`).  
> Når GraphQL engang returnerer data, forsøges GraphQL **først**; DAWA bruges kun hvis GraphQL er tom (eller returnerer for få kommuner til liste-opslag).  
> Slå DAWA fra med `Dagi:EnableDawaFallback: false` hvis du kun vil bruge Datafordeler-kanaler.

**Fallback-rækkefølge**

| Metode | Rækkefølge når GraphQL er tom |
|---|---|
| `GetAllAsync` | DAWA liste → WFS (API-nøgle) → fejl |
| `FindByCoordinatesAsync` | DAWA reverse (`/kommuner/reverse`, WGS84) → REST DAGI punkt (valgfri tjenestebruger) → fejl |
| `FindByEtrs89Async` | DAWA reverse (EPSG:25832) → REST DAGI punkt → fejl |

Kommune-listen caches i 24 timer som standard (`Dagi:KommuneListCacheDuration`). DAWA-listen hentes med paginering (`side` / `per_side`).

```json
"TheNerdCollective": {
  "Dar": {
    "ApiKey": "...",
    "Dagi": {
      "EnableDawaFallback": true,
      "DawaBaseUrl": "https://api.dataforsyningen.dk",
      "RestUsername": "",
      "RestPassword": "",
      "KommuneListCacheDuration": "24:00:00"
    }
  }
}
```

| Metode | Beskrivelse |
|---|---|
| `GetAllAsync(ct?)` | Alle aktuelle kommuner (`id_lokalId`, `navn`, `kommunekode`), sorteret efter navn |
| `FindByCoordinatesAsync(latitude, longitude, ct?)` | Finder kommune for WGS84-koordinater (EPSG:4326), fx fra `navigator.geolocation` |
| `FindByEtrs89Async(easting, northing, ct?)` | Samme opslag med ETRS89 UTM 32N (EPSG:25832) |

Eksempel (kommune-dropdown + “Din lokation”):

```csharp
// Dropdown: hent alle kommuner (GraphQL → DAWA → WFS)
var kommuner = await services.Dar.Kommune.GetAllAsync();

// Browser-geolocation → kommune (GraphQL → DAWA reverse → REST)
var position = await GetBrowserPositionAsync(); // latitude / longitude (WGS84)
var kommune = await services.Dar.Kommune.FindByCoordinatesAsync(
    position.Latitude,
    position.Longitude);
// kommune.Navn fx "København", kommune.Kommunekode fx "0101"
```

Returnerer `KommuneDto` med `IdLokalId`, `Navn` og `Kommunekode`.

#### `services.Dar.Postnummer`

Aktive postnumre via **DAR GraphQL** (`DAR_Postnummer`, [v3](https://graphql.datafordeler.dk/DAR/v3)) med `status = 3` (gyldige/aktive). Kommune-metadata hentes via **[DAWA](https://dawadocs.dataforsyningen.dk/dok/api/postnummer)** (`api.dataforsyningen.dk/postnumre`) — **gratis, ingen API-nøgle** — eller DAR REST husnummer-opslag (`MedDybde=true`) som fallback.

> **DAWA til kommune-metadata**  
> DAR GraphQL returnerer postnummer og postdistrikt, men ikke kommune direkte på `DAR_Postnummer`.  
> Pakken beriger derfor med DAWA (eller REST) når du kalder `GetByMunicipalityCodeAsync` / `GetByPostalCodesAsync`.  
> Slå DAWA fra med `Postnummer:EnableDawaEnrichment: false` — så bruges kun DAR REST husnummer-opslag (langsommere ved mange kald).

**Datakilder pr. metode**

| Metode | Primær kilde | Fallback | Cache |
|---|---|---|---|
| `GetAllActiveAsync` | DAR GraphQL (`DAR_Postnummer`, pagineret) | — | 30 dage (default) |
| `GetByMunicipalityCodeAsync` | DAWA `?kommunekode=` | REST-baseret filtrering | DAWA live |
| `GetByPostalCodesAsync` | DAWA `?nr=` | DAR REST husnummer pr. postnr | DAWA live |

**Primær kommune**

Når et postnummer tilhører flere kommuner (sjældent), vælges primær kommune således:

1. Første DAWA-kommune hvis `navn` matcher postdistriktet (fx 2730 → Herlev `0163`)
2. Ellers sidste kommune i DAWA-listen
3. Ved REST-fallback: kommune fra første husnummer (`kommuneinddeling` på `husnummer?postnr=…`)

**Downstream-mapping (eksempel)**

| Pakke (`PostnummerMedKommuneDto`) | Typisk host-DTO |
|---|---|
| `Postnummer` | `PostalCode` / `nr` |
| `Postdistrikt` | `City` / `navn` |
| `Kommunekode` | `MunicipalityCode` / `kommuner[].kode` |
| `Kommunenavn` | `MunicipalityName` / `kommuner[].navn` |

```json
"TheNerdCollective": {
  "Dar": {
    "ApiKey": "...",
    "Postnummer": {
      "EnableDawaEnrichment": true,
      "DawaBaseUrl": "https://api.dataforsyningen.dk",
      "RestUrl": "https://services.datafordeler.dk/DAR/DAR/3.0.0/rest",
      "CacheDuration": "30.00:00:00",
      "MaxParallelKommuneLookups": 12
    }
  }
}
```

| Config | Default | Beskrivelse |
|---|---|---|
| `EnableDawaEnrichment` | `true` | Brug DAWA til kommune-opslag og kommunefilter |
| `DawaBaseUrl` | `https://api.dataforsyningen.dk` | DAWA base-URL |
| `RestUrl` | DAR REST 3.0.0 | Husnummer-opslag når DAWA fejler |
| `CacheDuration` | `30` dage | Cache for `GetAllActiveAsync` |
| `MaxParallelKommuneLookups` | `12` | Parallel REST-opslag ved DAWA-fallback på stor liste |

| Metode | Beskrivelse |
|---|---|
| `GetAllActiveAsync(ct?)` | Alle aktive postnumre (`Postnummer`, `Postdistrikt`), sorteret efter postnummer |
| `GetByMunicipalityCodeAsync(kommunekode, ct?)` | Postnumre for en kommune (firecifret kode, fx `"0101"`) |
| `GetByPostalCodesAsync(postalCodes, ct?)` | `PostnummerMedKommuneDto` pr. angivet postnummer; pipe-separeret streng i ét element understøttes |

```csharp
// Fuld liste (tung første gang — caches i 30 dage)
var postnumre = await services.Dar.Postnummer.GetAllActiveAsync();
// postnumre.Count typisk > 1000; indeholder fx "2730" / "Herlev"

// København autocomplete (GEO-05)
var koebenhavn = await services.Dar.Postnummer.GetByMunicipalityCodeAsync("0101");

// Enkelt opslag (GEO-04)
var byNumber = await services.Dar.Postnummer.GetByPostalCodesAsync(new[] { "2100" });

// Batch med pipe (GEO-04b) — mindst 2 forskellige kommunekoder i resultatet
var batch = await services.Dar.Postnummer.GetByPostalCodesAsync(
    new[] { "2730|2750|2610|2800" });
// batch[0].Kommunekode fx "0163" (Herlev), batch[1] fx "0151" (Ballerup)
```

**DTO'er**

```csharp
// PostnummerDto — fra GetAllActiveAsync / GetByMunicipalityCodeAsync
public record PostnummerDto
{
    public string Postnummer { get; init; }   // "2730"
    public string Postdistrikt { get; init; } // "Herlev"
}

// PostnummerMedKommuneDto — fra GetByPostalCodesAsync
public sealed record PostnummerMedKommuneDto : PostnummerDto
{
    public string? Kommunekode { get; init; }  // "0163"
    public string? Kommunenavn { get; init; }    // "Herlev"
}
```

**Performance:** `GetAllActiveAsync` henter alle aktive postnumre via GraphQL-pagination (~1000+ rækker). Planlæg lang timeout ved første kald i downstream HTTP-API'er; efterfølgende kald er hurtige pga. cache.

Test i **TestWeb**: `/postnummer` — faner for alle tre metoder med kodeeksempler.

Returnerer `PostnummerDto` / `PostnummerMedKommuneDto`.

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
| `DanishAddressAutocompleteResult` | Fonetisk søgningsforslag — `LocalId`, `HusnummerId`, `ResultType`, parsed adressefelter |
| `DanishAddressDetailResult` | Id-opslag efter autocomplete via `/husnumre/{id}` (koordinater, husnummer-id) |
| `AdresseopslagResult` | Adresseopslag med native `Dar` + valgfri `KvHxInput` (legacy) |
| `DarAdresseopslagDto` | Native DAR-resultat (`Husnummer` + `Vejnavn`) |
| `HusnummerLookupResult` | Husnummer med native `Dar` (uden KvHxInput) |
| `KommuneDto` | Kommune (`id_lokalId`, `navn`, `kommunekode`) fra DAGI/DAWA |
| `PostnummerDto` | Aktivt postnummer (`Postnummer`, `Postdistrikt`) |
| `PostnummerMedKommuneDto` | Postnummer med primær `Kommunekode` / `Kommunenavn` |
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

| Side | Route | Tester |
|---|---|---|
| Adresse & BBR | `/adresse` | Autocomplete, adresseopslag, fuld BBR-kæde |
| Kommune | `/kommune` | `Dar.Kommune.GetAllAsync`, `FindByCoordinatesAsync` |
| Postnummer | `/postnummer` | `Dar.Postnummer.GetAllActiveAsync`, `GetByMunicipalityCodeAsync`, `GetByPostalCodesAsync` |

Opsæt API-nøgle:

```bash
cp src/TheNerdCollective.Integrations.Dar.TestWeb/appsettings.local.json.example \
   src/TheNerdCollective.Integrations.Dar.TestWeb/appsettings.local.json
```

`appsettings.local.json` er i `.gitignore` og committes ikke.

**Adresse & BBR** (`/adresse`) kalder alle BBR-services i én operation. Standard testadresse: **Århusvej 69a, 3000 Helsingør**.

**Kommune** og **Postnummer** har egne sider med faner, JSON-resultater og **Kald i kode**-eksempler pr. metode.

### Integrationstests

```bash
dotnet test tests/TheNerdCollective.Integrations.Dar.IntegrationTests
```

Integrationstests dækker bl.a. `DarKommuneIntegrationTests` og `DarPostnummerIntegrationTests` (postnummer: ≥500 aktive, 2730, kommunekode 0101, Herlev-batch).

API-nøgle læses i prioriteret rækkefølge:

1. `TheNerdCollective__Dar__ApiKey` eller `DATAFORDELER_API_KEY`
2. `appsettings.local.json` i TestWeb
3. Fallback i `IntegrationTestEnvironment`

Kræver whitelisted IP — ellers springes testen over ved `DAF-AUTH-0005`.

---

## Versionering

**Nuværende version:** `1.5.3`

Publiceres til [NuGet.org](https://www.nuget.org/packages/TheNerdCollective.Integrations.Dar) via GitHub Actions ved push til `main`.

Bump `<Version>` i `TheNerdCollective.Integrations.Dar.csproj` før release. Git-tag: `TheNerdCollective.Integrations.Dar-v{version}`.
