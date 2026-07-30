using TheNerdCollective.Blazor.Observability;
using TheNerdCollective.Blazor.Observability.SigNoz;

namespace TheNerdCollective.Blazor.Observability.SigNoz.Tests;

public sealed class SigNozResponseParserTests
{
    [Fact]
    public void ParseTimeSeries_reads_v5_aggregation_series()
    {
        var json = ReadFixture("query_range_v5_series.json");
        var result = SigNozResponseParser.ParseTimeSeries(json, ObservabilityPanelId.RequestRate);

        Assert.Equal("rps", result.Legend);
        Assert.Equal("reqps", result.Unit);
        Assert.Equal(2, result.Points.Count);
        Assert.Equal(12.5, result.Points[0].Value, precision: 3);
        Assert.Equal(14.2, result.Points[1].Value, precision: 3);
    }

    [Fact]
    public void ParseTimeSeries_reads_series_tuple_values()
    {
        var json = ReadFixture("query_range_series.json");
        var result = SigNozResponseParser.ParseTimeSeries(json, ObservabilityPanelId.RequestRate);

        Assert.Equal("rps", result.Legend);
        Assert.Equal("reqps", result.Unit);
        Assert.Equal(2, result.Points.Count);
        Assert.Equal(12.5, result.Points[0].Value, precision: 3);
        Assert.Equal(14.2, result.Points[1].Value, precision: 3);
    }

    [Fact]
    public void ParseServices_reads_service_metadata()
    {
        var json = ReadFixture("services_list.json");
        var services = SigNozResponseParser.ParseServices(json);

        Assert.Equal(2, services.Count);
        Assert.Equal("nerd-consent-host", services[0].Name);
        Assert.Equal("production", services[0].Environment);
        Assert.Equal(3.2, services[0].RequestRate);
        Assert.Equal(120, services[0].P95LatencyMs);
        Assert.Equal(0.01, services[0].ErrorRate);
    }

    [Fact]
    public void ParseTimeSeries_returns_empty_points_for_unknown_shape()
    {
        var result = SigNozResponseParser.ParseTimeSeries("{}", ObservabilityPanelId.P95Latency);

        Assert.Empty(result.Points);
        Assert.Equal("p95", result.Legend);
    }

    private static string ReadFixture(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName);
        return File.ReadAllText(path);
    }
}
