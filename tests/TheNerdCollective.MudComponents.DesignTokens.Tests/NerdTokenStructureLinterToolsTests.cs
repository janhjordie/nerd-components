namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdTokenStructureLinterToolsTests
{
    [Fact]
    public void Analyze_flags_missing_alias_reference()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc" };
        options.Add("ink", new NerdColorToken { Value = "#112233" });
        options.Alias("ghost", "missing");

        var issues = NerdTokenStructureLinterTools.Analyze(options);

        Assert.Contains(issues, issue => issue.Code == NerdTokenLintIssueCode.MissingReference);
    }

    [Fact]
    public void Analyze_flags_duplicate_hex_values()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc" };
        options.Add("one", new NerdColorToken { Value = "#AABBCC" });
        options.Add("two", new NerdColorToken { Value = "#AABBCC" });
        options.AddRecipe("card", new NerdDesignTokenRecipe("one", "two"));

        var issues = NerdTokenStructureLinterTools.Analyze(options);

        Assert.Contains(issues, issue => issue.Code == NerdTokenLintIssueCode.DuplicateValue);
    }
}
