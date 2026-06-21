using TheNerdCollective.Integrations.Dar;
using TheNerdCollective.Integrations.Dar.Configuration;
using TheNerdCollective.Integrations.Dar.Models;
using Xunit;

namespace TheNerdCollective.Integrations.Dar.IntegrationTests;

public sealed class DarKvHxIntegrationTests
{
    [Fact]
    public async Task LookupFromAutocompleteAsync_inkluderer_etage_og_doer_i_KvHxInput()
    {
        var services = IntegrationTestEnvironment.CreateServices();
        var autocomplete = DarClientFactory.CreateAutocomplete(
            new DarAutocompleteOptions { Token = "adressevaelger123" },
            new HttpClient());

        var results = await autocomplete.SearchAsync("Øster Allé 48, 2. tv, 8260");
        var valgt = DanishAddressAutocompleteMatching.ResolveBestMatch(results, "Øster Allé 48, 2. tv, 8260")
            ?? throw new InvalidOperationException("Forventede autocomplete-resultat for enhedsadresse.");

        var lookup = await services.Dar.Adresseopslag.LookupFromAutocompleteAsync(valgt);
        var kvhx = lookup.KvHxInput;

        Assert.Equal("2", kvhx.Etage);
        Assert.Equal("tv", kvhx.Door);
        Assert.Equal("07519554__48__2__tv", kvhx.KvhxId);
        Assert.Equal(valgt.LocalId, kvhx.Id);
        Assert.Contains("2. tv", kvhx.Adressebetegnelse, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(lookup.Dar.Adresse);
        Assert.Equal("2", lookup.Dar.Adresse.Etagebetegnelse);
        Assert.Equal("tv", lookup.Dar.Adresse.Doerbetegnelse);
    }
}
