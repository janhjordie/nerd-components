using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using TheNerdCollective.Blazor.Observability;

namespace TheNerdCollective.Blazor.Observability.Tests;

public sealed class ObservabilityDashboardEndpointTests
{
    [Fact]
    public async Task Overview_endpoint_returns_snapshot_json()
    {
        await using var app = await CreateAppAsync();
        var client = app.GetTestClient();

        var response = await client.GetAsync("/api/observability/overview?service=nerd-consent-host");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        Assert.Equal("nerd-consent-host", document.RootElement.GetProperty("serviceName").GetString());
        Assert.Equal((int)ObservabilityHealthStatus.Healthy, document.RootElement.GetProperty("health").GetInt32());
    }

    [Fact]
    public async Task Panel_endpoint_returns_time_series_json()
    {
        await using var app = await CreateAppAsync();
        var client = app.GetTestClient();

        var response = await client.GetAsync(
            "/api/observability/panel/RequestRate?service=nerd-consent-host&minutes=15");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        Assert.Equal("rps", document.RootElement.GetProperty("legend").GetString());
        Assert.True(document.RootElement.GetProperty("points").GetArrayLength() > 0);
    }

    [Fact]
    public async Task Services_endpoint_returns_service_list()
    {
        await using var app = await CreateAppAsync();
        var client = app.GetTestClient();

        var response = await client.GetAsync("/api/observability/services?minutes=15");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Array, document.RootElement.ValueKind);
        Assert.Equal("nerd-consent-host", document.RootElement[0].GetProperty("name").GetString());
    }

    private static async Task<WebApplication> CreateAppAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<IObservabilityBackend, EndpointFakeBackend>();
        builder.Services.AddSingleton<IObservabilityDashboardService, ObservabilityDashboardService>();
        builder.Services.Configure<ObservabilityDashboardOptions>(o =>
        {
            o.DefaultServiceName = "nerd-consent-host";
            o.EnableMinimalApi = true;
        });

        var app = builder.Build();
        app.MapObservabilityDashboardEndpoints();
        await app.StartAsync();
        return app;
    }

    private sealed class EndpointFakeBackend : IObservabilityBackend
    {
        public string BackendId => "fake";

        public Task<IReadOnlyList<ObservabilityServiceInfo>> ListServicesAsync(
            ObservabilityQueryContext context,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ObservabilityServiceInfo>>(
            [
                new("nerd-consent-host", "production", 3.2, 120, 0.01)
            ]);

        public Task<ObservabilityTimeSeriesResult> QueryTimeSeriesAsync(
            ObservabilityPanelQuery query,
            CancellationToken cancellationToken = default)
        {
            var definition = ObservabilityPanelCatalog.GetDefinition(query.PanelId);
            return Task.FromResult(new ObservabilityTimeSeriesResult(
                definition.Legend,
                definition.Unit,
                [new ObservabilityTimeSeriesPoint(DateTimeOffset.UtcNow, 3.5)]));
        }

        public async Task<ObservabilityScalarResult> QueryScalarAsync(
            ObservabilityPanelQuery query,
            CancellationToken cancellationToken = default)
        {
            var value = query.PanelId switch
            {
                ObservabilityPanelId.RequestRate => 3.5,
                ObservabilityPanelId.P95Latency => 120,
                ObservabilityPanelId.ErrorRate5xx => 0,
                ObservabilityPanelId.ErrorPercentage => 0.01,
                _ => 0
            };
            var definition = ObservabilityPanelCatalog.GetDefinition(query.PanelId);
            return new ObservabilityScalarResult(value, definition.Unit, definition.Title);
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
    }
}
