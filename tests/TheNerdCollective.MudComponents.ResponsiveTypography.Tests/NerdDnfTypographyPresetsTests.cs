using TheNerdCollective.Brand.Dnf;
using TheNerdCollective.MudComponents.ResponsiveTypography;

namespace TheNerdCollective.MudComponents.ResponsiveTypography.Tests;

public class NerdDnfTypographyPresetsTests
{
    [Fact]
    public void Apply_configures_editorial_dnf_scale()
    {
        var options = new ResponsiveTypographyOptions();
        NerdDnfTypographyPresets.Apply(options);

        Assert.Contains("clamp(", options.H1, StringComparison.Ordinal);
        Assert.Contains("clamp(", options.H3, StringComparison.Ordinal);
        Assert.Equal("1.5", options.LineHeight);
        Assert.Equal("0.12em", options.LetterSpacing);
    }
}
