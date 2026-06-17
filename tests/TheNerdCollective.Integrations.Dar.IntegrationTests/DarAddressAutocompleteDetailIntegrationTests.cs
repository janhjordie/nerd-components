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

    [Fact]
    public async Task GetDetailsAsync_virker_for_adresse_type_via_husnummerId()
    {
        using var httpClient = new HttpClient();
        var autocomplete = DarClientFactory.CreateAutocomplete(
            new DarAutocompleteOptions { Token = "adressevaelger123" },
            httpClient);

        var results = await autocomplete.SearchAsync("Århusvej 69a 1. th 3000");
        var selection = results.FirstOrDefault(r =>
            r.IsCompleteAddress
            && string.Equals(r.ResultType, "adresse", StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(selection);
        Assert.False(string.IsNullOrWhiteSpace(selection.HusnummerId));
        Assert.NotEqual(selection.LocalId, selection.HusnummerId);

        var details = await autocomplete.GetDetailsAsync(selection);

        Assert.Equal(selection.HusnummerId, details.HusnummerId);
        Assert.True(details.Easting > 0);
        Assert.True(details.Northing > 0);
    }

    [Fact]
    public async Task GetDetailsAsync_default_husnummer_naar_resultType_mangler()
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

        var details = await autocomplete.GetDetailsAsync(selection.LocalId);

        Assert.Equal(selection.LocalId, details.HusnummerId);
        Assert.True(details.Easting > 0);
    }

    [Fact]
    public async Task GetDetailsAsync_default_adresse_naar_resultType_mangler_men_husnummerId_findes()
    {
        using var httpClient = new HttpClient();
        var autocomplete = DarClientFactory.CreateAutocomplete(
            new DarAutocompleteOptions { Token = "adressevaelger123" },
            httpClient);

        var results = await autocomplete.SearchAsync("Baldersgade 45");
        var selection = results.FirstOrDefault(r =>
            r.IsCompleteAddress
            && string.Equals(r.ResultType, "adresse", StringComparison.OrdinalIgnoreCase)
            && r.DisplayName.Contains("2. th", StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(selection);
        Assert.False(string.IsNullOrWhiteSpace(selection.HusnummerId));

        var details = await autocomplete.GetDetailsAsync(
            selection.LocalId,
            husnummerId: selection.HusnummerId);

        Assert.Equal(selection.HusnummerId, details.HusnummerId);
        Assert.True(details.Easting > 0);
    }
}
