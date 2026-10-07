using TheNerdCollective.Integrations.Dar.Mapping;
using Xunit;

namespace TheNerdCollective.Integrations.Dar.IntegrationTests;

public sealed class Etrs89ConversionSanityTests
{
    [Theory]
    [InlineData(55.6794, 12.5346)]
    [InlineData(55.678757, 12.526196)]
    [InlineData(56.02304649121056, 12.598664281911976)]
    public void FromWgs84_roundtrip_within_one_centimeter(double latitude, double longitude)
    {
        var (easting, northing) = Etrs89Utm32NConverter.FromWgs84(latitude, longitude);
        var (latitude2, longitude2) = Etrs89Utm32NConverter.ToWgs84(easting, northing);

        Assert.InRange(latitude2, latitude - 1e-7, latitude + 1e-7);
        Assert.InRange(longitude2, longitude - 1e-7, longitude + 1e-7);
    }

    [Theory]
    [InlineData(55.6794, 12.5346, 721_500, 723_800, 6_174_500, 6_178_200)]
    [InlineData(55.678757, 12.526196, 721_500, 723_800, 6_174_500, 6_178_200)]
    public void FromWgs84_Frederiksberg_ligger_inden_for_0147_utm_bbox(
        double latitude,
        double longitude,
        double minE,
        double maxE,
        double minN,
        double maxN)
    {
        var (easting, northing) = Etrs89Utm32NConverter.FromWgs84(latitude, longitude);

        Assert.InRange(easting, minE, maxE);
        Assert.InRange(northing, minN, maxN);
    }
}
