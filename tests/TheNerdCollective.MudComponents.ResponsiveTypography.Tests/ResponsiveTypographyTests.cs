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
    public void UseResponsiveTypography_leaves_omitted_roles_unchanged_and_returns_theme()
    {
        var theme = new MudTheme();
        theme.Typography.H2.FontSize = "original";
        var originalH2 = theme.Typography.H2.FontSize;

        var result = theme.UseResponsiveTypography(options => options.H3 = "responsive");

        Assert.Same(theme, result);
        Assert.Equal(originalH2, theme.Typography.H2.FontSize);
        Assert.Equal("responsive", theme.Typography.H3.FontSize);
    }
}
