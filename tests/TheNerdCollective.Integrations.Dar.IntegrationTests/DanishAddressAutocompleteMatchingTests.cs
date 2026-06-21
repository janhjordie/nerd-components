using TheNerdCollective.Integrations.Dar.Models;
using Xunit;

namespace TheNerdCollective.Integrations.Dar.IntegrationTests;

public sealed class DanishAddressAutocompleteMatchingTests
{
    [Fact]
    public void ResolveBestMatch_foretraekker_husnummer_uden_enhed_i_soegning()
    {
        var husnummer = Create("h1", "h1", "husnummer", "Øster Allé 48, 8260 Viby J");
        var adresse = Create("a1", "h1", "adresse", "Øster Allé 48, 8260 Viby J");

        var valgt = DanishAddressAutocompleteMatching.ResolveBestMatch(
            [adresse, husnummer],
            "Øster Allé 48, 8260 Viby J");

        Assert.Equal("h1", valgt?.LocalId);
        Assert.Equal("husnummer", valgt?.ResultType);
    }

    [Fact]
    public void ResolveBestMatch_foretraekker_adresse_med_enhed_i_soegning()
    {
        var husnummer = Create("h1", "h1", "husnummer", "Øster Allé 48, 8260 Viby J");
        var adresse = Create("a1", "h1", "adresse", "Øster Allé 48, 2. tv, 8260 Viby J", "2. tv");

        var valgt = DanishAddressAutocompleteMatching.ResolveBestMatch(
            [husnummer, adresse],
            "Øster Allé 48, 2. tv, 8260 Viby J");

        Assert.Equal("a1", valgt?.LocalId);
        Assert.Equal("adresse", valgt?.ResultType);
    }

    [Fact]
    public void GetHusnummerIdForLookup_bruger_husnummerId_for_adresse()
    {
        var adresse = Create("a1", "h1", "adresse", "Øster Allé 48, 2. tv, 8260 Viby J", "2. tv");

        Assert.Equal("h1", adresse.GetHusnummerIdForLookup());
        Assert.Equal("a1", adresse.GetAdresseLocalId());
    }

    [Fact]
    public void GetHusnummerIdForLookup_bruger_localId_for_husnummer()
    {
        var husnummer = Create("h1", "h1", "husnummer", "Øster Allé 48, 8260 Viby J");

        Assert.Equal("h1", husnummer.GetHusnummerIdForLookup());
        Assert.Null(husnummer.GetAdresseLocalId());
    }

    private static DanishAddressAutocompleteResult Create(
        string localId,
        string husnummerId,
        string resultType,
        string displayName,
        string addressLine2 = "") =>
        new(
            localId,
            displayName,
            displayName.Split(',')[0].Trim(),
            "8260",
            "Viby J",
            addressLine2,
            true,
            resultType,
            husnummerId);
}
