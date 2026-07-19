using TheNerdCollective.Brand.Tnc;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdDtcgTokenToolsTests
{
    [Fact]
    public void Export_and_import_roundtrip_colors()
    {
        var source = new NerdDesignTokenOptions();
        NerdTncDesignTokenPresets.Apply(source);

        var json = NerdDtcgTokenTools.Export(source);
        var restored = new NerdDesignTokenOptions { Prefix = "tnc" };
        var result = NerdDtcgTokenTools.TryImport(restored, json);

        Assert.True(result.Success, result.Message);
        Assert.True(restored.Colors.ContainsKey("navy"));
        Assert.Equal(source.Colors["navy"].Value, restored.Colors["navy"].Value);
    }
}
