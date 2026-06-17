using TheNerdCollective.Integrations.Dar;
using TheNerdCollective.Integrations.Dar.Configuration;
using Xunit;

namespace TheNerdCollective.Integrations.Dar.IntegrationTests;

public sealed class DarAddressAutocompleteDetailIntegrationTests
{
    [Fact]
    public async Task GetDetailsAsync_returnerer_koordinater_for_autocomplete_valg()
    {
        using var httpClient = new HttpClient();
        var autocomplete = DarClientFactory.CreateAutocomplete(
            new DarAutocompleteOptions { Token = "adressevaelger123" },
            httpClient);

        var results = await autocomplete.SearchAsync("Århusvej 69a 3000");
        var selection = results.FirstOrDefault(r =>
            r.IsCompleteAddress
            && string.Equals(r.ResultType, "husnummer", StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(selection);

        var details = await autocomplete.GetDetailsAsync(selection);

        Assert.Equal(selection.LocalId, details.HusnummerId);
        Assert.False(string.IsNullOrWhiteSpace(details.Betegnelse));
        Assert.True(details.Easting > 0);
        Assert.True(details.Northing > 0);
        Assert.InRange(details.Latitude, 55.0, 57.0);
        Assert.InRange(details.Longitude, 12.0, 13.0);
        Assert.Equal("3000", details.Postnummer);
    }
}
