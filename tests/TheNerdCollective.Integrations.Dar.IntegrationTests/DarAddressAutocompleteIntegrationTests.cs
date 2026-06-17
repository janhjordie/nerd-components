using TheNerdCollective.Integrations.Dar;
using TheNerdCollective.Integrations.Dar.Configuration;
using Xunit;

namespace TheNerdCollective.Integrations.Dar.IntegrationTests;

public sealed class DarAddressAutocompleteIntegrationTests
{
    [Fact]
    public async Task Autocomplete_returnerer_adresser_for_Aarhusvej()
    {
        using var httpClient = new HttpClient();
        var autocomplete = DarClientFactory.CreateAutocomplete(
            new DarAutocompleteOptions { Token = "adressevaelger123" },
            httpClient);

        var results = await autocomplete.SearchAsync("Århusvej 69");

        Assert.NotEmpty(results);
        Assert.Contains(results, result => result.DisplayName.Contains("Århusvej", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Autocomplete_returnerer_enheder_for_Baldersgade_45()
    {
        using var httpClient = new HttpClient();
        var autocomplete = DarClientFactory.CreateAutocomplete(
            new DarAutocompleteOptions { Token = "adressevaelger123" },
            httpClient);

        var results = await autocomplete.SearchAsync("Baldersgade 45");

        Assert.Contains(
            results,
            result => result.DisplayName.Contains("2. th", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(
            results,
            result => result.DisplayName.Contains("2. tv", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(
            results,
            result => string.Equals(result.ResultType, "adresse", StringComparison.OrdinalIgnoreCase));
    }
}
