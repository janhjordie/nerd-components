using TheNerdCollective.Integrations.Dar.Models;
using Xunit;

namespace TheNerdCollective.Integrations.Dar.IntegrationTests;

public sealed class DarAddressIdsTests
{
    [Fact]
    public void GetHusnummerIdForLookup_bruger_localId_for_husnummer()
    {
        var ids = new DarAddressIds
        {
            LocalId = "h1",
            ResultType = "husnummer",
            HusnummerId = "h1"
        };

        Assert.Equal("h1", ids.GetHusnummerIdForLookup());
        Assert.Null(ids.GetAdresseLocalId());
    }

    [Fact]
    public void GetHusnummerIdForLookup_bruger_husnummerId_for_adresse()
    {
        var ids = new DarAddressIds
        {
            LocalId = "a1",
            ResultType = "adresse",
            HusnummerId = "h1"
        };

        Assert.Equal("h1", ids.GetHusnummerIdForLookup());
        Assert.Equal("a1", ids.GetAdresseLocalId());
    }

    [Fact]
    public void FromSelection_bevarer_ids()
    {
        var selection = new DanishAddressAutocompleteResult(
            "a1",
            "Øster Allé 48, 2. tv, 8260 Viby J",
            "Øster Allé 48",
            "8260",
            "Viby J",
            "2. tv",
            true,
            "adresse",
            "h1");

        var ids = DarAddressIds.FromSelection(selection);

        Assert.Equal("a1", ids.LocalId);
        Assert.Equal("adresse", ids.ResultType);
        Assert.Equal("h1", ids.HusnummerId);
    }
}
