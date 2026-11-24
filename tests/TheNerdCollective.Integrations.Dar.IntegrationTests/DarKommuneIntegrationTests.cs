using TheNerdCollective.Integrations.Dar.GraphQL;
using TheNerdCollective.Integrations.Dar.Models;
using TheNerdCollective.Integrations.Dar.Services;
using Xunit;

namespace TheNerdCollective.Integrations.Dar.IntegrationTests;

public sealed class DarKommuneIntegrationTests
{
    [SkippableFact]
    public async Task GetAllAsync_returnerer_kommuner_sorteret_efter_navn()
    {
        var services = await CreateServicesAsync();
        var kommuner = await GetKommunerOrSkipAsync(services);

        Assert.True(kommuner.Count >= 99, $"Forventede mindst 99 kommuner, fik {kommuner.Count}.");
        Assert.Contains(kommuner, k => k.Navn == "København");
        Assert.Equal(kommuner.OrderBy(k => k.Navn, StringComparer.CurrentCultureIgnoreCase), kommuner);
        Assert.All(kommuner, k =>
        {
            Assert.False(string.IsNullOrWhiteSpace(k.IdLokalId));
            Assert.False(string.IsNullOrWhiteSpace(k.Navn));
            Assert.False(string.IsNullOrWhiteSpace(k.Kommunekode));
            Assert.False(string.IsNullOrWhiteSpace(k.Regionskode));
            Assert.False(string.IsNullOrWhiteSpace(k.Regionnavn));
        });

        var koebenhavn = kommuner.First(k => k.Kommunekode == "0101");
        Assert.Equal("1084", koebenhavn.Regionskode);
        Assert.Equal("Region Hovedstaden", koebenhavn.Regionnavn);
    }

    [SkippableFact]
    public async Task FindByCoordinatesAsync_returnerer_Koebenhavn_for_centrum()
    {
        var services = await CreateServicesAsync();

        // Rådhuspladsen, København (WGS84 fra browser-geolocation)
        var kommune = await services.Dar.Kommune.FindByCoordinatesAsync(55.6759, 12.5655);

        Assert.Equal("København", kommune.Navn);
        Assert.Equal("0101", kommune.Kommunekode);
    }

    [SkippableFact]
    public async Task FindByCoordinatesAsync_returnerer_Helsingoer_for_testadresse()
    {
        var services = await CreateServicesAsync();

        // Ca. koordinater i Helsingør (WGS84)
        var kommune = await services.Dar.Kommune.FindByCoordinatesAsync(56.0361, 12.6136);

        Assert.Equal("Helsingør", kommune.Navn);
        Assert.Equal("0217", kommune.Kommunekode);
    }

    [SkippableTheory]
    [InlineData(57.008719345136434, 10.348322345606388, "0851", "Aalborg")] // Hals
    [InlineData(56.1456465450032, 10.14043589178547, "0751", "Aarhus")] // Brabrand
    [InlineData(57.4684031, 10.07790672, "0860", "Hjørring")]
    [InlineData(55.72707466, 12.36851587, "0151", "Ballerup")]
    public async Task FindByCoordinatesAsync_returnerer_kommune_for_tid_til_tur_koordinater(
        double latitude,
        double longitude,
        string expectedCode,
        string expectedNamePrefix)
    {
        var services = await CreateServicesAsync();
        var kommune = await services.Dar.Kommune.FindByCoordinatesAsync(latitude, longitude);

        Assert.Equal(expectedCode, kommune.Kommunekode);
        Assert.StartsWith(expectedNamePrefix, kommune.Navn, StringComparison.Ordinal);
    }

    private const string DagiAccessDeniedMessage =
        "Ingen kommuner fra DAGI GraphQL, DAWA eller WFS. Tjek EnableDawaFallback (default true) og netværksadgang til api.dataforsyningen.dk.";

    private static async Task<IReadOnlyList<KommuneDto>> GetKommunerOrSkipAsync(DarServices services)
    {
        try
        {
            var kommuner = await services.Dar.Kommune.GetAllAsync();
            return kommuner;
        }
        catch (InvalidOperationException)
        {
            Skip.If(true, DagiAccessDeniedMessage);
            throw;
        }
        catch (DatafordelerApiException ex) when (ex.StatusCode is 403 or 404)
        {
            Skip.If(true, DagiAccessDeniedMessage);
            throw;
        }
    }

    private static Task<DarServices> CreateServicesAsync()
    {
        try
        {
            return Task.FromResult(IntegrationTestEnvironment.CreateServices());
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
}
