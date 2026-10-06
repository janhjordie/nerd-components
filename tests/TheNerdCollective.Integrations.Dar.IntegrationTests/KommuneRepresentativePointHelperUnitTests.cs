using TheNerdCollective.Integrations.Dar.Mapping;
using TheNerdCollective.Integrations.Dar.Models;
using Xunit;

namespace TheNerdCollective.Integrations.Dar.IntegrationTests;

public sealed class KommuneRepresentativePointHelperUnitTests
{
    [Fact]
    public void HaversineDistanceMeters_is_zero_for_identical_points()
    {
        var distance = KommuneRepresentativePointHelper.HaversineDistanceMeters(55.6761, 12.5683, 55.6761, 12.5683);
        Assert.True(distance < 1);
    }

    [Fact]
    public void TryFindNearestByRepresentativePoint_picks_closest_kommune_within_max_distance()
    {
        var kommuner = new[]
        {
            new KommuneDto
            {
                Kommunekode = "0165",
                Navn = "Albertslund",
                RepræsentativPunktLatitude = 55.68497387,
                RepræsentativPunktLongitude = 12.35231988
            },
            new KommuneDto
            {
                Kommunekode = "0190",
                Navn = "Furesø",
                RepræsentativPunktLatitude = 55.78539677,
                RepræsentativPunktLongitude = 12.3712133
            }
        };

        var nearest = KommuneRepresentativePointHelper.TryFindNearestByRepresentativePoint(
            55.68497387,
            12.35231988,
            kommuner,
            maxDistanceMeters: 500);

        Assert.NotNull(nearest);
        Assert.Equal("0165", nearest!.Kommunekode);
    }

    [Fact]
    public void TryGetDistanceToRepresentativeMeters_returns_null_when_repr_missing()
    {
        var kommune = new KommuneDto { Kommunekode = "0101", Navn = "Test" };
        Assert.Null(KommuneRepresentativePointHelper.TryGetDistanceToRepresentativeMeters(55.0, 12.0, kommune));
    }
}
