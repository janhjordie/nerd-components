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
        // Lines must stay ink on cream — never cream-on-cream (skov.Content).
        Assert.Equal("#002D26", map.LinesInputs, StringComparer.OrdinalIgnoreCase);
        Assert.Equal("#002D26", map.LinesDefault, StringComparer.OrdinalIgnoreCase);
        Assert.NotEqual(map.Surface, map.LinesInputs, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void Brand_palette_lines_inputs_use_input_border_ink_not_token_content()
    {
        var options = NerdEmbeddedBrandPack.FromBrandJson("dnf").TokenPack.ToOptions();
        var map = NerdMudBrandPaletteMap.Resolve(options, NerdMudPaletteMode.Light);

        Assert.Equal("#002D26", map.LinesInputs, StringComparer.OrdinalIgnoreCase);
        Assert.NotEqual("#FDFAF3", map.LinesInputs, StringComparer.OrdinalIgnoreCase);
    }
}
