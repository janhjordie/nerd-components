using TheNerdCollective.Integrations.Dar.Models;
using TheNerdCollective.Integrations.Dar.Services.Dar.Internal;
using Xunit;

namespace TheNerdCollective.Integrations.Dar.IntegrationTests;

public sealed class KommuneRegionEnricherUnitTests
{
    [Fact]
    public void EnrichFromGraph_prefers_Dawa_visueltcenter_over_polygon_centroid()
    {
        var graph = new List<KommuneGraphDto>
        {
            new()
            {
                Kommunekode = "0420",
                Navn = "Assens",
                Geometri = new KoordinatDto
                {
                    Wkt = "POLYGON((587000 6200000, 588000 6200000, 588000 6201000, 587000 6201000, 587000 6200000))"
                }
            }
        };

        var dawa = new List<KommuneDto>
        {
            new()
            {
                Kommunekode = "0420",
                Navn = "Assens",
                RepræsentativPunktLatitude = 55.29,
                RepræsentativPunktLongitude = 10.03
            }
        };

        var enriched = KommuneRegionEnricher.EnrichFromGraph(graph, Array.Empty<RegionDto>(), dawa);

        Assert.Single(enriched);
        Assert.Equal(55.29, enriched[0].RepræsentativPunktLatitude);
        Assert.Equal(10.03, enriched[0].RepræsentativPunktLongitude);
    }
}
