using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdMudPageSurfaceIntentTests
{
    [Fact]
    public void Page_surface_intent_remaps_primary_and_info_to_readable_secondary_on_cream()
    {
        var options = NerdEmbeddedBrandPack.FromBrandJson("dnf").TokenPack.ToOptions();

        var map = NerdMudIntentPaletteMap.ResolveIntentPaletteMap(
            options,
            NerdDesignSystemUi.PageSurface,
            NerdMudPaletteMode.Light);

        Assert.Equal("#002D26", map.Primary, StringComparer.OrdinalIgnoreCase);
        Assert.Equal(map.Secondary, map.Primary);
        Assert.Equal(map.TextPrimary, map.InfoText);
        Assert.Equal(map.TextPrimary, map.SuccessText);
        Assert.NotEqual("#A6E54C", map.Primary, StringComparer.OrdinalIgnoreCase);
    }
}
