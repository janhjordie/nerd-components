using TheNerdCollective.Integrations.Dar.GraphQL;
using TheNerdCollective.Integrations.Dar.Services;
using Xunit;

namespace TheNerdCollective.Integrations.Dar.IntegrationTests;

public sealed class MunicipalityRealLocationRegressionTests
{
    [SkippableFact]
    public async Task FindByCoordinatesAsync_Frederiksberg_Raadhus_returnerer_0147()
    {
        DarServices services;
        try
        {
            services = IntegrationTestEnvironment.CreateServices();
        }
        catch (DatafordelerApiException ex) when (ex.ResponseBody.Contains("DAF-AUTH-0005", StringComparison.Ordinal))
        {
            Skip.If(true, "IP not whitelisted in Datafordeler (DAF-AUTH-0005).");
            return;
        }

        var resolved = await services.Dar.Kommune.FindByCoordinatesAsync(55.6794, 12.5346);
        Assert.Equal("0147", resolved.Kommunekode);
        Assert.Equal("Frederiksberg", resolved.Navn);
    }
}
