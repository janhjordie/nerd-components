using System.Text.Json;
using TheNerdCollective.MudComponents.DesignTokens;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdTokenPackTests
{
    [Fact]
    public void RoundTripPreservesColorsAliasesAndRecipes()
    {
        var options = new NerdDesignTokenOptions { Prefix = "acme" }
            .Add("ocean", new NerdColorToken { Value = "#123456", ContrastText = "#FFFFFF" })
            .Alias("primary", "ocean")
            .AddRecipe("cta", new NerdDesignTokenRecipe("ocean", "ocean", "ocean"));

        var pack = NerdTokenPack.FromOptions(options, "acme");
        var restored = NerdTokenPack.FromJson(pack.ToJson()).ToOptions();

        Assert.Equal("acme", pack.ClientId);
        Assert.Equal("#123456", restored.Colors["ocean"].Value);
        Assert.Equal("ocean", restored.Aliases["primary"]);
        Assert.Equal("ocean", restored.Recipes["cta"].Action);
    }

    [Fact]
    public void FromJsonRejectsEmptyJson()
    {
        Assert.Throws<ArgumentException>(() => NerdTokenPack.FromJson(" "));
        Assert.Throws<JsonException>(() => NerdTokenPack.FromJson("{"));
    }
}
