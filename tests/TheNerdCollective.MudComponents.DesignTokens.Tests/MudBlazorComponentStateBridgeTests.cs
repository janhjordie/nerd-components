using TheNerdCollective.Brand.Tnc;
using TheNerdCollective.MudComponents.DesignTokens;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public class MudBlazorComponentStateBridgeTests
{
    [Fact]
    public void Generate_does_not_flatten_mud_tabs_with_accent_text_pattern()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.DoesNotContain(".tnc-primary-action[class*=\"mud-tab\"]", css);
        Assert.Contains(".tnc-primary-action .mud-tab.mud-tab-active", css);
        Assert.Contains(".tnc-primary-action .mud-tab:not(.mud-tab-active)", css);
        Assert.Contains(".tnc-primary-action .mud-tab-slider", css);
    }

    [Fact]
    public void Generate_maps_inactive_tabs_on_action_intents_to_page_surface_content()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        var marker = ".tnc-primary-action .mud-tab:not(.mud-tab-active) {";
        var start = css.IndexOf(marker, StringComparison.Ordinal);
        Assert.True(start >= 0);
        var end = css.IndexOf('}', start);
        var block = css[start..end];
        Assert.Contains("color: var(--tnc-color-page-surface-content)", block);
    }

    [Fact]
    public void Generate_uses_correct_switch_checked_track_selector()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(".tnc-primary-action.mud-switch .mud-switch-base.mud-checked + .mud-switch-track", css);
        Assert.DoesNotContain(".tnc-primary-action .mud-switch.mud-checked + .mud-switch-track", css);
    }

    [Fact]
    public void Generate_does_not_paint_switch_base_as_thumb()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.DoesNotContain(".tnc-primary-action .mud-switch .mud-button-root {", css);
        Assert.Contains(".tnc-primary-action.mud-switch .mud-switch-base.mud-button-root", css);
        Assert.Contains(".tnc-primary-action.mud-switch .mud-switch-thumb-medium", css);
    }

    [Fact]
    public void Generate_maps_mud_primary_text_via_palette_not_bulk_pattern()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(".tnc-primary-action {", css);
        Assert.Contains("--mud-palette-primary: var(--tnc-color-primary-action)", css);
        Assert.DoesNotContain(".tnc-primary-action[class*=\"mud-primary-text\"]", css);
        Assert.DoesNotContain(".tnc-primary-action[class*=\"mud-button-filled\"]", css);
        Assert.Contains(".tnc-primary-action.mud-button-filled {", css);
        Assert.Contains(".tnc-primary-action.mud-button-outlined {", css);
        Assert.Contains(".tnc-primary-action.mud-chip-outlined {", css);
        Assert.DoesNotContain(".tnc-primary-action[class*=\"mud-chip-outlined\"]", css);
        Assert.Contains(".tnc-success.mud-chip-filled {", css);
        Assert.DoesNotContain(".tnc-success[class*=\"mud-chip-filled\"]", css);
    }

    [Fact]
    public void Generate_maps_toggle_items_to_channel_paint_not_structure_content()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.DoesNotContain(".tnc-coral .mud-toggle-item.mud-button-outlined", css);
        Assert.Contains(".tnc-coral[class*=\"mud-toggle-item\"][class*=\"mud-button-outlined\"],", css);
        Assert.Contains(".tnc-coral[class*=\"mud-toggle-item\"][class*=\"mud-button-filled\"],", css);

        var outlinedStart = css.IndexOf(".tnc-coral[class*=\"mud-toggle-item\"][class*=\"mud-button-outlined\"],", StringComparison.Ordinal);
        Assert.True(outlinedStart >= 0);
        var outlinedEnd = css.IndexOf('}', outlinedStart);
        var outlinedBlock = css[outlinedStart..outlinedEnd];
        Assert.Contains("color: var(--tnc-color-coral)", outlinedBlock);

        var filledStart = css.IndexOf(".tnc-coral[class*=\"mud-toggle-item\"][class*=\"mud-button-filled\"],", StringComparison.Ordinal);
        Assert.True(filledStart >= 0);
        var filledEnd = css.IndexOf('}', filledStart);
        var filledBlock = css[filledStart..filledEnd];
        Assert.Contains("background-color: var(--tnc-color-coral)", filledBlock);
        Assert.Contains("color: var(--tnc-color-coral-text)", filledBlock);
    }

    [Fact]
    public void Generate_does_not_apply_input_color_flatten_to_mud_switch()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.DoesNotContain(".tnc-primary-action[class*=\"mud-switch\"]", css);
        Assert.Contains(".tnc-primary-action.mud-switch .mud-switch-base,", css);
    }

    [Fact]
    public void MudBlazorVersion_matches_package_reference()
    {
        var options = new NerdDesignTokenOptions();
        Assert.Equal("9.7.0", options.MudBlazorVersion);
    }

    [Fact]
    public void Generate_catalog_chrome_tabs_use_state_bridge_not_band_aids()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.DoesNotContain(".nerd-catalog-chrome [data-nerd-accent=\"tnc-primary-action\"] .mud-tab:not(.mud-tab-active)", css);
        Assert.DoesNotContain(".nerd-catalog-chrome [data-nerd-accent=\"tnc-primary-action\"] .mud-tab.mud-tab-active", css);
        Assert.DoesNotContain(".nerd-catalog-chrome [data-nerd-accent=\"tnc-primary-action\"] .mud-tab-slider", css);
        Assert.Contains(".tnc-primary-action .mud-tab.mud-tab-active", css);
    }

    [Fact]
    public void Generate_catalog_toolbar_switch_uses_primary_action_scope_not_band_aids()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.DoesNotContain("[data-nerd-style-guard=\"catalog-toolbar\"] .mud-switch-base.mud-checked + .mud-switch-track", css);
        Assert.DoesNotContain("[data-nerd-style-guard=\"catalog-toolbar\"] .tnc-primary-action .mud-switch .mud-switch-track", css);
        Assert.Contains("[data-nerd-style-guard=\"catalog-toolbar\"] label", css);
    }

    [Fact]
    public void Generate_switch_track_uses_page_surface_mix_and_full_opacity()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(".tnc-primary-action.mud-switch .mud-switch-track,", css);
        Assert.Contains(
            "background-color: color-mix(in srgb, var(--tnc-color-primary-action) 30%, var(--tnc-color-page-surface))",
            css);
        Assert.Contains(
            ".tnc-primary-action.mud-switch .mud-switch-base.mud-checked + .mud-switch-track,",
            css);
        Assert.Contains("background-color: var(--tnc-color-primary-action)", css);
        Assert.Contains("opacity: 1", css);
        Assert.Contains("border: 1px solid var(--tnc-color-primary-action-hover)", css);
    }

    [Fact]
    public void Generate_does_not_emit_malformed_css_block_terminators()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.DoesNotContain("!important;\n}}", css);
        Assert.DoesNotContain("transparent;\n}}", css);
    }

    [Fact]
    public void Generate_maps_select_selected_item_to_channel_token()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(".tnc-coral.mud-popover-open .mud-selected-item", css);
        Assert.Contains("background-color: var(--tnc-color-coral)", css);
    }

    [Fact]
    public void Generate_maps_picker_selected_day_to_channel_token()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(".tnc-coral .mud-picker-calendar .mud-day.mud-selected", css);
        Assert.Contains("background-color: var(--tnc-color-coral)", css);
        Assert.Contains(".tnc-coral .mud-picker-calendar .mud-day.mud-current:not(.mud-selected)", css);
    }

    [Fact]
    public void Generate_maps_input_slot_values_to_page_surface_content_in_token_preview()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(".tnc-navy[data-nerd-token] :where([class*=\"mud-input-slot\"]),", css);

        var marker = ".tnc-navy[data-nerd-token] :where([class*=\"mud-input-slot\"]),";
        var start = css.IndexOf(marker, StringComparison.Ordinal);
        Assert.True(start >= 0);
        var end = css.IndexOf('}', start);
        var block = css[start..end];
        Assert.Contains("color: var(--tnc-color-page-surface-content)", block);
        Assert.Contains("color-mix(in srgb, var(--tnc-color-navy) 65%", css);
    }
}
