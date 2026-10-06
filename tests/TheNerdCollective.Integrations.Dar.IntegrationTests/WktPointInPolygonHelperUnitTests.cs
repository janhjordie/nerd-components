using System.Text.Json;
using TheNerdCollective.Integrations.Dar.Mapping;
using Xunit;

namespace TheNerdCollective.Integrations.Dar.IntegrationTests;

public sealed class WktPointInPolygonHelperUnitTests
{
    [Fact]
    public void ContainsEtrs89_returns_true_for_point_inside_simple_polygon()
    {
        const string wkt = "POLYGON ((0 0, 10 0, 10 10, 0 10, 0 0))";
        Assert.True(WktPointInPolygonHelper.ContainsEtrs89(wkt, 5, 5));
        Assert.False(WktPointInPolygonHelper.ContainsEtrs89(wkt, 15, 5));
    }

    [Fact]
    public void GeoJsonFeatureContainsEtrs89_returns_true_for_point_in_polygon_geometry()
    {
        using var document = JsonDocument.Parse(
            """
            {
              "type": "Feature",
              "geometry": {
                "type": "Polygon",
                "coordinates": [[[0,0],[10,0],[10,10],[0,10],[0,0]]]
              },
              "properties": {}
            }
            """);

        Assert.True(WktPointInPolygonHelper.GeoJsonFeatureContainsEtrs89(document.RootElement, 5, 5));
        Assert.False(WktPointInPolygonHelper.GeoJsonFeatureContainsEtrs89(document.RootElement, 20, 5));
    }

    [Fact]
    public void TryGetAbsoluteAreaEtrs89_returns_smaller_value_for_smaller_polygon()
    {
        const string small = "POLYGON ((0 0, 2 0, 2 2, 0 2, 0 0))";
        const string large = "POLYGON ((0 0, 10 0, 10 10, 0 10, 0 0))";

        var smallArea = WktPointInPolygonHelper.TryGetAbsoluteAreaEtrs89(small);
        var largeArea = WktPointInPolygonHelper.TryGetAbsoluteAreaEtrs89(large);

        Assert.True(smallArea < largeArea);
    }
}
