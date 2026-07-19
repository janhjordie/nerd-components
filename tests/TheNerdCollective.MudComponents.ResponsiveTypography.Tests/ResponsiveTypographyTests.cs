using MudBlazor;
using TheNerdCollective.MudComponents.ResponsiveTypography;

namespace TheNerdCollective.MudComponents.ResponsiveTypography.Tests;

public class ResponsiveTypographyTests
{
    [Fact]
    public void Clamp_returns_trimmed_css_clamp_expression()
    {
        Assert.Equal("clamp(1.75rem, 3vw, 2.5rem)",
            ResponsiveFontSize.Clamp(" 1.75rem ", "3vw", "2.5rem"));
    }

    [Theory]
    [InlineData(null, "3vw", "2.5rem")]
    [InlineData("1rem", null, "2.5rem")]
    [InlineData("1rem", "3vw", null)]
    [InlineData(" ", "3vw", "2.5rem")]
    public void Clamp_rejects_missing_arguments(string? minimum, string? preferred, string? maximum)
    {
        Assert.ThrowsAny<ArgumentException>(() => ResponsiveFontSize.Clamp(minimum!, preferred!, maximum!));
    }

    [Fact]
    public void Clamp_rejects_comma_in_argument()
    {
        Assert.Throws<ArgumentException>(() => ResponsiveFontSize.Clamp("1rem", "min(3vw, 2rem)", "4rem"));
    }

    [Fact]
    public void UseResponsiveTypography_applies_all_configured_roles()
    {
        var theme = new MudTheme();

        theme.UseResponsiveTypography(options =>
        {
            options.Default = "1rem";
            options.H1 = "2rem";
            options.H2 = "2.1rem";
            options.H3 = "2.2rem";
            options.H4 = "2.3rem";
            options.H5 = "2.4rem";
            options.H6 = "2.5rem";
            options.Subtitle1 = "1.1rem";
            options.Subtitle2 = "1.2rem";
            options.Body1 = "1.3rem";
            options.Body2 = "1.4rem";
            options.Button = "1.5rem";
            options.Caption = "1.6rem";
            options.Overline = "1.7rem";
        });

        Assert.Equal("1rem", theme.Typography.Default.FontSize);
        Assert.Equal("2rem", theme.Typography.H1.FontSize);
        Assert.Equal("2.1rem", theme.Typography.H2.FontSize);
        Assert.Equal("2.2rem", theme.Typography.H3.FontSize);
        Assert.Equal("2.3rem", theme.Typography.H4.FontSize);
        Assert.Equal("2.4rem", theme.Typography.H5.FontSize);
        Assert.Equal("2.5rem", theme.Typography.H6.FontSize);
        Assert.Equal("1.1rem", theme.Typography.Subtitle1.FontSize);
        Assert.Equal("1.2rem", theme.Typography.Subtitle2.FontSize);
        Assert.Equal("1.3rem", theme.Typography.Body1.FontSize);
        Assert.Equal("1.4rem", theme.Typography.Body2.FontSize);
        Assert.Equal("1.5rem", theme.Typography.Button.FontSize);
        Assert.Equal("1.6rem", theme.Typography.Caption.FontSize);
        Assert.Equal("1.7rem", theme.Typography.Overline.FontSize);
    }

    [Fact]
    public void UseResponsiveTypography_leaves_omitted_roles_unchanged_when_default_is_not_set()
    {
        var theme = new MudTheme();
        theme.Typography.H2.FontSize = "original";
        var originalH2 = theme.Typography.H2.FontSize;

        var result = theme.UseResponsiveTypography(options => options.H3 = "responsive");

        Assert.Same(theme, result);
        Assert.Equal(originalH2, theme.Typography.H2.FontSize);
        Assert.Equal("responsive", theme.Typography.H3.FontSize);
    }

    [Fact]
    public void Typography_accessibility_passes_for_relative_clamp_values()
    {
        var options = new NerdResponsiveTypographyOptions();
        options.Typography.H3 = ResponsiveFontSize.Clamp("1.75rem", "3vw", "2.5rem");

        var result = NerdTypographyAccessibilityTools.CheckAccessibility(options)
            .Single(item => item.Role == "H3");

        Assert.True(result.MeetsResizeGuidance);
        Assert.True(result.MeetsMinimumSize);
        Assert.Equal(28, result.MinimumPixels);
    }

    [Fact]
    public void Typography_accessibility_warns_for_small_fixed_px_values()
    {
        var options = new NerdResponsiveTypographyOptions();
        options.Typography.Body1 = "10px";

        var warnings = NerdTypographyAccessibilityTools.GetAccessibilityWarnings(options);

        Assert.Contains(warnings, warning => warning.Role == "Body1");
    }

    [Theory]
    [InlineData("clamp(1rem, 4vw, 2rem)", 320, 16)]
    [InlineData("clamp(1rem, 4vw, 2rem)", 1920, 32)]
    [InlineData("2rem", 1280, 32)]
    public void Clamp_evaluator_returns_expected_pixel_sizes(string fontSize, double viewport, double expected)
    {
        var actual = NerdClampEvaluator.EvaluateAtViewport(fontSize, viewport);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Marketing_preset_configures_roles_and_spacing()
    {
        var options = new ResponsiveTypographyOptions();
        NerdTypographyPresets.ApplyMarketing(options);

        Assert.Contains("H1", options.ConfiguredRoles);
        Assert.Equal("1.5", options.LineHeight);
        Assert.Equal("0.12em", options.LetterSpacing);
        Assert.Equal("0.875rem", options.Button);
        Assert.Equal("clamp(0.75rem, 0.7vw, 0.8125rem)", options.Caption);
    }

    [Fact]
    public void Per_role_line_height_overrides_global_default()
    {
        var theme = new MudTheme();
        theme.UseResponsiveTypography(options =>
        {
            options.H1 = "2rem";
            options.Body1 = "1rem";
            options.LineHeight = "1.4";
            options.Roles.H1.LineHeight = "1.1";
            options.Roles.Body1.LineHeight = "1.7";
        });

        Assert.Equal("1.1", theme.Typography.H1.LineHeight);
        Assert.Equal("1.7", theme.Typography.Body1.LineHeight);
    }

    [Theory]
    [InlineData("clamp(1.75rem, 3.5vw, 2.5rem)", 320, 28)]
    [InlineData("clamp(1.75rem, 3.5vw, 2.5rem)", 768, 28)]
    [InlineData("clamp(1.75rem, 3.5vw, 2.5rem)", 1280, 40)]
    [InlineData("clamp(1.75rem, 3.5vw, 2.5rem)", 1920, 40)]
    [InlineData("clamp(0.75rem, 0.7vw, 0.8125rem)", 320, 12)]
    [InlineData("clamp(0.75rem, 0.7vw, 0.8125rem)", 1280, 12)]
    [InlineData("clamp(0.75rem, 0.7vw, 0.8125rem)", 1920, 13)]
    public void Marketing_preset_has_stable_sizes_at_supported_viewports(
        string fontSize, double viewport, double expected)
    {
        var actual = NerdClampEvaluator.EvaluateAtViewport(fontSize, viewport);

        Assert.Equal(expected, actual ?? double.NaN, precision: 1);
    }

    [Fact]
    public void Typography_pack_round_trips_configured_roles()
    {
        var source = new NerdResponsiveTypographyOptions();
        source.Typography.H1 = "clamp(2rem, 4vw, 4rem)";
        source.Typography.Body1 = "1rem";

        var restored = NerdTypographyPack.FromJson(
            NerdTypographyPack.FromOptions(source, "acme").ToJson()).ToOptions();

        Assert.Equal("clamp(2rem, 4vw, 4rem)", restored.Typography.H1);
        Assert.Equal("1rem", restored.Typography.Body1);
    }

    [Fact]
    public async Task Typography_pack_store_saves_loads_and_lists_clients()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"nerd-typo-{Guid.NewGuid():N}");
        try
        {
            var store = new FileNerdTypographyPackStore(directory);
            var options = new NerdResponsiveTypographyOptions();
            options.Typography.H1 = "2rem";
            await store.SaveAsync(NerdTypographyPack.FromOptions(options, "acme"));

            var loaded = await store.LoadAsync("acme");

            Assert.Equal("2rem", loaded?.Roles["H1"]);
            Assert.Equal(["acme"], await store.ListAsync());
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }

    [Fact]
    public void Modular_scale_generator_returns_roles_and_clamp_values()
    {
        var scale = NerdModularScaleGenerator.Generate(1, 1.25);

        Assert.Equal(7, scale.Count);
        Assert.Contains("H1", scale.Keys);
        Assert.StartsWith("clamp(", scale["H1"]);
        Assert.StartsWith("clamp(", scale["Caption"]);
    }

    [Theory]
    [InlineData(0, 1.25)]
    [InlineData(1, 1)]
    public void Modular_scale_generator_rejects_invalid_inputs(double baseRem, double ratio)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => NerdModularScaleGenerator.Generate(baseRem, ratio));
    }

    [Fact]
    public void Editorial_and_dashboard_presets_configure_distinct_scales()
    {
        var editorial = new NerdResponsiveTypographyOptions();
        var dashboard = new NerdResponsiveTypographyOptions();

        NerdTypographyPresets.ApplyEditorial(editorial.Typography);
        NerdTypographyPresets.ApplyDashboard(dashboard.Typography);

        Assert.StartsWith("clamp(", editorial.Typography.H1);
        Assert.StartsWith("clamp(", dashboard.Typography.H1);
        Assert.NotEqual(editorial.Typography.H1, dashboard.Typography.H1);
        Assert.Equal("1rem", dashboard.Typography.Body1);
        Assert.Equal("0.875rem", dashboard.Typography.Button);
        Assert.Equal("1.7", editorial.Typography.Roles.Body1.LineHeight);
        Assert.Empty(NerdTypographyAccessibilityTools.GetAccessibilityWarnings(editorial));
        Assert.Empty(NerdTypographyAccessibilityTools.GetAccessibilityWarnings(dashboard));
    }

    [Fact]
    public void Accessibility_storyboard_reports_all_configured_roles()
    {
        var options = new NerdResponsiveTypographyOptions();
        NerdTypographyPresets.ApplyEditorial(options.Typography);

        var results = NerdTypographyAccessibilityTools.CheckAccessibility(options);

        Assert.Contains(results, result => result.Role == "H1");
        Assert.Contains(results, result => result.Role == "Body1");
        Assert.All(results, result => Assert.False(string.IsNullOrWhiteSpace(result.WcagVersion)));
    }

    [Fact]
    public void Tokens_studio_export_and_import_round_trip_font_sizes()
    {
        var source = new NerdResponsiveTypographyOptions();
        source.Typography.H1 = "clamp(2rem, 4vw, 4rem)";
        source.Typography.Body1 = "1rem";
        source.Typography.LineHeight = "1.5";

        var json = NerdTypographyTools.ExportTokensStudioJson(source);
        var restored = new NerdResponsiveTypographyOptions();
        NerdTypographyTools.ImportTokensStudioJson(restored, json);

        Assert.Equal("clamp(2rem, 4vw, 4rem)", restored.Typography.H1);
        Assert.Equal("1rem", restored.Typography.Body1);
        Assert.Equal("1.5", restored.Typography.LineHeight);
        Assert.Contains("\"fontSizes\"", json);
    }
}
