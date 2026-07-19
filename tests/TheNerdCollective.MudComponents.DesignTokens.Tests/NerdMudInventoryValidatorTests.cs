using TheNerdCollective.Brand.Tnc;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdMudInventoryValidatorTests
{
    [Fact]
    public void ValidateDirectory_passes_for_committed_wave1_inventories()
    {
        var directory = NerdMudInventoryValidator.ResolveInventoryDirectory("9.7.0");
        var errors = NerdMudInventoryValidator.ValidateDirectory(directory);
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateHarvestCoverage_passes_for_committed_components_list()
    {
        var errors = NerdMudInventoryValidator.ValidateHarvestCoverage("9.7.0");
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("component: tabs\nclassification: P1\nstates:\n  - name: active\n")]
    [InlineData("component: alert\nclassification: P0\n")]
    public void ValidateFile_accepts_valid_inventory(string yaml)
    {
        Assert.Empty(NerdMudInventoryValidator.ValidateFile(yaml, "test.yaml"));
    }

    [Fact]
    public void ValidateFile_requires_states_for_p1()
    {
        var errors = NerdMudInventoryValidator.ValidateFile(
            "component: tabs\nclassification: P1\n",
            "bad.yaml");

        Assert.Contains(errors, error => error.Contains("states", StringComparison.Ordinal));
    }
}

public sealed class NerdMudStateParityToolsTests
{
    [Fact]
    public void Evaluate_reports_passing_wave1_components_for_tnc()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var result = NerdMudStateParityTools.Evaluate(options);

        Assert.True(result.Score >= 80, $"Expected score >= 80, got {result.Score}");
        Assert.Contains(result.Components, component => component.Component == "tabs" && component.Passes);
        Assert.Contains(result.Components, component => component.Component == "switch" && component.Passes);
        Assert.Contains(result.Components, component => component.Component == "pickerdate" && component.Passes);
        Assert.Contains(result.Components, component => component.Component == "select" && component.Passes);
        Assert.Contains(result.Components, component => component.Component == "alert" && component.Passes);
        Assert.Contains(result.Components, component => component.Component == "badge" && component.Passes);
        Assert.Contains(result.Components, component => component.Component == "progresslinear" && component.Passes);
        Assert.Contains(result.Components, component => component.Component == "button" && component.Passes);
        Assert.Contains(result.Components, component => component.Component == "list" && component.Passes);
        Assert.All(
            result.Components.Where(component => component.Component is "tabs" or "switch" or "checkbox" or "radio"),
            component => Assert.True(component.Passes, component.Component));
    }
}
