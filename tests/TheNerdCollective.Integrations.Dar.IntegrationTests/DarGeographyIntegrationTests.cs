using TheNerdCollective.Integrations.Dar.GraphQL;
using TheNerdCollective.Integrations.Dar.Services;
using Xunit;

namespace TheNerdCollective.Integrations.Dar.IntegrationTests;

public sealed class DarGeographyIntegrationTests
{
    [SkippableFact]
    public async Task FindByCoordinatesDatafordelerAsync_returnerer_Helsingoer_med_repraesentativt_punkt()
    {
        var services = CreateDatafordelerServices();
        try
        {
            var kommune = await services.Dar.Kommune.FindByCoordinatesDatafordelerAsync(
                56.02304649121056,
                12.598664281911976);

            Assert.Equal("0217", kommune.Kommunekode);
            Assert.Equal("Helsingør", kommune.Navn);
            Assert.NotNull(kommune.RepræsentativPunktLongitude);
            Assert.NotNull(kommune.RepræsentativPunktLatitude);
        }
        catch (InvalidOperationException)
        {
            Skip.If(true, "DAGI-geografi er endnu ikke tilgængelig på Datafordeler for denne nøgle.");
        }
    }

    [SkippableFact]
    public async Task GetByCircleAsync_returnerer_postnumre_med_alle_kommuner()
    {
        var services = CreateServicesWithDawaFallback();
        var postnumre = await services.Dar.Postnummer.GetByCircleAsync(
            12.48168066,
            56.04907246,
            10000);

        Assert.Contains(postnumre, p => p.Postnummer == "3000");
        var post3050 = postnumre.FirstOrDefault(p => p.Postnummer == "3050");
        Assert.NotNull(post3050);
        var codes3050 = post3050.Kommuner.Select(k => k.Kommunekode).ToList();
        Assert.Contains("0210", codes3050);
        Assert.Contains("0217", codes3050);

        var allCodes = postnumre
            .SelectMany(p => p.Kommuner)
            .Select(k => k.Kommunekode)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        Assert.Contains("0217", allCodes);
        Assert.Contains("0210", allCodes);
        Assert.Contains("0270", allCodes);
        Assert.Contains("0219", allCodes);
    }

    [SkippableFact]
    public async Task GetByMunicipalityCodeWithKommunerAsync_returnerer_postnumre()
    {
        var services = CreateServicesWithDawaFallback();
        var postnumre = await services.Dar.Postnummer.GetByMunicipalityCodeWithKommunerAsync("0217");

        Assert.NotEmpty(postnumre);
        Assert.All(postnumre, p => Assert.NotEmpty(p.Kommuner));
    }

    private static DarServices CreateServicesWithDawaFallback()
    {
        try
        {
            return IntegrationTestEnvironment.CreateServices();
        }
        catch (DatafordelerApiException ex) when (ex.ResponseBody.Contains("DAF-AUTH-0005", StringComparison.Ordinal))
        {
            Skip.If(true, "IP not whitelisted (DAF-AUTH-0005).");
            throw;
        }
    }

    private static DarServices CreateDatafordelerServices()
    {
        try
        {
            return IntegrationTestEnvironment.CreateDatafordelerOnlyServices();
        }
        catch (DatafordelerApiException ex) when (ex.ResponseBody.Contains("DAF-AUTH-0005", StringComparison.Ordinal))
        {
            Skip.If(true, "IP not whitelisted (DAF-AUTH-0005).");
            throw;
        }
    }
}
