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

        var adresseopslag = await services.Dar.Adresseopslag.LookupAsync(
            request.StreetAndNumber,
            request.PostalCode,
            request.City,
            cancellationToken);

        var husnummer = await services.Dar.Husnummer.FindByAddressAsync(
            request.StreetAndNumber,
            request.PostalCode,
            cancellationToken);

        var bygning = !string.IsNullOrWhiteSpace(adresseopslag.BygningId)
            ? await services.Bbr.Bygning.GetByIdAsync(adresseopslag.BygningId, cancellationToken)
            : await services.Bbr.Bygning.GetByHusnummerIdAsync(adresseopslag.HusnummerId, cancellationToken);

        var bygningId = bygning.IdLokalId
            ?? throw new InvalidOperationException("Bygning mangler id_lokalId.");

        var enheder = await services.Bbr.Enhed.GetByBygningIdAsync(bygningId, cancellationToken);
        var etager = await services.Bbr.Etage.GetByBygningIdAsync(bygningId, cancellationToken);
        var opgange = await services.Bbr.Opgang.GetByBygningIdAsync(bygningId, cancellationToken);
        var tekniskeAnlaeg = await services.Bbr.TekniskAnlaeg.GetByBygningIdAsync(bygningId, cancellationToken);

        GrundDto? grund = null;
        IReadOnlyList<GrundJordstykkeDto> grundJordstykker = [];
        if (!string.IsNullOrWhiteSpace(bygning.Grund))
        {
            grund = await services.Bbr.Grund.GetByIdAsync(bygning.Grund, cancellationToken);
            grundJordstykker = await services.Bbr.Grund.GetJordstykkerByGrundIdAsync(bygning.Grund, cancellationToken);
        }

        var bygningEjendomsrelationer = await services.Bbr.Ejendomsrelation.GetByBygningIdAsync(
            bygningId,
            cancellationToken);
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
            Bygning = bygning,
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
