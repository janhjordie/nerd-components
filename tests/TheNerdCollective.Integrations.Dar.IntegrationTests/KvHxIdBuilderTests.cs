using TheNerdCollective.Integrations.Dar.Mapping;
using Xunit;

namespace TheNerdCollective.Integrations.Dar.IntegrationTests;

public sealed class KvHxIdBuilderTests
{
    [Fact]
    public void BuildAdgangsadresse_matcher_dawa_format()
    {
        var kvhx = KvHxIdBuilder.BuildAdgangsadresse("0217", "9781", "69A");

        Assert.Equal("02179781__69A______", kvhx);
    }

    [Fact]
    public void BuildEnhedsadresse_matcher_dawa_format_for_oester_alle()
    {
        var kvhx = KvHxIdBuilder.BuildEnhedsadresse("0751", "9554", "48", "2", "tv");

        Assert.Equal("07519554__48__2__tv", kvhx);
    }

    [Fact]
    public void BuildEnhedsadresse_matcher_dawa_format_uden_doer()
    {
        var kvhx = KvHxIdBuilder.BuildEnhedsadresse("0461", "5319", "93", "1", null);

        Assert.Equal("04615319__93__1____", kvhx);
    }
}
