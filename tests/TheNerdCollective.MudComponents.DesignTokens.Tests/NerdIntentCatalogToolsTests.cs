namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdIntentCatalogToolsTests
{
    [Fact]
    public void StandardIntents_includes_primary_action()
    {
        Assert.Contains(
            NerdIntentCatalogTools.StandardIntents,
            entry => entry.Name == "primary-action");
    }

    [Fact]
    public void FormatClass_uses_prefix()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc" };
        Assert.Equal("tnc-primary-action", NerdIntentCatalogTools.FormatClass(options, "primary-action"));
    }
}
