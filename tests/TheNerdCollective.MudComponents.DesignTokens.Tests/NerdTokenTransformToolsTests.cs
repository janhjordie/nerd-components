namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdTokenTransformToolsTests
{
    [Fact]
    public void ApplyTransforms_adds_derived_color_token()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc" };
        options.Add("navy", new NerdColorToken { Value = "#001B3A" });
        options.AddTransform("navy-soft", new NerdTokenTransform("navy", "lighten", 0.2));

        NerdTokenTransformTools.ApplyTransforms(options, options.Transforms);

        Assert.True(options.Colors.ContainsKey("navy-soft"));
        Assert.StartsWith("#", options.Colors["navy-soft"].Value);
    }
}
