using TheNerdCollective.Brand.Dnf;
using TheNerdCollective.MudComponents.DesignTokens;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public class NerdTokenPackDiffTests
{
    public NerdTokenPackDiffTests() => NerdBrandPackTestBootstrap.EnsureRegistered();
    [Fact]
    public void Compare_detects_added_modified_and_removed_colors()
    {
        var baseline = NerdTokenPack.FromPreset("demo");
        var current = NerdTokenPack.FromOptions(baseline.ToOptions(), "current");
        current.ToOptions().Add("extra", new NerdColorToken { Value = "#112233", ContrastText = "#FFFFFF" });
        var modifiedPack = NerdTokenPack.FromOptions(current.ToOptions(), "current");
        modifiedPack.ToOptions().Colors.TryGetValue("violet", out _);

        var diff = NerdTokenPackDiff.Compare(baseline, NerdTokenPack.FromOptions(
            new NerdDesignTokenOptions { Prefix = "demo" }
                .Add("violet", new NerdColorToken { Value = "#000000", ContrastText = "#FFFFFF" })
                .Add("extra", new NerdColorToken { Value = "#112233", ContrastText = "#FFFFFF" }),
            "current"));

        Assert.Contains(diff, entry => entry.Kind == NerdTokenPackDiffKind.Added && entry.Name == "extra");
        Assert.Contains(diff, entry => entry.Kind == NerdTokenPackDiffKind.Modified && entry.Name == "violet");
        Assert.Contains(diff, entry => entry.Kind == NerdTokenPackDiffKind.Removed || entry.Kind == NerdTokenPackDiffKind.Modified);
    }

    [Fact]
    public void ExportPackJson_includes_aliases_and_recipes()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf" };
        NerdDnfDesignTokenPresets.Apply(options);

        var json = NerdDesignTokenTools.ExportPackJson(options);

        Assert.Contains("\"aliases\"", json);
        Assert.Contains("primary-action", json);
        Assert.Contains("\"recipes\"", json);
        Assert.Contains("hero", json);
    }

    [Fact]
    public void Alias_usage_tools_lists_recipe_references()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf" };
        NerdDnfDesignTokenPresets.Apply(options);

        var usages = NerdAliasUsageTools.GetUsages(options, "primary-action", "himmel");

        Assert.Contains(usages, usage => usage.Contains("hero", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ExportTokensStudioJson_emits_color_tokens()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf" };
        NerdDnfDesignTokenPresets.Apply(options);

        var json = NerdDesignTokenTools.ExportTokensStudioJson(options);

        Assert.Contains("\"color\"", json);
        Assert.Contains("\"blad\"", json);
        Assert.Contains("#0E4D3A", json);
    }
}
