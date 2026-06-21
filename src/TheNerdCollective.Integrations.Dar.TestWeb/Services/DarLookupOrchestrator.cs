using System.Diagnostics;
using TheNerdCollective.Integrations.Dar.Models;
using TheNerdCollective.Integrations.Dar.Services;
using TheNerdCollective.Integrations.Dar.TestWeb.Models;

namespace TheNerdCollective.Integrations.Dar.TestWeb.Services;

public sealed class DarLookupOrchestrator(DarRuntime runtime)
{
    public string? ConfigurationError => runtime.ConfigurationError;

    public async Task<DarFullLookupResult> LookupAllAsync(
        DarLookupRequest request,
        CancellationToken cancellationToken = default)
    {
        if (ConfigurationError is not null)
        {
            throw new InvalidOperationException(ConfigurationError);
        }

        var services = runtime.CreateServices();
        var stopwatch = Stopwatch.StartNew();

        AdresseopslagResult adresseopslag;
        HusnummerLookupResult husnummer;

        if (request.AutocompleteSelection is not null)
        {
            adresseopslag = await services.Dar.Adresseopslag.LookupFromAutocompleteAsync(
                request.AutocompleteSelection,
                cancellationToken);

            husnummer = new HusnummerLookupResult
            {
                Dar = adresseopslag.Dar,
                Husnummer = adresseopslag.Husnummer,
                Adgangsadresse = adresseopslag.Adgangsadresse,
                HusnummerId = adresseopslag.HusnummerId,
                BygningId = adresseopslag.BygningId
            };
        }
        else
        {
            adresseopslag = await services.Dar.Adresseopslag.LookupAsync(
                request.StreetAndNumber,
                request.PostalCode,
                request.City,
                cancellationToken);

            husnummer = await services.Dar.Husnummer.FindByAddressAsync(
                request.StreetAndNumber,
                request.PostalCode,
                cancellationToken);
        }

        var bygninger = await services.Bbr.Bygning.GetAllByHusnummerIdAsync(
            adresseopslag.HusnummerId,
            cancellationToken);

        var enheder = new List<EnhedDto>();
        var etager = new List<EtageDto>();
        var opgange = new List<OpgangDto>();
        var tekniskeAnlaeg = new List<TekniskAnlaegDto>();
        var bygningEjendomsrelationer = new List<BygningEjendomsrelationDto>();

        GrundDto? grund = null;
        var grundJordstykker = new List<GrundJordstykkeDto>();
        var behandledeGrunde = new HashSet<string>(StringComparer.Ordinal);

        foreach (var bygning in bygninger)
        {
            var bygningId = bygning.IdLokalId
                ?? throw new InvalidOperationException("Bygning mangler id_lokalId.");

            enheder.AddRange(await services.Bbr.Enhed.GetByBygningIdAsync(bygningId, cancellationToken));
            etager.AddRange(await services.Bbr.Etage.GetByBygningIdAsync(bygningId, cancellationToken));
            opgange.AddRange(await services.Bbr.Opgang.GetByBygningIdAsync(bygningId, cancellationToken));
            tekniskeAnlaeg.AddRange(await services.Bbr.TekniskAnlaeg.GetByBygningIdAsync(bygningId, cancellationToken));
            bygningEjendomsrelationer.AddRange(
                await services.Bbr.Ejendomsrelation.GetByBygningIdAsync(bygningId, cancellationToken));

            if (!string.IsNullOrWhiteSpace(bygning.Grund) && behandledeGrunde.Add(bygning.Grund))
            {
                grund ??= await services.Bbr.Grund.GetByIdAsync(bygning.Grund, cancellationToken);
                grundJordstykker.AddRange(
                    await services.Bbr.Grund.GetJordstykkerByGrundIdAsync(bygning.Grund, cancellationToken));
            }
        }

        var ejendomsrelationer = await services.Bbr.Ejendomsrelation.ResolveAsync(
            bygningEjendomsrelationer,
            grund,
            cancellationToken);

        stopwatch.Stop();

        return new DarFullLookupResult
        {
            Duration = stopwatch.Elapsed,
            Adresseopslag = adresseopslag,
            Husnummer = husnummer,
            Bygninger = bygninger,
            Enheder = enheder,
            Etager = etager,
            Opgange = opgange,
            TekniskeAnlaeg = tekniskeAnlaeg,
            Grund = grund,
            GrundJordstykker = grundJordstykker,
            BygningEjendomsrelationer = bygningEjendomsrelationer,
            Ejendomsrelationer = ejendomsrelationer
        };
    }
}
