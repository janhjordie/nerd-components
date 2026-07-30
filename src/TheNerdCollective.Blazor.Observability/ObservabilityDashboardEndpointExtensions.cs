using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TheNerdCollective.Blazor.Observability;

/// <summary>Minimal API endpoints for observability dashboard data.</summary>
public static class ObservabilityDashboardEndpointExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    /// <summary>
    /// Maps observability dashboard API endpoints.
    /// Host applications should call <c>.RequireAuthorization()</c> on the returned group.
    /// </summary>
    public static IEndpointRouteBuilder MapObservabilityDashboardEndpoints(
        this IEndpointRouteBuilder endpoints,
        string pattern = "/api/observability")
    {
        var options = endpoints.ServiceProvider.GetRequiredService<IOptions<ObservabilityDashboardOptions>>().Value;
        if (!options.EnableMinimalApi)
        {
            return endpoints;
        }

        var group = endpoints.MapGroup(pattern);

        group.MapGet("/overview", async (
            IObservabilityDashboardService dashboard,
            string? service,
            CancellationToken cancellationToken) =>
        {
            var snapshot = await dashboard.GetOverviewAsync(service, cancellationToken).ConfigureAwait(false);
            return Results.Json(snapshot, JsonOptions);
        })
        .WithName("GetObservabilityOverview")
        .WithDescription("Get scalar overview metrics for a service");

        group.MapGet("/panel/{panelId}", async (
            IObservabilityDashboardService dashboard,
            ObservabilityPanelId panelId,
            string service,
            int? minutes,
            CancellationToken cancellationToken) =>
        {
            var lookback = minutes ?? options.DefaultLookbackMinutes;
            var timeRange = ObservabilityTimeRange.LastMinutes(lookback);
            var series = await dashboard.GetPanelAsync(panelId, service, timeRange, cancellationToken)
                .ConfigureAwait(false);
            return Results.Json(series, JsonOptions);
        })
        .WithName("GetObservabilityPanel")
        .WithDescription("Get a preset panel time series");

        group.MapGet("/services", async (
            IObservabilityBackend backend,
            int? minutes,
            CancellationToken cancellationToken) =>
        {
            var lookback = minutes ?? options.DefaultLookbackMinutes;
            var timeRange = ObservabilityTimeRange.LastMinutes(lookback);
            var context = new ObservabilityQueryContext(timeRange.Start, timeRange.End);
            var services = await backend.ListServicesAsync(context, cancellationToken).ConfigureAwait(false);
            return Results.Json(services, JsonOptions);
        })
        .WithName("ListObservabilityServices")
        .WithDescription("List services visible to the observability backend");

        group.MapGet("/health", async (
            IObservabilityBackend backend,
            string service,
            int? minutes,
            CancellationToken cancellationToken) =>
        {
            var lookback = minutes ?? options.DefaultLookbackMinutes;
            var timeRange = ObservabilityTimeRange.LastMinutes(lookback);
            var context = new ObservabilityQueryContext(timeRange.Start, timeRange.End, service);
            var health = await backend.GetHealthSummaryAsync(service, context, cancellationToken)
                .ConfigureAwait(false);
            return Results.Json(health, JsonOptions);
        })
        .WithName("GetObservabilityHealth")
        .WithDescription("Get coarse health summary for a service");

        return endpoints;
    }
}
