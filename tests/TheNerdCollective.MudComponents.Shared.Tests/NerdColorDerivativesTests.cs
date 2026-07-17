using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.Shared.Tests;

public class NerdColorDerivativesTests
{
    [Fact]
    public void ToRgbString_converts_hex_to_rgb_tuple()
    {
        Assert.Equal("54, 92, 58", NerdColorDerivatives.ToRgbString("#365C3A"));
    }

    [Fact]
    public void Lighten_and_darken_adjust_hex_colors()
    {
        var lightened = NerdColorDerivatives.Lighten("#365C3A", 0.2);
        var darkened = NerdColorDerivatives.Darken("#365C3A", 0.2);

        Assert.StartsWith("#", lightened);
        Assert.StartsWith("#", darkened);
        Assert.NotEqual(lightened, darkened);
    }
}
