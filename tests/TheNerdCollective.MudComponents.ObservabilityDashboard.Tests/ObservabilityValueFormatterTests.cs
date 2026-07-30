using TheNerdCollective.Blazor.Observability;

namespace TheNerdCollective.MudComponents.ObservabilityDashboard.Tests;

public sealed class ObservabilityValueFormatterTests
{
    [Theory]
    [InlineData(3.5, "reqps", "3.50/s")]
    [InlineData(120, "ms", "120 ms")]
    [InlineData(0.05, "percentunit", "5.0 %")]
    public void FormatValue_formats_known_units(double value, string unit, string expected)
    {
        Assert.Equal(expected, ObservabilityValueFormatter.FormatValue(value, unit));
    }

    [Fact]
    public void FormatScalar_returns_dash_when_null()
    {
        Assert.Equal("—", ObservabilityValueFormatter.FormatScalar(null));
    }
}
