# Azure Functions API — DAR/BBR som HTTP

Denne guide viser, hvordan du bygger en **Azure Function App**, der eksponerer **præcis de samme data** som [TestWeb Home-siden](../TheNerdCollective.Integrations.Dar.TestWeb/Components/Pages/Home.razor) — ét endpoint per panel, plus samlet opslag og opsummering.

Klar til kald fra **Postman**, curl eller andre HTTP-klienter.

---

## Kort over TestWeb → API

Hver række svarer til et `ResultPanel` (eller sektion) på Home-siden:

| TestWeb (Home.razor) | HTTP-endpoint | Response-type |
|---|---|---|
| Adresse-autocomplete | `GET /api/dar/autocomplete?q=` | `DanishAddressAutocompleteResult[]` |
| **Autocomplete → KvHxInput** | `GET /api/dar/adresseopslag/kvhx/from-autocomplete?q=` | `KvHxInputDto` |
| **Autocomplete → Adresseopslag** | `GET /api/dar/adresseopslag/from-autocomplete?localId=&resultType=&husnummerId=&q=` | `AdresseopslagResult` |
| **Autocomplete → full lookup** | `GET /api/lookup/full/from-autocomplete?q=` | `DarFullLookupResult` |
| **Dar.Adresseopslag.Dar** | `GET /api/dar/adresseopslag/dar` | `DarAdresseopslagDto` |
| **Dar.Adresseopslag** | `GET /api/dar/adresseopslag` | `AdresseopslagResult` |
| **Dar.Adresseopslag.KvHxInput** | `GET /api/dar/adresseopslag/kvhx` | `KvHxInputDto` |
| **Dar.Husnummer** | `GET /api/dar/husnummer` | `HusnummerLookupResult` |
| Knappen **Test alle services** | `GET /api/lookup/full` | `DarFullLookupResult` |
| **Opsummering** (tællere øverst) | `GET /api/lookup/summary` | opsummeringsobjekt |
| **Bbr.Bygning** | `GET /api/bbr/bygninger` | `BygningDto[]` |
| **Bbr.Enhed** | `GET /api/bbr/enheder` | `EnhedDto[]` |
| **Bbr.Etage** | `GET /api/bbr/etager` | `EtageDto[]` |
| **Bbr.Opgang** | `GET /api/bbr/opgange` | `OpgangDto[]` |
| **Bbr.TekniskAnlaeg** | `GET /api/bbr/tekniske-anlaeg` | `TekniskAnlaegDto[]` |
| **Bbr.Grund** | `GET /api/bbr/grund` | `GrundDto` |
| **Bbr.Grund.Jordstykker** | `GET /api/bbr/grund/jordstykker` | `GrundJordstykkeDto[]` |
| **Bbr.Ejendomsrelation (bygning)** | `GET /api/bbr/ejendomsrelation/bygning` | `BygningEjendomsrelationDto[]` |
| **Bbr.Ejendomsrelation.Resolve** | `GET /api/bbr/ejendomsrelation` | `EjendomsrelationDto[]` |

### Fælles query-parametre

Alle endpoints undtagen **autocomplete** bruger adresse-input som TestWeb-formularen:

| Parameter | Påkrævet | Eksempel |
|---|---|---|
| `vej` | Ja | `Århusvej 69a` |
| `postnr` | Ja | `3000` |
| `by` | Nej | `Helsingør` |

Autocomplete bruger `q` (min. 2 tegn).

**Autocomplete → opslag (anbefalet for Cosmos/adresse-tekst):**

| Parameter | Påkrævet | Beskrivelse |
|---|---|---|
| `q` | Ja* | Fri-tekst søgning, fx `Øster Allé 48, 8260 Viby J` |
| `localId` | Nej | Fra autocomplete — springer søgning over hvis angivet sammen med `resultType` |
| `resultType` | Nej | `husnummer` eller `adresse` |
| `husnummerId` | Nej | Påkrævet for type `adresse` |

\* Enten `q` alene (server kalder `SearchAsync` + `ResolveBestMatch`) eller eksplicitte ids fra forrige autocomplete-kald.

> **Tip:** Brug `/api/dar/adresseopslag/kvhx/from-autocomplete?q=...` til Sverres Azure Function-flow — **ikke** `DisplayName` i klassisk `vej`/`postnr`-opslag.

> **Tip:** Brug `/api/lookup/full` når du skal hente alt på én gang. De granulære `/api/bbr/*`-endpoints er til Postman/debug og returnerer samme data som det tilsvarende panel — de kalder internt `DarLookupOrchestrator.LookupAllAsync()` og returnerer det relevante udsnit.

---

## Forudsætninger

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Azure Functions Core Tools v4](https://learn.microsoft.com/azure/azure-functions/functions-run-local) (`func`)
- Datafordeler API-nøgle med adgang til DAR og BBR
- Function App'ens **udgående IP** whitelisted i [Datafordeler Administration](https://administration.datafordeler.dk/)
- (Valgfrit) Adressevælger-token til autocomplete — demo: `adressevaelger123`

---

## 1. Opret Function App

```bash
mkdir MyDarApi
cd MyDarApi
func init --worker-runtime dotnet-isolated --target-framework net8.0
dotnet new func --name MyDarApi
dotnet add package TheNerdCollective.Integrations.Dar
dotnet add package Microsoft.Azure.Functions.Worker
dotnet add package Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore
dotnet add package Microsoft.Azure.Functions.Worker.Sdk
```

---

## 2. Konfiguration

### `local.settings.json` (lokal — commit **ikke**)

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

I **Azure Portal** (Function App → Configuration → Application settings) sættes de samme nøgler med `__` som nesting (`TheNerdCollective__Dar__ApiKey`).

### `host.json`

```json
{
  "version": "2.0",
  "logging": {
    "applicationInsights": {
      "samplingSettings": {
        "isEnabled": true,
        "excludedTypes": "Request"
      }
    }
  },
  "extensionBundle": {
    "id": "Microsoft.Azure.Functions.ExtensionBundle",
    "version": "[4.*, 5.0.0)"
  }
}
```

---

## 3. Projektstruktur

```
MyDarApi/
├── Program.cs
├── host.json
├── local.settings.json
├── Models/
│   ├── DarLookupRequest.cs
│   ├── DarFullLookupResult.cs
│   └── DarLookupSummary.cs          ← ny (se nedenfor)
├── Services/
│   ├── DarRuntime.cs
│   └── DarLookupOrchestrator.cs
└── Functions/
    ├── DarApiFunctions.cs           ← DAR + autocomplete
    ├── LookupApiFunctions.cs        ← full + summary
    └── BbrApiFunctions.cs           ← alle BBR-paneler
```

**Kopiér fra TestWeb (uændret logik):**

| Kilde (TestWeb) | Destination |
|---|---|
| `Models/DarLookupRequest.cs` | `Models/DarLookupRequest.cs` |
| `Models/DarFullLookupResult.cs` | `Models/DarFullLookupResult.cs` |
| `Services/DarRuntime.cs` | `Services/DarRuntime.cs` |
| `Services/DarLookupOrchestrator.cs` | `Services/DarLookupOrchestrator.cs` |

Tilpas namespaces fra `...TestWeb...` til dit projektnavn (fx `MyDarApi`).

### `Models/DarLookupSummary.cs`

Svarer til **Opsummering**-sektionen på Home-siden:

```csharp
namespace MyDarApi.Models;

public sealed class DarLookupSummary
{
    public required double DurationMs { get; init; }
    public string? Adgangsadresse { get; init; }
    public string? HusnummerId { get; init; }
    public string? KvhxId { get; init; }
    public IReadOnlyList<string> Bygninger { get; init; } = [];
    public int EnhederCount { get; init; }
    public int EtagerCount { get; init; }
    public int OpgangeCount { get; init; }
    public int TekniskeAnlaegCount { get; init; }
    public int EjendomsrelationerCount { get; init; }

    public static DarLookupSummary FromResult(DarFullLookupResult result) => new()
    {
        DurationMs = result.Duration.TotalMilliseconds,
        Adgangsadresse = result.Adresseopslag?.Adgangsadresse,
        HusnummerId = result.Adresseopslag?.Dar.Husnummer.IdLokalId,
        KvhxId = result.Adresseopslag?.KvHxInput.KvhxId,
        Bygninger = result.Bygninger
            .Select(b => b.Bygningsnummer ?? b.IdLokalId ?? "?")
            .ToList(),
        EnhederCount = result.Enheder.Count,
        EtagerCount = result.Etager.Count,
        OpgangeCount = result.Opgange.Count,
        TekniskeAnlaegCount = result.TekniskeAnlaeg.Count,
        EjendomsrelationerCount = result.Ejendomsrelationer.Count
    };
}
```

---

## 4. `Program.cs`

```csharp
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyDarApi.Services;
using TheNerdCollective.Integrations.Dar.Configuration;

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
            client.DefaultRequestHeaders.Accept.Add(new("application/json"));
        });

        services.AddScoped<DarRuntime>();
        services.AddScoped<DarLookupOrchestrator>();
    })
    .Build();

host.Run();
```

---

## 5. Fælles hjælper

Alle adresse-baserede endpoints deler validering og fejlhåndtering:

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyDarApi.Models;
using MyDarApi.Services;
using TheNerdCollective.Integrations.Dar.GraphQL;
using TheNerdCollective.Integrations.Dar.Models;

namespace MyDarApi.Functions;

internal static class DarHttpHelper
{
    internal static bool TryParseAddressQuery(
        HttpRequest req,
        out DarLookupRequest request,
        out string? validationError)
    {
        request = new DarLookupRequest
        {
            StreetAndNumber = req.Query["vej"].ToString(),
            PostalCode = req.Query["postnr"].ToString(),
            City = string.IsNullOrWhiteSpace(req.Query["by"]) ? null : req.Query["by"].ToString()
        };

        if (string.IsNullOrWhiteSpace(request.StreetAndNumber) || string.IsNullOrWhiteSpace(request.PostalCode))
        {
            validationError = "Query parametrene 'vej' og 'postnr' er påkrævet.";
            return false;
        }

        validationError = null;
        return true;
    }

    internal static bool TryParseAutocompleteQuery(
        HttpRequest req,
        out DanishAddressAutocompleteResult? selection,
        out string? searchText,
        out string? validationError)
    {
        selection = null;
        searchText = req.Query["q"].ToString();
        var localId = req.Query["localId"].ToString();
        var resultType = req.Query["resultType"].ToString();
        var husnummerId = req.Query["husnummerId"].ToString();

        if (!string.IsNullOrWhiteSpace(localId))
        {
            selection = new DanishAddressAutocompleteResult(
                localId,
                req.Query["displayName"].ToString(),
                string.Empty,
                string.Empty,
                string.Empty,
                IsCompleteAddress: true,
                ResultType: resultType,
                HusnummerId: string.IsNullOrWhiteSpace(husnummerId) ? null : husnummerId);
            validationError = null;
            return true;
        }

        if (string.IsNullOrWhiteSpace(searchText) || searchText.Length < 2)
        {
            validationError = "Query parameter 'q' skal være mindst 2 tegn (eller angiv localId/resultType/husnummerId).";
            return false;
        }

        validationError = null;
        return true;
    }

    internal static async Task<DanishAddressAutocompleteResult> ResolveAutocompleteSelectionAsync(
        DarRuntime runtime,
        HttpRequest req,
        CancellationToken cancellationToken)
    {
        if (!TryParseAutocompleteQuery(req, out var explicitSelection, out var searchText, out var validationError))
        {
            throw new InvalidOperationException(validationError);
        }

        if (explicitSelection is not null)
        {
            return explicitSelection;
        }

        var results = await runtime.CreateAutocomplete().SearchAsync(searchText!, cancellationToken);
        return DanishAddressAutocompleteMatching.ResolveBestMatch(results, searchText)
            ?? throw new InvalidOperationException("Ingen DAR autocomplete-resultater fundet for adressen.");
    }

    internal static async Task<AdresseopslagResult> LookupFromAutocompleteAsync(
        DarRuntime runtime,
        HttpRequest req,
        CancellationToken cancellationToken)
    {
        var selection = await ResolveAutocompleteSelectionAsync(runtime, req, cancellationToken);
        var services = runtime.CreateServices();
        return await services.Dar.Adresseopslag.LookupFromAutocompleteAsync(selection, cancellationToken);
    }

    internal static IActionResult? ConfigError(DarRuntime runtime) =>
        runtime.ConfigurationError is { } error
            ? new ObjectResult(new { error }) { StatusCode = StatusCodes.Status503ServiceUnavailable }
            : null;

    internal static IActionResult MapDarException(Exception ex)
    {
        if (ex is DatafordelerApiException apiEx
            && apiEx.ResponseBody.Contains("DAF-AUTH-0005", StringComparison.Ordinal))
        {
            return new ObjectResult(new
            {
                error = "IP-adressen er ikke whitelisted i Datafordeler (DAF-AUTH-0005).",
                detail = "Tilføj Function App'ens udgående IP under https://administration.datafordeler.dk/"
            })
            { StatusCode = StatusCodes.Status403Forbidden };
        }

        return new ObjectResult(new { error = ex.Message }) { StatusCode = StatusCodes.Status502BadGateway };
    }

    internal static async Task<IActionResult> WithLookupAsync(
        HttpRequest req,
        DarRuntime runtime,
        DarLookupOrchestrator orchestrator,
        CancellationToken cancellationToken,
        Func<DarFullLookupResult, object?> selector)
    {
        if (ConfigError(runtime) is { } configFailure)
        {
            return configFailure;
        }

        if (!TryParseAddressQuery(req, out var request, out var validationError))
        {
            return new BadRequestObjectResult(new { error = validationError });
        }

        try
        {
            var result = await orchestrator.LookupAllAsync(request, cancellationToken);
            return new OkObjectResult(selector(result));
        }
        catch (Exception ex) when (ex is DatafordelerApiException or InvalidOperationException)
        {
            return MapDarException(ex);
        }
    }
}
```

---

## 6. HTTP-triggers

### `Functions/LookupApiFunctions.cs` — samlet opslag + opsummering

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using MyDarApi.Models;
using MyDarApi.Services;

namespace MyDarApi.Functions;

public sealed class LookupApiFunctions(DarLookupOrchestrator orchestrator, DarRuntime runtime)
{
    /// <summary>Panel: alle ResultPanels — svarer til "Test alle services".</summary>
    [Function("LookupFull")]
    public Task<IActionResult> LookupFull(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "lookup/full")] HttpRequest req,
        CancellationToken cancellationToken) =>
        DarHttpHelper.WithLookupAsync(req, runtime, orchestrator, cancellationToken, result => result);

    /// <summary>Panel: Opsummering (tællere øverst på Home-siden).</summary>
    [Function("LookupSummary")]
    public Task<IActionResult> LookupSummary(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "lookup/summary")] HttpRequest req,
        CancellationToken cancellationToken) =>
        DarHttpHelper.WithLookupAsync(
            req, runtime, orchestrator, cancellationToken,
            result => DarLookupSummary.FromResult(result));

    /// <summary>Samlet opslag via autocomplete (id-baseret) — Sverre/Cosmos-flow.</summary>
    [Function("LookupFullFromAutocomplete")]
    public async Task<IActionResult> LookupFullFromAutocomplete(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "lookup/full/from-autocomplete")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        if (DarHttpHelper.ConfigError(runtime) is { } configFailure)
        {
            return configFailure;
        }

        try
        {
            var selection = await DarHttpHelper.ResolveAutocompleteSelectionAsync(runtime, req, cancellationToken);
            var request = new DarLookupRequest
            {
                AutocompleteSelection = selection,
                AutocompleteSearchText = req.Query["q"].ToString()
            };
            var result = await orchestrator.LookupAllAsync(request, cancellationToken);
            return new OkObjectResult(result);
        }
        catch (Exception ex) when (ex is DatafordelerApiException or InvalidOperationException)
        {
            return DarHttpHelper.MapDarException(ex);
        }
    }
}
```

### `Functions/DarApiFunctions.cs` — DAR-paneler + autocomplete

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using MyDarApi.Services;
using TheNerdCollective.Integrations.Dar.GraphQL;
using TheNerdCollective.Integrations.Dar.Models;

namespace MyDarApi.Functions;

public sealed class DarApiFunctions(DarLookupOrchestrator orchestrator, DarRuntime runtime)
{
    /// <summary>Panel: Adresse-autocomplete.</summary>
    [Function("DarAutocomplete")]
    public async Task<IActionResult> Autocomplete(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "dar/autocomplete")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        var q = req.Query["q"].ToString();
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
        {
            return new BadRequestObjectResult(new { error = "Query parameter 'q' skal være mindst 2 tegn." });
        }

        try
        {
            var results = await runtime.CreateAutocomplete().SearchAsync(q, cancellationToken);
            return new OkObjectResult(results);
        }
        catch (Exception ex) when (ex is InvalidOperationException)
        {
            return DarHttpHelper.MapDarException(ex);
        }
    }

    /// <summary>Panel: Dar.Adresseopslag.Dar (native DAR).</summary>
    [Function("DarAdresseopslagDar")]
    public Task<IActionResult> AdresseopslagDar(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "dar/adresseopslag/dar")] HttpRequest req,
        CancellationToken cancellationToken) =>
        DarHttpHelper.WithLookupAsync(req, runtime, orchestrator, cancellationToken, r => r.Adresseopslag?.Dar);

    /// <summary>Panel: Dar.Adresseopslag.</summary>
    [Function("DarAdresseopslag")]
    public Task<IActionResult> Adresseopslag(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "dar/adresseopslag")] HttpRequest req,
        CancellationToken cancellationToken) =>
        DarHttpHelper.WithLookupAsync(req, runtime, orchestrator, cancellationToken, r => r.Adresseopslag);

    /// <summary>Panel: Dar.Adresseopslag.KvHxInput (DAWA legacy).</summary>
    [Function("DarAdresseopslagKvhx")]
    public Task<IActionResult> AdresseopslagKvhx(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "dar/adresseopslag/kvhx")] HttpRequest req,
        CancellationToken cancellationToken) =>
        DarHttpHelper.WithLookupAsync(req, runtime, orchestrator, cancellationToken, r => r.Adresseopslag?.KvHxInput);

    /// <summary>KvHxInput via autocomplete + LookupFromAutocompleteAsync (anbefalet).</summary>
    [Function("DarAdresseopslagKvhxFromAutocomplete")]
    public async Task<IActionResult> AdresseopslagKvhxFromAutocomplete(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "dar/adresseopslag/kvhx/from-autocomplete")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        if (DarHttpHelper.ConfigError(runtime) is { } configFailure)
        {
            return configFailure;
        }

        try
        {
            var lookup = await DarHttpHelper.LookupFromAutocompleteAsync(runtime, req, cancellationToken);
            return new OkObjectResult(lookup.KvHxInput);
        }
        catch (Exception ex) when (ex is DatafordelerApiException or InvalidOperationException)
        {
            return DarHttpHelper.MapDarException(ex);
        }
    }

    /// <summary>Adresseopslag via autocomplete (fuld AdresseopslagResult).</summary>
    [Function("DarAdresseopslagFromAutocomplete")]
    public async Task<IActionResult> AdresseopslagFromAutocomplete(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "dar/adresseopslag/from-autocomplete")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        if (DarHttpHelper.ConfigError(runtime) is { } configFailure)
        {
            return configFailure;
        }

        try
        {
            var lookup = await DarHttpHelper.LookupFromAutocompleteAsync(runtime, req, cancellationToken);
            return new OkObjectResult(lookup);
        }
        catch (Exception ex) when (ex is DatafordelerApiException or InvalidOperationException)
        {
            return DarHttpHelper.MapDarException(ex);
        }
    }

    /// <summary>Panel: Dar.Husnummer.</summary>
    [Function("DarHusnummer")]
    public Task<IActionResult> Husnummer(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "dar/husnummer")] HttpRequest req,
        CancellationToken cancellationToken) =>
        DarHttpHelper.WithLookupAsync(req, runtime, orchestrator, cancellationToken, r => r.Husnummer);
}
```

### `Functions/BbrApiFunctions.cs` — alle BBR-paneler

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using MyDarApi.Services;

namespace MyDarApi.Functions;

public sealed class BbrApiFunctions(DarLookupOrchestrator orchestrator, DarRuntime runtime)
{
    /// <summary>Panel: Bbr.Bygning.</summary>
    [Function("BbrBygninger")]
    public Task<IActionResult> Bygninger(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "bbr/bygninger")] HttpRequest req,
        CancellationToken cancellationToken) =>
        DarHttpHelper.WithLookupAsync(req, runtime, orchestrator, cancellationToken, r => r.Bygninger);

    /// <summary>Panel: Bbr.Enhed.</summary>
    [Function("BbrEnheder")]
    public Task<IActionResult> Enheder(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "bbr/enheder")] HttpRequest req,
        CancellationToken cancellationToken) =>
        DarHttpHelper.WithLookupAsync(req, runtime, orchestrator, cancellationToken, r => r.Enheder);

    /// <summary>Panel: Bbr.Etage.</summary>
    [Function("BbrEtager")]
    public Task<IActionResult> Etager(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "bbr/etager")] HttpRequest req,
        CancellationToken cancellationToken) =>
        DarHttpHelper.WithLookupAsync(req, runtime, orchestrator, cancellationToken, r => r.Etager);

    /// <summary>Panel: Bbr.Opgang.</summary>
    [Function("BbrOpgange")]
    public Task<IActionResult> Opgange(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "bbr/opgange")] HttpRequest req,
        CancellationToken cancellationToken) =>
        DarHttpHelper.WithLookupAsync(req, runtime, orchestrator, cancellationToken, r => r.Opgange);

    /// <summary>Panel: Bbr.TekniskAnlaeg.</summary>
    [Function("BbrTekniskeAnlaeg")]
    public Task<IActionResult> TekniskeAnlaeg(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "bbr/tekniske-anlaeg")] HttpRequest req,
        CancellationToken cancellationToken) =>
        DarHttpHelper.WithLookupAsync(req, runtime, orchestrator, cancellationToken, r => r.TekniskeAnlaeg);

    /// <summary>Panel: Bbr.Grund.</summary>
    [Function("BbrGrund")]
    public Task<IActionResult> Grund(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "bbr/grund")] HttpRequest req,
        CancellationToken cancellationToken) =>
        DarHttpHelper.WithLookupAsync(req, runtime, orchestrator, cancellationToken, r => r.Grund);

    /// <summary>Panel: Bbr.Grund.Jordstykker.</summary>
    [Function("BbrGrundJordstykker")]
    public Task<IActionResult> GrundJordstykker(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "bbr/grund/jordstykker")] HttpRequest req,
        CancellationToken cancellationToken) =>
        DarHttpHelper.WithLookupAsync(req, runtime, orchestrator, cancellationToken, r => r.GrundJordstykker);

    /// <summary>Panel: Bbr.Ejendomsrelation (bygning).</summary>
    [Function("BbrEjendomsrelationBygning")]
    public Task<IActionResult> EjendomsrelationBygning(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "bbr/ejendomsrelation/bygning")] HttpRequest req,
        CancellationToken cancellationToken) =>
        DarHttpHelper.WithLookupAsync(req, runtime, orchestrator, cancellationToken, r => r.BygningEjendomsrelationer);

    /// <summary>Panel: Bbr.Ejendomsrelation.Resolve.</summary>
    [Function("BbrEjendomsrelation")]
    public Task<IActionResult> Ejendomsrelation(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "bbr/ejendomsrelation")] HttpRequest req,
        CancellationToken cancellationToken) =>
        DarHttpHelper.WithLookupAsync(req, runtime, orchestrator, cancellationToken, r => r.Ejendomsrelationer);
}
```

> **Lokal udvikling:** Skift midlertidigt `AuthorizationLevel.Function` til `AuthorizationLevel.Anonymous` hvis du vil undgå function key under `func start`. Brug **Function** i produktion.

---

## 7. Kør lokalt

```bash
func start
```

Eksempler (standard-testadresse fra TestWeb):

```bash
BASE="http://localhost:7071/api"
ADDR="vej=Århusvej%2069a&postnr=3000&by=Helsingør"
KEY="code=DIN_FUNCTION_KEY"

# Samlet opslag — alle paneler
curl "$BASE/lookup/full?$ADDR&$KEY"

# Opsummering (tællere)
curl "$BASE/lookup/summary?$ADDR&$KEY"

# Enkelt panel — fx etager
curl "$BASE/bbr/etager?$ADDR&$KEY"

# Autocomplete
curl "$BASE/dar/autocomplete?q=Århusvej%2069&$KEY"

# Sverre — autocomplete → KvHx (adgangsadresse)
curl "$BASE/dar/adresseopslag/kvhx/from-autocomplete?q=Øster%20Allé%2048,%208260%20Viby%20J&$KEY"

# Sverre — autocomplete → KvHx (enhed med etage/dør)
curl "$BASE/dar/adresseopslag/kvhx/from-autocomplete?q=Øster%20Allé%2048,%202.%20tv,%208260%20Viby%20J&$KEY"
```

---

## 8. Deploy til Azure

```bash
az functionapp create --resource-group <rg> --consumption-plan-location westeurope \
  --runtime dotnet-isolated --runtime-version 8 --functions-version 4 \
  --name <unique-function-app-name> --storage-account <storage>

func azure functionapp publish <unique-function-app-name>
```

Husk Application settings for `TheNerdCollective__Dar__ApiKey` og evt. `TheNerdCollective__Dar__AutocompleteToken`.

Whitelist Function App'ens **outbound IP-adresser** i Datafordeler Administration.

---

## 9. Postman

Importér filerne fra [`postman/`](./postman/):

| Fil | Formål |
|---|---|
| [`TheNerdCollective.Integrations.Dar.postman_collection.json`](./postman/TheNerdCollective.Integrations.Dar.postman_collection.json) | Alle endpoints inkl. **Sverre — Autocomplete → KVHX** (Øster Allé 48) |
| [`TheNerdCollective.Integrations.Dar.postman_environment.json`](./postman/TheNerdCollective.Integrations.Dar.postman_environment.json) | Lokal udvikling |
| [`TheNerdCollective.Integrations.Dar.Azure.postman_environment.json`](./postman/TheNerdCollective.Integrations.Dar.Azure.postman_environment.json) | Azure (skabelon) |

### Opsætning

1. **Import** → collection + environment.
2. Vælg environment (**Local** eller **Azure**).
3. Sæt `functionKey` (tom ved `Anonymous` lokalt; ellers default key fra Azure Portal).
4. Sæt `baseUrl` til `http://localhost:7071/api` eller `https://<app>.azurewebsites.net/api`.
5. Gennemgå mapperne **Sverre — Autocomplete → KVHX**, **Lookup**, **DAR** og **BBR**.

**Sverre (Øster Allé 48):** Kør request *2. KvHxInput from autocomplete* direkte, eller *1 → 2b* for to-trins flow. For enhed: *3 → 4*.

---

## 10. Fejlfinding

| Symptom | Løsning |
|---|---|
| `DAF-AUTH-0005` | Whitelist Function App'ens udgående IP i Datafordeler Administration |
| `503` + "API-nøgle mangler" | Sæt `TheNerdCollective__Dar__ApiKey` i config |
| `401 Unauthorized` | Tilføj korrekt `functionKey` i Postman |
| Autocomplete virker ikke | Sæt `TheNerdCollective__Dar__AutocompleteToken` |
| Kun én bygning i `/bbr/bygninger` | Opgrader NuGet-pakken — orchestratoren bruger `GetAllByHusnummerIdAsync` |
| Granulære endpoints er langsomme | De kalder fuldt opslag internt — brug `/lookup/full` i produktion |

---

## Relateret

- [README.md](./README.md) — NuGet-pakke, konfiguration og API-reference
- [Home.razor](../TheNerdCollective.Integrations.Dar.TestWeb/Components/Pages/Home.razor) — TestWeb UI med alle paneler
