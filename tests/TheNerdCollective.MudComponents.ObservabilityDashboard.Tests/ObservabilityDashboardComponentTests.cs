using Bunit;
using Microsoft.Extensions.DependencyInjection;
using TheNerdCollective.Blazor.Observability;
using TheNerdCollective.MudComponents.ObservabilityDashboard;

namespace TheNerdCollective.MudComponents.ObservabilityDashboard.Tests;

public sealed class ObservabilityHealthBadgeTests : ObservabilityComponentTestContext
{
    [Theory]
    [InlineData(ObservabilityHealthStatus.Healthy, "Healthy")]
    [InlineData(ObservabilityHealthStatus.Degraded, "Degraded")]
    [InlineData(ObservabilityHealthStatus.Unhealthy, "Unhealthy")]
    public void Health_badge_renders_status_label(ObservabilityHealthStatus status, string label)
    {
        var cut = Render<ObservabilityHealthBadge>(parameters => parameters
            .Add(p => p.Status, status));

        Assert.Contains(label, cut.Markup);
    }
}

public sealed class ObservabilityMetricCardTests : ObservabilityComponentTestContext
{
    [Fact]
    public void Metric_card_renders_title_value_and_test_id()
    {
        var cut = Render<ObservabilityMetricCard>(parameters => parameters
            .Add(p => p.Title, "Request rate")
            .Add(p => p.Value, "3.50/s")
            .Add(p => p.DataTestId, "observability-metric-request-rate"));

        Assert.Contains("Request rate", cut.Markup);
        Assert.Contains("3.50/s", cut.Markup);
        Assert.Contains("observability-metric-request-rate", cut.Markup);
    }
}

public sealed class ObservabilityTimeSeriesChartTests : ObservabilityComponentTestContext
{
    [Fact]
    public void Time_series_chart_renders_when_points_exist()
    {
        var series = new ObservabilityTimeSeriesResult(
            "rps",
            "reqps",
            [
                new(DateTimeOffset.UtcNow.AddMinutes(-5), 2.5),
                new(DateTimeOffset.UtcNow, 3.1)
            ]);

        var cut = Render<ObservabilityTimeSeriesChart>(parameters => parameters
            .Add(p => p.Title, "Request rate")
            .Add(p => p.Series, series)
            .Add(p => p.DataTestId, "observability-chart-request-rate"));

        Assert.Contains("Request rate", cut.Markup);
        Assert.Contains("observability-chart-request-rate", cut.Markup);
    }

    [Fact]
    public void Time_series_chart_shows_empty_state_without_points()
    {
        var series = new ObservabilityTimeSeriesResult("rps", "reqps", []);

        var cut = Render<ObservabilityTimeSeriesChart>(parameters => parameters
            .Add(p => p.Title, "Request rate")
            .Add(p => p.Series, series));

        Assert.Contains("No time series data", cut.Markup);
    }
}

public sealed class ObservabilityDashboardTests : ObservabilityComponentTestContext
{
    [Fact]
    public void Dashboard_renders_overview_cards_and_charts()
    {
        Services.AddSingleton<IObservabilityDashboardService>(new FakeDashboardService());

        var cut = Render<ObservabilityDashboard>(parameters => parameters
            .Add(p => p.ServiceName, "nerd-consent-host")
            .Add(p => p.ShowExternalLink, false)
            .Add(p => p.AutoRefreshSeconds, 0));

        cut.WaitForState(() => cut.Markup.Contains("observability-metric-request-rate", StringComparison.Ordinal));

        Assert.Contains("Observability", cut.Markup);
        Assert.Contains("nerd-consent-host", cut.Markup);
        Assert.Contains("observability-chart-request-rate", cut.Markup);
    }

    private sealed class FakeDashboardService : IObservabilityDashboardService
    {
        public ObservabilityDashboardOptions Options { get; } = new()
        {
            DefaultServiceName = "nerd-consent-host"
        };

        public Task RefreshAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<ObservabilityOverviewSnapshot> GetOverviewAsync(
            string? serviceName = null,
            CancellationToken cancellationToken = default)
        {
            var service = serviceName ?? Options.DefaultServiceName;
            return Task.FromResult(new ObservabilityOverviewSnapshot(
                service,
                new ObservabilityScalarResult(3.5, "reqps", "Request rate"),
                new ObservabilityScalarResult(120, "ms", "P95 latency"),
                new ObservabilityScalarResult(0, "reqps", "5xx rate"),
                new ObservabilityScalarResult(0.01, "percentunit", "Error percentage"),
                ObservabilityHealthStatus.Healthy,
                DateTimeOffset.UtcNow));
        }

        public Task<ObservabilityTimeSeriesResult> GetPanelAsync(
            ObservabilityPanelId panelId,
            string serviceName,
            ObservabilityTimeRange timeRange,
            CancellationToken cancellationToken = default)
        {
            var definition = ObservabilityPanelCatalog.GetDefinition(panelId);
            return Task.FromResult(new ObservabilityTimeSeriesResult(
                definition.Legend,
                definition.Unit,
                [new ObservabilityTimeSeriesPoint(DateTimeOffset.UtcNow.AddMinutes(-1), 2.5),
                 new ObservabilityTimeSeriesPoint(DateTimeOffset.UtcNow, 3.5)]));
        }

        public Task<ObservabilityScalarResult?> GetScalarAsync(
            ObservabilityPanelId panelId,
            string serviceName,
            ObservabilityTimeRange? timeRange = null,
            CancellationToken cancellationToken = default)
        {
            var definition = ObservabilityPanelCatalog.GetDefinition(panelId);
            var value = panelId switch
            {
                ObservabilityPanelId.HostCpuUtilization => 0.42,
                ObservabilityPanelId.HostMemoryUtilization => 0.61,
                ObservabilityPanelId.HostDiskUtilization => 0.35,
                ObservabilityPanelId.RuntimeGcHeap => 52_428_800,
                ObservabilityPanelId.RuntimeProcessMemory => 104_857_600,
                ObservabilityPanelId.DbQueryRate => 12.5,
                ObservabilityPanelId.DbQueryP95 => 45,
                ObservabilityPanelId.HttpClientRate => 2.1,
                ObservabilityPanelId.HttpClientP95 => 180,
                _ => 1.0
            };
            return Task.FromResult<ObservabilityScalarResult?>(
                new ObservabilityScalarResult(value, definition.Unit, definition.Title));
        }

        public Uri? GetExternalDashboardUrl(string? serviceName = null) => null;
    }
}
