using MudBlazor.Utilities;
using TheNerdCollective.Brand.Dnf;
using TheNerdCollective.Brand.Tnc;
using TheNerdCollective.MudComponents.DesignTokens;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public class MudBlazorDesignTokenCssGeneratorTests
{
    [Fact]
    public void Generate_emits_variables_and_mudblazor_component_variants()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf", UseCssLayer = true }
            .Add("forest", new NerdColorToken
            {
                Value = "#365C3A",
                ContrastText = "#FFFFFF",
                Hover = "#2D4D30"
            });

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(".dnf-forest", css);
        Assert.Contains("--dnf-color-forest: #365C3A", css);
        Assert.Contains(".dnf-forest[class*=\"mud-chip-filled\"]", css);
        Assert.DoesNotContain("[class*=\"mud-chip\"],", css);
        Assert.Contains(".dnf-forest[class*=\"mud-button-filled\"]", css);
        Assert.Contains(".dnf-forest[class*=\"mud-button-outlined\"]", css);
        Assert.Contains(".dnf-forest[class*=\"mud-button-text\"]", css);
        Assert.Contains(".dnf-forest[class*=\"mud-typography\"]", css);
        Assert.Contains(".dnf-forest.mud-disabled", css);
        Assert.Contains("!important", css);
    }

    [Fact]
    public void Generate_styles_chip_without_matching_chip_content()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf", UseImportantOverrides = false }
            .Add("kridt", new NerdColorToken { Value = "#E8E0D3", ContrastText = "#002D26" });

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(".dnf-kridt[class*=\"mud-chip-filled\"]", css);
        Assert.Contains(".dnf-kridt .mud-chip-content", css);
        Assert.DoesNotContain("[class*=\"mud-chip\"],", css);
        Assert.DoesNotContain("[class*=\"mud-chip\"] {", css);
        Assert.Contains(".dnf-kridt .mud-chip-filled:hover", css);
    }

    [Fact]
    public void Generate_uses_accent_color_for_outlined_and_text_button_variants()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf", UseImportantOverrides = false }
            .Add("graes", new NerdColorToken { Value = "#A6E54C", ContrastText = "#002D26" })
            .Add("skov", new NerdColorToken { Value = "#002D26", ContrastText = "#FDFAF3" });

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(".dnf-graes[class*=\"mud-button-outlined\"]", css);
        Assert.Contains("color: var(--dnf-color-graes); border-color: var(--dnf-color-graes-border);", css);
        Assert.Contains(".dnf-graes[class*=\"mud-button-text\"]", css);
        Assert.Contains("color: var(--dnf-color-graes);", css);
        Assert.Contains(".dnf-graes[class*=\"mud-typography\"]", css);
        Assert.Contains("color: var(--dnf-color-graes-content);", css);
        Assert.Contains("--dnf-color-graes-content: #002D26;", css);
        Assert.Contains("--dnf-color-skov-content: #002D26;", css);
    }

    [Fact]
    public void Generate_uses_coral_accent_for_tnc_primary_action_outlined_buttons()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains("--mud-palette-primary: var(--tnc-color-primary-action)", css);
        Assert.DoesNotContain(".tnc-primary-action[class*=\"mud-button-outlined\"]", css);
        Assert.Contains(".tnc-coral[class*=\"mud-button-outlined\"]", css);
    }

    [Fact]
    public void Generate_supports_customer_specific_token_sets()
    {
        var options = new NerdDesignTokenOptions { Prefix = "kunde" }
            .Add("sand", new NerdColorToken { Value = "#E8D8AD", ContrastText = "#2D2D2D" })
            .Add("sea-2", new NerdColorToken { Value = "#287A9E", ContrastText = "#FFFFFF" });

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(".kunde-sand", css);
        Assert.Contains(".kunde-sea-2", css);
        Assert.DoesNotContain(".kunde-forest", css);
    }

    [Fact]
    public void Generate_emits_dark_mode_and_semantic_role_variables()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf" }
            .Add("forest", new NerdColorToken
            {
                Value = "#365C3A",
                Light = "#4D7A50",
                Dark = "#203B25",
                Surface = "#F0F7F0",
                Content = "#19301D",
                Interactive = "#2D4D30"
            });

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains("[data-theme=\"dark\"] .dnf-forest", css);
        Assert.Contains("--dnf-color-forest-surface: #F0F7F0", css);
        Assert.Contains("--dnf-color-forest-content: #19301D", css);
        Assert.Contains("--dnf-color-forest-interactive: #2D4D30", css);
        Assert.Contains(".dnf-forest.mud-selected", css);
        Assert.Contains("[aria-pressed=\"true\"]", css);
    }

    [Fact]
    public void Generate_maps_mud_palette_via_theme_factory_not_css_brand_root()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf" }
            .Add("forest", new NerdColorToken { Value = "#365C3A", ContrastText = "#FFFFFF" });
        NerdDnfDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);
        var theme = NerdMudThemeFactory.Create(options);

        Assert.DoesNotContain(".dnf-mud-brand, .mud-theme-provider {", css);
        AssertPaletteSlotSet(theme.PaletteLight.Primary);
        AssertPaletteSlotSet(theme.PaletteLight.Secondary);
        AssertPaletteSlotSet(theme.PaletteLight.Surface);
        AssertPaletteSlotSet(theme.PaletteLight.TableHover);
        AssertPaletteSlotSet(theme.PaletteLight.OverlayLight);
        AssertPaletteSlotSet(theme.PaletteLight.Skeleton);
        AssertPaletteSlotSet(theme.PaletteLight.ActionDisabledBackground);
        Assert.DoesNotContain("--mud-palette-secondary: var(--dnf-color-forest)", css);
    }

    private static void AssertPaletteSlotSet(MudColor color) =>
        Assert.NotEqual(default(MudColor), color);

    [Fact]
    public void Generate_legacy_mode_flattens_palette_onto_token_class()
    {
        var options = new NerdDesignTokenOptions
        {
            Prefix = "dnf",
            UsePaletteFirstAdapter = false
        }
            .Add("forest", new NerdColorToken { Value = "#365C3A", ContrastText = "#FFFFFF" });

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains("--mud-palette-primary: var(--dnf-color-forest)", css);
        Assert.Contains("--mud-palette-secondary: var(--dnf-color-forest)", css);
    }

    [Fact]
    public void Generate_emits_pattern_rules_for_inputs_tables_and_navigation()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf" }
            .Add("forest", new NerdColorToken { Value = "#365C3A", ContrastText = "#FFFFFF" });

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains("[class*=\"mud-text-field\"]", css);
        Assert.Contains(".dnf-forest.mud-switch .mud-switch-base", css);
        Assert.Contains("[class*=\"mud-table\"]", css);
        Assert.Contains("[class*=\"mud-data-grid\"]", css);
        Assert.Contains("[class*=\"mud-nav-link\"]", css);
        Assert.Contains("[class*=\"mud-icon\"]", css);
        Assert.Contains("[class*=\"mud-popover\"] [class*=\"mud-list-item\"]", css);
        Assert.Contains("[class*=\"mud-rating\"]", css);
        Assert.Contains(".dnf-forest .mud-checkbox .mud-icon-button", css);
    }

    [Fact]
    public void Generate_nav_link_inherits_under_muted_content_palette_first()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc" }
            .Add("ink", new NerdColorToken { Value = "#111827", ContrastText = "#FFFFFF" })
            .Alias("muted-content", "ink");

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(".tnc-muted-content {", css);
        Assert.Contains("--mud-palette-text-secondary: var(--tnc-color-muted-content-content)", css);
        Assert.Contains(".tnc-muted-content .mud-nav-link .mud-nav-link-text", css);
        Assert.Contains("color: inherit", css);
        Assert.DoesNotContain(".tnc-muted-content[class*=\"mud-button-filled\"]", css);
        Assert.DoesNotContain(".tnc-muted-content .mud-nav-link:hover .mud-nav-link-text", css);
    }

    [Fact]
    public void Generate_sidebar_recipe_styles_nav_links()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc" }
            .Add("snow", new NerdColorToken { Value = "#FFFFFF", ContrastText = "#111827" })
            .Add("ink", new NerdColorToken { Value = "#111827", ContrastText = "#FFFFFF" })
            .Add("coral", new NerdColorToken { Value = "#F27271", ContrastText = "#FFFFFF" })
            .AddRecipe(NerdDesignSystemUi.SidebarRecipe, new NerdDesignTokenRecipe("snow", "ink", "coral"));

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(".tnc-recipe-sidebar .mud-nav-link:hover", css);
        Assert.Contains(".tnc-recipe-sidebar .mud-nav-link.active .mud-nav-link-icon", css);
        Assert.Contains("font-size: 0.875rem !important;", css);
        Assert.Contains("#F27271", css);
    }

    [Fact]
    public void Generate_scopes_token_rules_to_portaled_popovers()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf" }
            .Add("forest", new NerdColorToken { Value = "#365C3A", ContrastText = "#FFFFFF" });

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(".dnf-forest.mud-popover-open", css);
        Assert.Contains(".dnf-forest.mud-popover-open :where([class*=\"mud-select\"])", css);
    }

    [Fact]
    public void Generate_can_disable_portaled_popover_scope()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf", EnablePortalTokenScope = false }
            .Add("forest", new NerdColorToken { Value = "#365C3A", ContrastText = "#FFFFFF" });

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.DoesNotContain(".dnf-forest.mud-popover-open", css);
    }

    [Fact]
    public void Generate_computes_contrast_text_when_omitted()
    {
        var options = new NerdDesignTokenOptions { Prefix = "test" }
            .Add("sun", new NerdColorToken { Value = "#FFFFFF" });

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains("--test-color-sun-text: #1F2937", css);
    }

    [Fact]
    public void Generate_surface_aliases_emit_surface_styles_without_mudblazor_descendant_rules()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf" }
            .Add("kridt-lys", new NerdColorToken { Value = "#FDFAF3", ContrastText = "#002D26" })
            .Alias("page-surface", "kridt-lys")
            .Alias("brand-chrome", "kridt-lys")
            .Alias("secondary-action", "kridt-lys");

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(".dnf-page-surface {", css);
        Assert.Contains("background-color: var(--dnf-color-page-surface-surface)", css);
        Assert.Contains("--mud-palette-surface: var(--dnf-color-page-surface-surface)", css);
        Assert.Contains(".dnf-page-surface.mud-popover-open .mud-selected-item", css);
        Assert.Contains(".dnf-page-surface.mud-popover-open .mud-list-item.mud-selected", css);
        Assert.Contains("color-mix(in srgb, var(--dnf-color-nav-item-active) 12%, transparent)", css);
        Assert.Contains("color: var(--dnf-color-page-surface-content)", css);
        Assert.Contains(":root .dnf-page-surface.mud-button-outlined", css);
        Assert.Contains(":root .dnf-page-surface.mud-button-filled", css);
        Assert.Contains("color: var(--dnf-color-secondary-action)", css);
        Assert.Contains(":root .dnf-brand-chrome.mud-button-outlined", css);
    }

    [Fact]
    public void Generate_brand_chrome_surface_uses_on_brand_chrome_for_outlined_and_text_buttons()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(":root .tnc-brand-chrome.mud-button-outlined", css);
        Assert.Contains(":root .tnc-brand-chrome.mud-button-text", css);
        var brandChromeOutlined = css.Substring(
            css.IndexOf(":root .tnc-brand-chrome.mud-button-outlined", StringComparison.Ordinal),
            200);
        Assert.Contains("color: var(--tnc-color-on-brand-chrome)", brandChromeOutlined);
        Assert.DoesNotContain(":root .tnc-brand-chrome .mud-button-outlined", css);
        Assert.DoesNotContain(":root .tnc-page-surface .mud-button-outlined", css);
        Assert.Contains(":root .tnc-page-surface.mud-button-outlined", css);
        Assert.Contains("color: var(--tnc-color-secondary-action)", css);
    }

    [Fact]
    public void Generate_brand_chrome_surface_uses_on_brand_chrome_for_inputs()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(":root .tnc-brand-chrome.mud-input-control :where([class*=\"mud-input-label\"])", css);
        Assert.Contains(":root .tnc-brand-chrome.mud-input-control :where([class*=\"mud-input-slot\"])", css);
        Assert.Contains(
            ":root .tnc-brand-chrome.mud-picker .mud-input-control > .mud-input-control-input-container > [class*=\"mud-input-label\"]",
            css);
        Assert.DoesNotContain(":root .tnc-page-surface .tnc-brand-chrome.mud-input-control", css);

        var labelRule = css.Substring(
            css.IndexOf(":root .tnc-brand-chrome.mud-input-control :where([class*=\"mud-input-label\"])", StringComparison.Ordinal),
            120);
        Assert.Contains("color: var(--tnc-color-on-brand-chrome)", labelRule);
    }

    [Fact]
    public void Generate_isolates_css_and_emits_design_system_helpers()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf", UseCssLayer = true }
            .Add("forest", new NerdColorToken { Value = "#365C3A" })
            .Alias("primary-action", "forest")
            .AddRadius("card", "12px")
            .AddShadow("elevated", "0 4px 16px rgba(0,0,0,.16)")
            .AddSpacing("4", "16px");

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains("@layer nerd-design-tokens", css);
        Assert.Contains(".dnf-primary-action", css);
        Assert.Contains("--mud-palette-primary: var(--dnf-color-primary-action)", css);
        Assert.DoesNotContain(".dnf-primary-action[class*=\"mud-button-filled\"]", css);
        Assert.Contains(".dnf-primary-action .mud-tab-slider", css);
        Assert.Contains(".dnf-radius-card", css);
        Assert.Contains(".dnf-shadow-elevated", css);
        Assert.Contains(".dnf-space-4", css);
        Assert.Contains("--dnf-space-4: 16px", css);
        Assert.Contains(".dnf-pa-4", css);
    }

    [Fact]
    public void Generate_emits_opacity_overlay_classes()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf", UseImportantOverrides = false }
            .Add("skov", new NerdColorToken { Value = "#002D26", ContrastText = "#FDFAF3" })
            .AddOpacity("watermark", new NerdOpacityToken("skov", 0.12));

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(".dnf-opacity-watermark", css);
        Assert.Contains("color-mix(in srgb, var(--dnf-color-skov)", css);
        Assert.Contains("var(--dnf-color-skov-content)", css);
    }

    [Fact]
    public void Accessibility_check_reports_wcag_aa_status()
    {
        var options = new NerdDesignTokenOptions { Prefix = "test" }
            .Add("light", new NerdColorToken { Value = "#FFFFFF", ContrastText = "#000000" });

        var result = NerdDesignTokenTools.CheckAccessibility(options).Single();

        Assert.True(result.MeetsAa);
        Assert.True(result.Light.ContrastRatio >= 4.5);
        Assert.Equal("2.1", result.WcagVersion);
    }

    [Fact]
    public void Accessibility_warnings_include_mode_ratio_and_recommendation()
    {
        var options = new NerdDesignTokenOptions { Prefix = "test", WcagVersion = "2.1" }
            .Add("sand", new NerdColorToken { Value = "#E8D8AD", ContrastText = "#D8C58E" });

        var warnings = NerdDesignTokenTools.GetAccessibilityWarnings(options);

        Assert.NotEmpty(warnings);
        Assert.Contains(warnings, warning => warning.TokenName == "sand");
        Assert.Contains(warnings, warning => warning.Mode is "light" or "dark");
        Assert.False(string.IsNullOrWhiteSpace(warnings[0].RecommendedForeground));
    }

    [Fact]
    public void Stitch_export_contains_customer_tokens()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf" }
            .Add("forest", new NerdColorToken { Value = "#365C3A" });

        var design = NerdDesignTokenTools.ExportStitchDesignMd(options);

        Assert.Contains("# Design tokens", design);
        Assert.Contains("forest", design);
        Assert.Contains("#365C3A", design);
    }

    [Fact]
    public void Stitch_export_includes_typography_roles_when_provided()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf" }
            .Add("forest", new NerdColorToken { Value = "#365C3A" });
        var typography = new Dictionary<string, string>
        {
            ["H1"] = "clamp(1.75rem, 3vw, 2.5rem)"
        };

        var design = NerdDesignTokenTools.ExportStitchDesignMd(options, typography);

        Assert.Contains("## Typography", design);
        Assert.Contains("H1", design);
        Assert.Contains("clamp(1.75rem, 3vw, 2.5rem)", design);
    }

    [Fact]
    public void Generate_emits_pairing_surface_and_recipe_outlined_rules()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(".nerd-pairing-surface .mud-button-outlined", css);
        Assert.Contains("--nerd-pairing-surface-color", css);
        Assert.Contains(".nerd-pairing-surface--swatch", css);
        Assert.Contains(".tnc-recipe-hero .mud-button-outlined", css);
        Assert.Contains("border-color: currentColor", css);
    }

    [Fact]
    public void Generate_catalog_chrome_resets_non_recipe_surfaces_inside_accent_scope()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(
            ".nerd-catalog-chrome [data-nerd-accent=\"tnc-primary-action\"] .mud-paper:not([class*=\"-recipe-\"])",
            css);
        Assert.Contains(
            ".nerd-catalog-chrome [data-nerd-accent=\"tnc-primary-action\"] .mud-typography:not(.mud-tab):not(:is([class*=\"-recipe-\"] *))",
            css);
        Assert.Contains(":not(:is([data-nerd-accent] [data-nerd-token] *))", css);
        Assert.Contains(".tnc-recipe-hero.mud-card .mud-typography", css);
    }

    [Fact]
    public void Generate_brand_chrome_surface_paints_content_on_tables_and_typography()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(":root .tnc-brand-chrome[class*=\"mud-typography\"]", css);
        Assert.Contains(":root .tnc-brand-chrome[class*=\"mud-table\"]", css);
        Assert.DoesNotContain(":root .tnc-page-surface :where([class*=\"mud-table-cell\"])", css);
        Assert.Contains("color: var(--tnc-color-on-brand-chrome)", css);
        Assert.Contains("--mud-palette-text-primary: var(--tnc-color-brand-chrome-content)", css);
    }

    [Fact]
    public void Generate_on_brand_chrome_content_intent_paints_typography_without_page_surface_input_values()
    {
        var options = new NerdDesignTokenOptions { Prefix = "tnc", UseImportantOverrides = false };
        NerdTncDesignTokenPresets.Apply(options);

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains(".tnc-on-brand-chrome[class*=\"mud-typography\"]", css);
        var inputSlotIndex = css.IndexOf(
            ".tnc-on-brand-chrome :where([class*=\"mud-input-slot\"])",
            StringComparison.Ordinal);
        Assert.True(inputSlotIndex >= 0);
        var inputSlotRule = css.Substring(inputSlotIndex, 180);
        Assert.Contains("color: var(--tnc-color-on-brand-chrome-content)", inputSlotRule);
        Assert.DoesNotContain("page-surface-content", inputSlotRule);
    }

    [Fact]
    public void Generate_emits_dark_mode_recipe_variants()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf" }
            .Add("kridt", new NerdColorToken { Value = "#E8E0D3", ContrastText = "#002D26" })
            .Add("skov", new NerdColorToken
            {
                Value = "#002D26",
                Dark = "#001A16",
                ContrastText = "#FDFAF3"
            });

        options.AddRecipe("hero", new NerdDesignTokenRecipe("kridt", "skov", "skov"));

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.Contains("[data-theme=\"dark\"] .dnf-recipe-hero", css);
    }

    [Fact]
    public void Catalog_options_have_sensible_defaults()
    {
        var options = new NerdDesignTokenOptions();

        Assert.True(options.EnableCatalogPage);
        Assert.Equal("/nerd-design-tokens", options.CatalogRoute);
        Assert.True(options.RestrictCatalogToDevelopment);
    }

    [Fact]
    public void Generate_respects_use_important_overrides_option()
    {
        var options = new NerdDesignTokenOptions { Prefix = "dnf", UseImportantOverrides = false }
            .Add("forest", new NerdColorToken { Value = "#365C3A", ContrastText = "#FFFFFF" });

        var css = MudBlazorDesignTokenCssGenerator.Generate(options);

        Assert.DoesNotContain("!important", css);
    }

    [Fact]
    public void Accessibility_check_uses_dark_contrast_text_when_configured()
    {
        var options = new NerdDesignTokenOptions { Prefix = "test" }
            .Add("forest", new NerdColorToken
            {
                Value = "#365C3A",
                Dark = "#203B25",
                ContrastText = "#FFFFFF",
                DarkContrastText = "#F8FAFC"
            });

        var result = NerdDesignTokenTools.CheckAccessibility(options).Single();

        Assert.Equal("#F8FAFC", result.Dark.Foreground);
    }

    [Theory]
    [InlineData("#FFFFFF", true)]
    [InlineData("rgb(255, 255, 255)", true)]
    [InlineData("hsl(120, 50%, 50%)", true)]
    public void Color_parser_supports_hex_rgb_and_hsl(string color, bool expected)
    {
        Assert.Equal(expected, TheNerdCollective.MudComponents.Shared.NerdColorParser.TryGetRgb(color, out _, out _, out _));
    }

    [Fact]
    public void Color_parser_resolves_token_css_variables()
    {
        var variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["--test-color-forest"] = "#365C3A",
            ["--test-color-forest-text"] = "#FFFFFF"
        };

        Assert.True(TheNerdCollective.MudComponents.Shared.NerdColorParser.TryGetRgb(
            "var(--test-color-forest)", variables, out _, out _, out _));
    }

    [Theory]
    [InlineData("Sand")]
    [InlineData("sand color")]
    [InlineData("sand;")]
    public void Add_rejects_invalid_token_names(string name)
    {
        var options = new NerdDesignTokenOptions();

        Assert.Throws<ArgumentException>(() =>
            options.Add(name, new NerdColorToken { Value = "red", ContrastText = "white" }));
    }
}
