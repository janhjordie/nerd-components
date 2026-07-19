using TheNerdCollective.Brand.Tnc;
using TheNerdCollective.MudComponents.ResponsiveTypography;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.ResponsiveTypography.Tests;

public class NerdTncTypographyPresetsTests
{
    [Fact]
    public void Apply_loads_embedded_tnc_typography_json()
    {
        var options = new NerdResponsiveTypographyOptions();
        NerdTncTypographyPresets.Apply(options.Typography);

        Assert.Equal("1rem", options.Typography.Default);
        Assert.Equal("clamp(1.5rem, 2.5vw, 2rem)", options.Typography.H1);
        Assert.Equal("0.875rem", options.Typography.Button);
        Assert.Equal("0.75rem", options.Typography.Caption);
        Assert.Equal("1.5", options.Typography.LineHeight);
        Assert.Equal("0.12em", options.Typography.LetterSpacing);
    }

    [Fact]
    public void Apply_produces_no_accessibility_warnings()
    {
        var options = new NerdResponsiveTypographyOptions();
        NerdTncTypographyPresets.Apply(options.Typography);

        Assert.Empty(NerdTypographyAccessibilityTools.GetAccessibilityWarnings(options));
    }

    [Fact]
    public void Roles_use_tiered_wcag_minimums()
    {
        var options = new NerdResponsiveTypographyOptions();
        NerdTncTypographyPresets.Apply(options.Typography);
        var roles = NerdTypographyAccessibilityTools.CheckAccessibility(options).ToDictionary(r => r.Role);

        Assert.Equal(12, roles["Caption"].RequiredMinimumPixels);
        Assert.Equal(14, roles["Button"].RequiredMinimumPixels);
        Assert.Equal(16, roles["Body1"].RequiredMinimumPixels);
    }
}
