using TheNerdCollective.Blazor.ThemeKit;
using Xunit;

namespace TheNerdCollective.Blazor.ThemeKit.Tests;

public class ThemeVersionHelperTests
{
    [Theory]
    [InlineData("1.0.0", "1.0.1")]
    [InlineData("0.1.0", "0.1.1")]
    [InlineData("2.3.4", "2.3.5")]
    public void BumpPatch_increments_build_number(string current, string expected)
        => Assert.Equal(expected, ThemeVersionHelper.BumpPatch(current));
}

public class MudThemeSessionSerializationTests
{
    [Fact]
    public void Session_collection_roundtrips_through_json()
    {
        var collection = new MudThemeSessionCollection
        {
            Sessions =
            {
                ["billetsalg-default"] = new MudThemeSession
                {
                    SavedAtUtc = "2026-06-16T12:00:00Z",
                    Version = "1.0.0",
                    Document = new MudThemeJsonDocument
                    {
                        Id = "billetsalg-default",
                        Version = "1.0.0",
                        Tokens = new Dictionary<string, string> { ["light.primary"] = "#0B7285" },
                    },
                },
            },
        };

        var json = System.Text.Json.JsonSerializer.Serialize(collection);
        var restored = System.Text.Json.JsonSerializer.Deserialize<MudThemeSessionCollection>(json);

        Assert.NotNull(restored);
        Assert.True(restored!.Sessions.ContainsKey("billetsalg-default"));
        Assert.Equal("#0B7285", restored.Sessions["billetsalg-default"].Document.Tokens["light.primary"]);
    }
}
