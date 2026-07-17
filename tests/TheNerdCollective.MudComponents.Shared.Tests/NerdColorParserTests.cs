using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.Shared.Tests;

public class NerdColorParserTests
{
    [Fact]
    public void Color_parser_resolves_css_variables()
    {
        var variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["--demo-color-forest"] = "#365C3A",
            ["--demo-color-forest-text"] = "#FFFFFF"
        };

        Assert.True(NerdColorParser.TryGetRgb("var(--demo-color-forest)", variables, out _, out _, out _));
        Assert.True(NerdColorParser.ContrastRatio(
            "var(--demo-color-forest)",
            "var(--demo-color-forest-text)",
            variables) >= 4.5);
    }
}
