using TheNerdCollective.Integrations.Dar;
using TheNerdCollective.Integrations.Dar.Models;
using Xunit;

namespace TheNerdCollective.Integrations.Dar.IntegrationTests;

public sealed class DarAddressResolutionIntegrationTests
{
    [Fact]
    public async Task ResolveBestMatchAsync_adgangsadresse_returnerer_ids_og_KvHxInput()
    {
        var services = IntegrationTestEnvironment.CreateServices();
        var address = "Øster Allé 48, 8260 Viby J";

        var resolved = await services.Address.ResolveBestMatchAsync(address);

        Assert.Equal("husnummer", resolved.Ids.ResultType, ignoreCase: true);
        Assert.False(string.IsNullOrWhiteSpace(resolved.Ids.LocalId));
        Assert.Equal(resolved.Ids.LocalId, resolved.HusnummerId);
        Assert.Null(resolved.AdresseLocalId);
        Assert.False(string.IsNullOrWhiteSpace(resolved.KvHxInput.KvhxId));
    }

    [Fact]
    public async Task ResolveBestMatchAsync_enhed_inkluderer_etage_og_doer()
    {
        var services = IntegrationTestEnvironment.CreateServices();
        var address = "Øster Allé 48, 2. tv, 8260 Viby J";

        var resolved = await services.Address.ResolveBestMatchAsync(address);

        Assert.Equal("adresse", resolved.Ids.ResultType, ignoreCase: true);
        Assert.Equal(resolved.Ids.LocalId, resolved.AdresseLocalId);
        Assert.NotEqual(resolved.Ids.LocalId, resolved.HusnummerId);
        Assert.Equal("2", resolved.KvHxInput.Etage);
        Assert.Equal("tv", resolved.KvHxInput.Door);
        Assert.Equal("07519554__48__2__tv", resolved.KvHxInput.KvhxId);
    }

    [Fact]
    public async Task GetKvHxInputByLocalIdAsync_matcher_ResolveBestMatchAsync()
    {
        var services = IntegrationTestEnvironment.CreateServices();
        var address = "Øster Allé 48, 2. tv, 8260 Viby J";

        var resolved = await services.Address.ResolveBestMatchAsync(address);
        var kvhx = await services.Address.GetKvHxInputByLocalIdAsync(
            resolved.Ids.LocalId,
            resolved.Ids.ResultType,
            resolved.Ids.HusnummerId);

        Assert.Equal(resolved.KvHxInput.KvhxId, kvhx.KvhxId);
        Assert.Equal(resolved.KvHxInput.Etage, kvhx.Etage);
        Assert.Equal(resolved.KvHxInput.Door, kvhx.Door);
    }

    [Fact]
    public async Task Bbr_bygninger_via_husnummerId_fra_resolution()
    {
        var services = IntegrationTestEnvironment.CreateServices();
        var resolved = await services.Address.ResolveBestMatchAsync("Øster Allé 48, 8260 Viby J");

        var bygninger = await services.Bbr.Bygning.GetAllByHusnummerIdAsync(resolved.HusnummerId);

        Assert.NotEmpty(bygninger);
    }
}
