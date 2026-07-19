using TheNerdCollective.MudComponents.DesignTokens;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdPairingSurfaceStylesTests
{
    [Fact]
    public void Create_emits_css_variables_and_resolved_colors()
    {
        var style = NerdPairingSurfaceStyles.Create("#122E43", "#FFFFFF");

        Assert.Contains("--nerd-pairing-surface-color:#122E43", style);
        Assert.Contains("--nerd-pairing-content-color:#FFFFFF", style);
        Assert.Contains("background-color:var(--nerd-pairing-surface-color)", style);
        Assert.Contains("color:var(--nerd-pairing-content-color)", style);
    }

    [Fact]
    public void ClassFor_includes_variant_modifier()
    {
        Assert.Equal(
            "nerd-pairing-surface nerd-pairing-surface--studio extra",
            NerdPairingSurfaceStyles.ClassFor(NerdPairingSurfaceVariant.Studio, "extra"));
    }
}
