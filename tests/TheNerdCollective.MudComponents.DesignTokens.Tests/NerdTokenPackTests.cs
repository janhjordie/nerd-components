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

    [Fact]
    public void FromJsonRejectsReferences_to_missing_tokens()
    {
        var json = """
        {
          "clientId": "acme",
          "prefix": "acme",
          "colors": {
            "ocean": { "value": "#123456", "contrastText": "#FFFFFF" }
          },
          "aliases": { "primary": "missing" },
          "recipes": {}
        }
        """;

        Assert.Throws<ArgumentException>(() => NerdTokenPack.FromJson(json));
    }

    [Fact]
    public void Validate_accepts_a_complete_pack()
    {
        var options = new NerdDesignTokenOptions { Prefix = "acme" }
            .Add("ocean", new NerdColorToken { Value = "#123456", ContrastText = "#FFFFFF" })
            .Alias("primary", "ocean")
            .AddRecipe("cta", new NerdDesignTokenRecipe("ocean", "ocean", "ocean"));

        NerdTokenPack.FromOptions(options, "acme").Validate();
    }

    [Fact]
    public void FromPreset_loads_dnf_pack()
    {
        var pack = NerdTokenPack.FromPreset("dnf", "acme");

        Assert.Equal("acme", pack.ClientId);
        Assert.Equal("dnf", pack.Prefix);
        Assert.Equal(12, pack.Colors.Count);
        Assert.Contains("kridt-himmel", pack.Recipes.Keys);
    }

    [Fact]
    public void FromPreset_rejects_unknown_preset()
    {
        Assert.Throws<ArgumentException>(() => NerdTokenPack.FromPreset("unknown"));
    }

    [Fact]
    public void Merge_overrides_existing_tokens_and_keeps_base_tokens()
    {
        var basePack = NerdTokenPack.FromPreset("dnf", "base");
        var overrides = NerdTokenPack.FromOptions(
            new NerdDesignTokenOptions { Prefix = "acme" }
                .Add("aurora", new NerdColorToken { Value = "#7C5CFF", ContrastText = "#FFFFFF" }),
            "acme");

        var merged = basePack.Merge(overrides);

        Assert.Equal("acme", merged.ClientId);
        Assert.Equal(13, merged.Colors.Count);
        Assert.Contains("skov", merged.Colors.Keys);
        Assert.Equal("#7C5CFF", merged.Colors["aurora"].Value);
    }

    [Fact]
    public async Task Token_pack_store_saves_loads_and_lists_clients()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"nerd-token-{Guid.NewGuid():N}");
        try
        {
            var store = new FileNerdTokenPackStore(directory);
            await store.SaveAsync(NerdTokenPack.FromPreset("dnf", "acme"));

            var loaded = await store.LoadAsync("acme");

            Assert.Equal(12, loaded?.Colors.Count);
            Assert.Equal(["acme"], await store.ListAsync());
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }
}
