using TheNerdCollective.MudComponents.DesignTokens;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdBrandRootClassesTests
{
    [Fact]
    public void Combine_includes_mud_nerd_and_fluent_brand_roots()
    {
        var classes = NerdBrandRootClasses.Combine("tnc");

        Assert.Contains("tnc-mud-brand", classes);
        Assert.Contains("tnc-nerd-brand", classes);
        Assert.Contains("tnc-fluent-brand", classes);
        Assert.Contains("tnc-radzen-brand", classes);
    }
}
