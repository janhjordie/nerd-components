using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TheNerdCollective.Blazor.Observability;

namespace TheNerdCollective.Blazor.Observability.Tests;

public sealed class ObservabilityDashboardServiceTests
{
    [Fact]
    public async Task GetOverviewAsync_aggregates_scalar_panels_and_health()
    {
        var backend = new FakeObservabilityBackend();
        var options = Options.Create(new ObservabilityDashboardOptions
        {
            DefaultServiceName = "nerd-consent-host",
            DefaultLookbackMinutes = 15,
            DegradedP95LatencyMs = 2000,
            UnhealthyErrorPercentage = 0.05
        });

        var service = new ObservabilityDashboardService(backend, options);
        var snapshot = await service.GetOverviewAsync();

        Assert.Equal("nerd-consent-host", snapshot.ServiceName);
        Assert.Equal(3.5, snapshot.RequestRate?.Value);
        Assert.Equal(2500, snapshot.P95LatencyMs?.Value);
        Assert.Equal(ObservabilityHealthStatus.Degraded, snapshot.Health);
    }

    [Fact]
    public void GetExternalDashboardUrl_appends_service_query_when_configured()
    {
        var service = new ObservabilityDashboardService(
            new FakeObservabilityBackend(),
            Options.Create(new ObservabilityDashboardOptions
            {
                DefaultServiceName = "my-app",
                ExternalDashboardBaseUrl = "https://devops.example.com"
            }));

        var url = service.GetExternalDashboardUrl("nerd-consent-host");

        Assert.NotNull(url);
        Assert.Equal("https://devops.example.com/services?service=nerd-consent-host", url.ToString());
    }

    [Fact]
    public void AddObservabilityDashboard_registers_dashboard_service()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IObservabilityBackend, FakeObservabilityBackend>();
        services.AddObservabilityDashboard(o =>
        {
            o.DefaultServiceName = "test-app";
        });

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<IObservabilityDashboardService>());
        Assert.IsType<FakeObservabilityBackend>(provider.GetRequiredService<IObservabilityBackend>());
    }

    private sealed class FakeObservabilityBackend : IObservabilityBackend
    {
        public string BackendId => "fake";

        public Task<IReadOnlyList<ObservabilityServiceInfo>> ListServicesAsync(
            ObservabilityQueryContext context,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ObservabilityServiceInfo>>([]);

        public Task<ObservabilityTimeSeriesResult> QueryTimeSeriesAsync(
            ObservabilityPanelQuery query,
            CancellationToken cancellationToken = default)
        {
            var points = new List<ObservabilityTimeSeriesPoint>
            {
                new(DateTimeOffset.UtcNow.AddMinutes(-1), ResolveValue(query.PanelId)),
                new(DateTimeOffset.UtcNow, ResolveValue(query.PanelId))
            };

            var definition = ObservabilityPanelCatalog.GetDefinition(query.PanelId);
            return Task.FromResult(new ObservabilityTimeSeriesResult(definition.Legend, definition.Unit, points));
        }

        public async Task<ObservabilityScalarResult> QueryScalarAsync(
            ObservabilityPanelQuery query,
            CancellationToken cancellationToken = default)
        {
            var series = await QueryTimeSeriesAsync(query, cancellationToken).ConfigureAwait(false);
            var definition = ObservabilityPanelCatalog.GetDefinition(query.PanelId);
            return new ObservabilityScalarResult(series.Points[^1].Value, definition.Unit, definition.Title);
        }

        public Task<ObservabilityHealthSummary> GetHealthSummaryAsync(
            string serviceName,
            ObservabilityQueryContext context,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new ObservabilityHealthSummary(
                serviceName,
                ObservabilityHealthStatus.Healthy,
                null,
                DateTimeOffset.UtcNow));

        private static double ResolveValue(ObservabilityPanelId panelId) =>
            panelId switch
            {
                ObservabilityPanelId.RequestRate => 3.5,
                ObservabilityPanelId.P95Latency => 2500,
                ObservabilityPanelId.ErrorRate5xx => 0,
                ObservabilityPanelId.ErrorPercentage => 0.01,
                _ => 0
            };
    }
}
