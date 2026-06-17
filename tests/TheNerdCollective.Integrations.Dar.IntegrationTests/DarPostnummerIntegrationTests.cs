using TheNerdCollective.Integrations.Dar.GraphQL;
using TheNerdCollective.Integrations.Dar.Models;
using TheNerdCollective.Integrations.Dar.Services;
using Xunit;

namespace TheNerdCollective.Integrations.Dar.IntegrationTests;

public sealed class DarPostnummerIntegrationTests
{
    [SkippableFact]
    public async Task GetAllActiveAsync_returnerer_mindst_500_aktive_postnumre_indeholdende_2730()
    {
        var services = CreateServices();
        var postnumre = await services.Dar.Postnummer.GetAllActiveAsync();

        Assert.True(postnumre.Count >= 500, $"Forventede mindst 500 postnumre, fik {postnumre.Count}.");
        Assert.Contains(postnumre, p => p.Postnummer == "2730");
        Assert.Equal(postnumre.OrderBy(p => p.Postnummer, StringComparer.Ordinal), postnumre);
        Assert.All(postnumre, p =>
        {
            Assert.Matches("^[0-9]{4}$", p.Postnummer);
            Assert.False(string.IsNullOrWhiteSpace(p.Postdistrikt));
        });
    }

    [SkippableFact]
    public async Task GetByMunicipalityCodeAsync_returnerer_postnumre_for_Koebenhavn()
    {
        var services = CreateServices();
        var postnumre = await services.Dar.Postnummer.GetByMunicipalityCodeAsync("0101");

        Assert.NotEmpty(postnumre);
        Assert.All(postnumre, p => Assert.Matches("^[0-9]{4}$", p.Postnummer));
    }

    [SkippableFact]
    public async Task GetByPostalCodesAsync_returnerer_2100()
    {
        var services = CreateServices();
        var results = await services.Dar.Postnummer.GetByPostalCodesAsync(new[] { "2100" });

        Assert.Contains(results, p => p.Postnummer == "2100");
        Assert.All(results, p => Assert.False(string.IsNullOrWhiteSpace(p.Postdistrikt)));
    }

    [SkippableFact]
    public async Task GetByPostalCodesAsync_herlev_batch_returnerer_forskellige_kommunekoder()
    {
        var services = CreateServices();
        var results = await services.Dar.Postnummer.GetByPostalCodesAsync(
            new[] { "2730", "2750", "2610", "2800" });

        Assert.True(results.Count >= 2, $"Forventede mindst 2 postnumre, fik {results.Count}.");
        var codes = results
            .Select(p => p.Kommunekode)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        Assert.True(codes.Count >= 2, $"Forventede mindst 2 kommunekoder, fik {string.Join(", ", codes)}.");
        Assert.Contains(results, p => p.Postnummer == "2730" && p.Kommunekode == "0163");
    }

    [SkippableFact]
    public async Task GetByPostalCodesAsync_accepterer_pipe_separeret_streng()
    {
        var services = CreateServices();
        var results = await services.Dar.Postnummer.GetByPostalCodesAsync(
            new[] { "2730|2750" });

        Assert.Equal(2, results.Count);
        Assert.Contains(results, p => p.Postnummer == "2730");
        Assert.Contains(results, p => p.Postnummer == "2750");
    }

    private static DarServices CreateServices()
    {
        try
        {
            return IntegrationTestEnvironment.CreateServices();
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
