using TheNerdCollective.Brand.Tnc;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdAliasChainToolsTests
{
    [Fact]
    public void Build_resolves_alias_to_color_hex()
    {
        var options = new NerdDesignTokenOptions();
        NerdTncDesignTokenPresets.Apply(options);

        var chain = NerdAliasChainTools.Build(options, "primary-action");

        Assert.Contains(chain, step => step.Name == "primary-action");
        Assert.Contains(chain, step => step.Kind == NerdAliasChainStepKind.Color);
        Assert.Contains("#", NerdAliasChainTools.Format(chain));
    }
}
