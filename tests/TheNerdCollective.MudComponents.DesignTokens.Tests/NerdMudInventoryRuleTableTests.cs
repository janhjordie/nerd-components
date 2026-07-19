using TheNerdCollective.Brand.Tnc;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdMudInventoryRuleTableTests
{
    [Fact]
    public void Load_reads_committed_inventory_entries()
    {
        var entries = NerdMudInventoryRuleTable.Load("9.7.0");

        Assert.True(entries.Count >= 74);
        Assert.Contains(entries, entry => entry.Component == "button");
        Assert.Contains(entries, entry => entry.Component == "list");
        Assert.Contains(entries, entry => entry.Component == "stepper");
        Assert.Contains(entries, entry => entry.Component == "iconbutton");
        Assert.Contains(entries, entry => entry.Component == "fab");
        Assert.Contains(entries, entry => entry.Component == "buttongroup");
        Assert.Contains(entries, entry => entry.Component == "togglegroup");
        Assert.Contains(entries, entry => entry.Component == "card");
        Assert.Contains(entries, entry => entry.Component == "drawer");
        Assert.Contains(entries, entry => entry.Component == "dropzone");
        Assert.Contains(entries, entry => entry.Component == "fabmenu");
        Assert.Contains(entries, entry => entry.Component == "chart");
        Assert.Contains(entries, entry => entry.Component == "rating");
        Assert.Contains(entries, entry => entry.Component == "expansionpanel");
        Assert.Contains(entries, entry => entry.Component == "datagrid");
        Assert.Contains(entries, entry => entry.Component == "carousel");
        Assert.Contains(entries, entry => entry.Component == "fileupload");
        Assert.Contains(entries, entry => entry.Component == "pagination");
        Assert.Contains(entries, entry => entry.Component == "table");
        Assert.Contains(entries, entry => entry.Component == "treeview");
        Assert.Contains(entries, entry => entry.Component == "timeline");
        Assert.Contains(entries, entry => entry.Component == "skeleton");
        Assert.Contains(entries, entry => entry.Component == "paper");
        Assert.Contains(entries, entry => entry.Component == "overlay");
        Assert.Contains(entries, entry => entry.Component == "toolbar");
        Assert.Contains(entries, entry => entry.Component == "appbar");
        Assert.Contains(entries, entry => entry.Component == "collapse");
        Assert.Contains(entries, entry => entry.Component == "navmenu");
        Assert.Contains(entries, entry => entry.Component == "menu");
        Assert.Contains(entries, entry => entry.Component == "snackbar");
        Assert.Contains(entries, entry => entry.Component == "slider");
        Assert.Contains(entries, entry => entry.Component == "dialog");
        Assert.Contains(entries, entry => entry.Component == "popover");
        Assert.Contains(entries, entry => entry.Component == "divider");
        Assert.Contains(entries, entry => entry.Component == "simpletable");
        Assert.Contains(entries, entry => entry.Component == "link");
        Assert.Contains(entries, entry => entry.Component == "icons");
        Assert.Contains(entries, entry => entry.Component == "input");
        Assert.Contains(entries, entry => entry.Component == "field");
        Assert.Contains(entries, entry => entry.Component == "inputlabel");
        Assert.Contains(entries, entry => entry.Component == "inputcontrol");
        Assert.Contains(entries, entry => entry.Component == "form");
        Assert.Contains(entries, entry => entry.Component == "picker");
        Assert.Contains(entries, entry => entry.Component == "pickertime");
        Assert.Contains(entries, entry => entry.Component == "pickercolor");
        Assert.Contains(entries, entry => entry.Component == "grid");
        Assert.Contains(entries, entry => entry.Component == "layout");
        Assert.Contains(entries, entry => entry.Component == "flexbreak");
        Assert.Contains(entries, entry => entry.Component == "pagecontentnavigation");
        Assert.Contains(entries, entry => entry.Component == "tabs");
        Assert.Contains(entries, entry => entry.Component == "alert");
        Assert.Contains(entries, entry => entry.Component == "badge");
        Assert.Contains(entries, entry => entry.Component == "dialog");
        Assert.Contains(entries, entry => entry.Component == "progresslinear");
        Assert.Contains(entries, entry => entry.Component == "tooltip");
        Assert.Contains(entries, entry => entry.RequiredSelectors.Contains(".mud-tab-slider"));
    }

    [Fact]
    public void ValidateGeneratedCss_passes_for_tnc_preset()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var errors = NerdMudInventoryRuleTable.ValidateGeneratedCss(options);

        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateGeneratedCss_passes_for_dnf_preset()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf", UseImportantOverrides = false };
        TheNerdCollective.Brand.Dnf.NerdDnfDesignTokenPresets.Apply(options);

        var errors = NerdMudInventoryRuleTable.ValidateGeneratedCss(options);

        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateGeneratedCss_covers_all_inventory_entries_with_requirements()
    {
        var entries = NerdMudInventoryRuleTable.Load("9.7.0");
        var withRequirements = entries
            .Where(entry => entry.RequiredSelectors.Count > 0 || entry.ForbiddenSelectors.Count > 0)
            .ToList();

        Assert.True(withRequirements.Count >= 64, $"Expected >= 64 inventoried rule entries, got {withRequirements.Count}.");
    }

    [Fact]
    public void Generate_does_not_emit_catalog_state_band_aids()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.DoesNotContain("[data-nerd-style-guard=\"catalog-toolbar\"] .mud-switch .mud-switch-track {", css);
        Assert.DoesNotContain("[data-nerd-style-guard=\"catalog-toolbar\"] .mud-switch-base.mud-checked + .mud-switch-track", css);
        Assert.DoesNotContain("[data-nerd-style-guard=\"catalog-toolbar\"] .tnc-primary-action .mud-switch .mud-switch-track", css);
        Assert.Contains("[data-nerd-style-guard=\"catalog-toolbar\"] label", css);
        Assert.DoesNotContain(".nerd-catalog-chrome [data-nerd-accent=\"tnc-primary-action\"] .mud-tab.mud-tab-active", css);
        Assert.DoesNotContain(".nerd-catalog-chrome [data-nerd-accent=\"tnc-primary-action\"] .mud-tab-slider", css);
        Assert.Contains("[data-nerd-style-guard=\"catalog-toolbar\"] label", css);
    }

    [Fact]
    public void WriteGeneratedArtifacts_updates_committed_markdown()
    {
        NerdMudInventoryRuleTable.WriteGeneratedArtifacts("9.7.0");

        var path = Path.Combine(
            NerdMudInventoryValidator.ResolveInventoryDirectory("9.7.0"),
            "..",
            "generated-rule-table.md");

        Assert.True(File.Exists(path));
        var markdown = File.ReadAllText(path);
        Assert.Contains("|tabs|", markdown);
        Assert.Contains(".mud-tab-slider", markdown);
    }
}
