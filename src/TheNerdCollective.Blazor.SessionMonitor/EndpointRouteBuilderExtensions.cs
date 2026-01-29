using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace TheNerdCollective.Blazor.SessionMonitor;

/// <summary>
/// Extension methods for mapping session monitor API endpoints.
/// </summary>
public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps session monitoring API endpoints.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="pattern">Base URL pattern (default: "/api/session-monitor").</param>
    /// <returns>The endpoint route builder for chaining.</returns>
    public static IEndpointRouteBuilder MapSessionMonitoringEndpoints(
        this IEndpointRouteBuilder endpoints, 
        string pattern = "/api/session-monitor")
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        // GET /api/session-monitor/current
        endpoints.MapGet($"{pattern}/current", async (ISessionMonitorService monitor) =>
        {
            var metrics = monitor.GetCurrentMetrics();
            return Results.Json(metrics, jsonOptions);
        })
        .WithName("GetCurrentSessionMetrics")
        .WithDescription("Get current session metrics");

        // GET /api/session-monitor/history?since=<timestamp>&maxCount=100
        endpoints.MapGet($"{pattern}/history", async (
            ISessionMonitorService monitor,
            DateTime? since,
            int maxCount = 100) =>
        {
            var history = monitor.GetHistory(since, maxCount);
            return Results.Json(history, jsonOptions);
        })
        .WithName("GetSessionHistory")
        .WithDescription("Get historical session snapshots");

        // GET /api/session-monitor/active-circuits
        endpoints.MapGet($"{pattern}/active-circuits", async (ISessionMonitorService monitor) =>
        {
            var circuits = monitor.GetActiveCircuitIds();
            return Results.Json(new { activeCircuits = circuits, count = circuits.Count() }, jsonOptions);
        })
        .WithName("GetActiveCircuits")
        .WithDescription("Get list of active circuit IDs");

        // GET /api/session-monitor/deployment-windows?windowMinutes=5&lookbackHours=24
        endpoints.MapGet($"{pattern}/deployment-windows", async (
            ISessionMonitorService monitor,
            int windowMinutes = 5,
            int lookbackHours = 24) =>
        {
            var windows = monitor.FindOptimalDeploymentWindows(windowMinutes, lookbackHours);
            return Results.Json(windows, jsonOptions);
        })
        .WithName("GetOptimalDeploymentWindows")
        .WithDescription("Find optimal deployment windows with minimal active sessions");

        // GET /api/session-monitor/can-deploy?maxActiveSessions=0
        endpoints.MapGet($"{pattern}/can-deploy", async (
            ISessionMonitorService monitor,
            int maxActiveSessions = 0) =>
        {
            var metrics = monitor.GetCurrentMetrics();
            var canDeploy = metrics.ActiveSessions <= maxActiveSessions;
            
            return Results.Json(new 
            { 
                canDeploy,
                currentActiveSessions = metrics.ActiveSessions,
                threshold = maxActiveSessions,
                timestamp = DateTime.UtcNow
            }, jsonOptions);
        })
        .WithName("CanDeploy")
        .WithDescription("Check if deployment is safe based on active session count");

        return endpoints;
    }
}
