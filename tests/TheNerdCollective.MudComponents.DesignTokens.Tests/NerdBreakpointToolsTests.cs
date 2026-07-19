using MudBlazor;
using TheNerdCollective.MudComponents.DesignTokens;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdBreakpointToolsTests
{
    [Fact]
    public void ToMudBreakpoint_maps_standard_names()
    {
        Assert.Equal(Breakpoint.Sm, NerdBreakpointTools.ToMudBreakpoint("sm"));
        Assert.Equal(Breakpoint.Xl, NerdBreakpointTools.ToMudBreakpoint("xl"));
    }

    [Fact]
    public void GetComparisonColumns_uses_token_breakpoints_when_present()
    {
        var options = new NerdDesignTokenOptions();
        NerdFoundationTaxonomyTools.ApplyDefaults(options);

        var columns = NerdBreakpointTools.GetComparisonColumns(options);

        Assert.Contains(600, columns);
        Assert.Contains(1280, columns);
    }
}
