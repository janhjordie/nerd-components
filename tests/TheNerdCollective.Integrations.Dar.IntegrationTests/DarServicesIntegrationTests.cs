using TheNerdCollective.Integrations.Dar.GraphQL;
using TheNerdCollective.Integrations.Dar.Models;
using TheNerdCollective.Integrations.Dar.Services;
using Xunit;

namespace TheNerdCollective.Integrations.Dar.IntegrationTests;

public sealed class DarServicesIntegrationTests
{
    [SkippableFact]
    public async Task Alle_services_returnerer_data_for_Aarhusvej_69a()
    {
        var services = IntegrationTestEnvironment.CreateServices();
        var context = await ResolveTestContextAsync(services);

        AssertDarAddressServices(context);
        AssertBbrBygningService(context);
        AssertBbrEnhedService(context);
        AssertBbrEtageService(context);
        AssertBbrOpgangService(context);
        AssertBbrTekniskAnlaegService(context);
        AssertBbrGrundService(context);
        AssertBbrEjendomsrelationService(context);
    }

    private static async Task<IntegrationTestContext> ResolveTestContextAsync(DarServices services)
    {
        try
        {
            return await FetchTestContextAsync(services);
        }
        catch (DatafordelerApiException ex) when (ex.ResponseBody.Contains("DAF-AUTH-0005", StringComparison.Ordinal))
        {
            Skip.If(
                true,
                "IP-adressen er ikke whitelisted i Datafordeler Administration (DAF-AUTH-0005). " +
                "Tilføj din udgående IP under https://administration.datafordeler.dk/");
            throw;
        }
    }

    private static async Task<IntegrationTestContext> FetchTestContextAsync(DarServices services)
    {
        var adresseopslag = await services.Dar.Adresseopslag.LookupAsync(
            IntegrationTestEnvironment.TestStreetAndNumber,
            IntegrationTestEnvironment.TestPostalCode,
            IntegrationTestEnvironment.TestCity);

        var husnummer = await services.Dar.Husnummer.FindByAddressAsync(
            IntegrationTestEnvironment.TestStreetAndNumber,
            IntegrationTestEnvironment.TestPostalCode);

        var bygning = !string.IsNullOrWhiteSpace(adresseopslag.BygningId)
            ? await services.Bbr.Bygning.GetByIdAsync(adresseopslag.BygningId)
            : await services.Bbr.Bygning.GetByHusnummerIdAsync(adresseopslag.HusnummerId);

        var bygningId = bygning.IdLokalId
            ?? throw new InvalidOperationException("Bygning mangler id_lokalId.");

        var enheder = await services.Bbr.Enhed.GetByBygningIdAsync(bygningId);
        var etager = await services.Bbr.Etage.GetByBygningIdAsync(bygningId);
        var opgange = await services.Bbr.Opgang.GetByBygningIdAsync(bygningId);
        var tekniskeAnlaeg = await services.Bbr.TekniskAnlaeg.GetByBygningIdAsync(bygningId);

        GrundDto? grund = null;
        IReadOnlyList<GrundJordstykkeDto> grundJordstykker = [];
        if (!string.IsNullOrWhiteSpace(bygning.Grund))
        {
            grund = await services.Bbr.Grund.GetByIdAsync(bygning.Grund);
            grundJordstykker = await services.Bbr.Grund.GetJordstykkerByGrundIdAsync(bygning.Grund);
        }

        var bygningEjendomsrelationer = await services.Bbr.Ejendomsrelation.GetByBygningIdAsync(bygningId);
        var ejendomsrelationer = await services.Bbr.Ejendomsrelation.ResolveAsync(bygningEjendomsrelationer, grund);

        return new IntegrationTestContext(
            adresseopslag,
            husnummer,
            bygning,
            enheder,
            etager,
            opgange,
            tekniskeAnlaeg,
            grund,
            grundJordstykker,
            bygningEjendomsrelationer,
            ejendomsrelationer);
    }

    private static void AssertDarAddressServices(IntegrationTestContext context)
    {
        var adresse = context.Adresseopslag;
        Assert.False(string.IsNullOrWhiteSpace(adresse.HusnummerId));
        Assert.Equal(IntegrationTestEnvironment.TestPostalCode, adresse.PostalCode);
        Assert.Equal(IntegrationTestEnvironment.TestStreetAndNumber, adresse.StreetAndNumber, ignoreCase: true);
        Assert.Equal(IntegrationTestEnvironment.TestCity, adresse.City);
        Assert.Contains("Århusvej", adresse.Adgangsadresse, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("69", adresse.Adgangsadresse, StringComparison.OrdinalIgnoreCase);
        Assert.False(string.IsNullOrWhiteSpace(adresse.Husnummer.IdLokalId));

        var husnummer = context.Husnummer;
        Assert.Equal(adresse.HusnummerId, husnummer.HusnummerId);
        Assert.Equal(adresse.Adgangsadresse, husnummer.Adgangsadresse);
        Assert.Equal(adresse.BygningId, husnummer.BygningId);

        var kvHx = adresse.KvHxInput;
        Assert.Equal(adresse.HusnummerId, adresse.Dar.Husnummer.IdLokalId);
        Assert.Equal(adresse.HusnummerId, kvHx.Id);
        Assert.Equal(IntegrationTestEnvironment.TestPostalCode, kvHx.Postnummer);
        Assert.Equal("0", kvHx.Esrejendomsnr);
        Assert.Equal("69A", kvHx.Husnummer);
        Assert.Equal("0217", kvHx.Komunekode);
        Assert.Equal("9781", kvHx.Vejkode);
        Assert.Equal("02179781__69A______", kvHx.KvhxId);
        Assert.Contains("Århusvej", kvHx.Vejnavn, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Århusvej", kvHx.Adressebetegnelse, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertBbrBygningService(IntegrationTestContext context)
    {
        var bygning = context.Bygning;
        Assert.False(string.IsNullOrWhiteSpace(bygning.IdLokalId));
        Assert.False(string.IsNullOrWhiteSpace(bygning.Bygningsnummer));
        Assert.NotNull(bygning.Opfoerelsesaar);
        Assert.NotNull(bygning.SamletBygningsareal);
    }

    private static void AssertBbrEnhedService(IntegrationTestContext context)
    {
        Assert.NotEmpty(context.Enheder);
        Assert.All(context.Enheder, enhed => Assert.False(string.IsNullOrWhiteSpace(enhed.IdLokalId)));
    }

    private static void AssertBbrEtageService(IntegrationTestContext context)
    {
        Assert.NotEmpty(context.Etager);
        Assert.All(context.Etager, etage =>
        {
            Assert.False(string.IsNullOrWhiteSpace(etage.IdLokalId));
            Assert.Equal(context.Bygning.IdLokalId, etage.Bygning);
        });
    }

    private static void AssertBbrOpgangService(IntegrationTestContext context)
    {
        Assert.NotEmpty(context.Opgange);
        Assert.All(context.Opgange, opgang => Assert.False(string.IsNullOrWhiteSpace(opgang.IdLokalId)));
    }

    private static void AssertBbrTekniskAnlaegService(IntegrationTestContext context)
    {
        Assert.NotNull(context.TekniskeAnlaeg);
    }

    private static void AssertBbrGrundService(IntegrationTestContext context)
    {
        Assert.False(string.IsNullOrWhiteSpace(context.Bygning.Grund));
        Assert.NotNull(context.Grund);
        Assert.Equal(context.Bygning.Grund, context.Grund!.IdLokalId);
        Assert.NotEmpty(context.GrundJordstykker);
    }

    private static void AssertBbrEjendomsrelationService(IntegrationTestContext context)
    {
        Assert.NotNull(context.BygningEjendomsrelationer);
        Assert.NotNull(context.Ejendomsrelationer);

        // BFE hentes typisk via grund.bestemtFastEjendom — bygning-relationer kan være tomme.
        if (!string.IsNullOrWhiteSpace(context.Bygning.Grund))
        {
            Assert.NotEmpty(context.Ejendomsrelationer);
            Assert.Contains(context.Ejendomsrelationer, relation => relation.BfeNummer.HasValue);
        }
    }

    private sealed record IntegrationTestContext(
        AdresseopslagResult Adresseopslag,
        HusnummerLookupResult Husnummer,
        BygningDto Bygning,
        IReadOnlyList<EnhedDto> Enheder,
        IReadOnlyList<EtageDto> Etager,
        IReadOnlyList<OpgangDto> Opgange,
        IReadOnlyList<TekniskAnlaegDto> TekniskeAnlaeg,
        GrundDto? Grund,
        IReadOnlyList<GrundJordstykkeDto> GrundJordstykker,
        IReadOnlyList<BygningEjendomsrelationDto> BygningEjendomsrelationer,
        IReadOnlyList<EjendomsrelationDto> Ejendomsrelationer);
}
