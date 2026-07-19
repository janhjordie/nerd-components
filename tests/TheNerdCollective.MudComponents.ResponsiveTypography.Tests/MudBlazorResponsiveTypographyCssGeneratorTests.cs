using MudBlazor;
using TheNerdCollective.Brand.Tnc;
using TheNerdCollective.MudComponents.ResponsiveTypography;

namespace TheNerdCollective.MudComponents.ResponsiveTypography.Tests;

public sealed class MudBlazorResponsiveTypographyCssGeneratorTests
{
    [Fact]
    public void Generate_emits_mud_typography_css_variables_for_all_configured_roles()
    {
        var options = new ResponsiveTypographyOptions();
        NerdTncTypographyPresets.Apply(options);

        var css = MudBlazorResponsiveTypographyCssGenerator.Generate(options);

        Assert.Contains("--mud-typography-caption-size: 0.75rem;", css, StringComparison.Ordinal);
        Assert.Contains("--mud-typography-h1-size: clamp(1.5rem, 2.5vw, 2rem);", css, StringComparison.Ordinal);
        Assert.Contains(".mud-typography-caption {", css, StringComparison.Ordinal);
        Assert.Contains("font-size: 0.75rem !important;", css, StringComparison.Ordinal);
    }

    [Fact]
    public void UseResponsiveTypography_applies_default_fallback_to_unconfigured_roles()
    {
        var theme = new MudTheme();
        theme.Typography.Caption.FontSize = "0.75rem";

        theme.UseResponsiveTypography(options =>
        {
            options.Default = "1rem";
            options.H3 = "1.5rem";
            options.LineHeight = "1.5";
            options.LetterSpacing = "0.12em";
        });

        Assert.Equal("1rem", theme.Typography.Caption.FontSize);
        Assert.Equal("1rem", theme.Typography.Body1.FontSize);
        Assert.Equal("1.5rem", theme.Typography.H3.FontSize);
        Assert.Equal("1.5", theme.Typography.Caption.LineHeight);
        Assert.Equal("0.12em", theme.Typography.Caption.LetterSpacing);
    }
}
