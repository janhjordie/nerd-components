using TheNerdCollective.Integrations.Dar.GraphQL;
using TheNerdCollective.Integrations.Dar.Models;
using TheNerdCollective.Integrations.Dar.Services;
using Xunit;

namespace TheNerdCollective.Integrations.Dar.IntegrationTests;

public sealed class DarRegionIntegrationTests
{
    [SkippableFact]
    public async Task GetAllAsync_returnerer_mindst_5_regioner_indeholdende_Hovedstaden()
    {
        var services = await CreateServicesAsync();
        var regioner = await GetRegionerOrSkipAsync(services);

        Assert.True(regioner.Count >= 5, $"Forventede mindst 5 regioner, fik {regioner.Count}.");
        Assert.Contains(
            regioner,
            r => string.Equals(r.Regionskode, "1084", StringComparison.Ordinal)
                 && r.Regionnavn == "Region Hovedstaden");
        Assert.All(regioner, r =>
        {
            Assert.False(string.IsNullOrWhiteSpace(r.Regionskode));
            Assert.False(string.IsNullOrWhiteSpace(r.Regionnavn));
        });
    }

    private const string DagiAccessDeniedMessage =
        "Ingen regioner fra DAGI GraphQL, DAWA eller WFS. Tjek EnableDawaFallback (default true) og netværksadgang til api.dataforsyningen.dk.";

    private static async Task<IReadOnlyList<RegionDto>> GetRegionerOrSkipAsync(DarServices services)
    {
        try
        {
            return await services.Dar.Region.GetAllAsync();
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
