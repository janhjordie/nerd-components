using TheNerdCollective.Brand.Tnc;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdComponentFamilyToolsTests
{
    [Fact]
    public void LoadAll_includes_picker_day_selected_primary_action_mapping()
    {
        var parts = NerdComponentFamilyTools.LoadAll();

        Assert.Contains(
            parts,
            part => part.Family == "picker" && part.PartId == "day-selected" && part.IntentAlias == "primary-action");
        Assert.Contains(
            parts,
            part => part.Family == "toggle" && part.PartId == "switch-channel" && part.IntentAlias == "primary-action");
    }

    [Fact]
    public void ResolveBindings_maps_intent_alias_to_token_css_variable()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var bindings = NerdComponentFamilyTools.ResolveBindings(options);
        var daySelected = bindings.Single(binding => binding.Family == "picker" && binding.PartId == "day-selected");

        Assert.Equal("primary-action", daySelected.IntentAlias);
        Assert.Equal("--tnc-color-coral", daySelected.CssVariable);
        Assert.False(string.IsNullOrWhiteSpace(daySelected.SampleColor));
    }
}
