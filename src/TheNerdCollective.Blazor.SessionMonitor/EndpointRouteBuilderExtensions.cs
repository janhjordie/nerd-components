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
            var metrics = monitor.GetCurrentMetrics();
            var clients = monitor.GetActiveSessionsByClient().ToList();
            var sessions = clients.SelectMany(c => c.Circuits).ToList();
            return Results.Json(new
            {
                trackingMode = metrics.TrackingMode,
                isDegradedMode = metrics.IsDegradedMode,
                degradedModeThreshold = metrics.DegradedModeThreshold,
                activeCircuits = sessions.Select(s => s.CircuitId),
                clients,
                sessions,
                count = metrics.ActiveSessions,
                uniqueBrowsers = clients.Count,
                detailAvailable = !metrics.IsDegradedMode
            }, jsonOptions);
        })
        .WithName("GetActiveCircuits")
        .WithDescription("Get list of active circuit IDs");

        // GET /api/session-monitor/active-by-path
        endpoints.MapGet($"{pattern}/active-by-path", async (ISessionMonitorService monitor) =>
        {
            var metrics = monitor.GetCurrentMetrics();
            var summaries = monitor.GetActiveSessionsByPath().ToList();
            return Results.Json(new
            {
                trackingMode = metrics.TrackingMode,
                isDegradedMode = metrics.IsDegradedMode,
                paths = summaries,
                uniquePaths = summaries.Count,
                totalActiveSessions = metrics.ActiveSessions
            }, jsonOptions);
        })
        .WithName("GetActiveSessionsByPath")
        .WithDescription("Get active session counts grouped by URL path");

        // GET /api/session-monitor/active-by-client
        endpoints.MapGet($"{pattern}/active-by-client", async (ISessionMonitorService monitor) =>
        {
            var metrics = monitor.GetCurrentMetrics();
            var summaries = monitor.GetActiveSessionsByClient().ToList();
            return Results.Json(new
            {
                trackingMode = metrics.TrackingMode,
                isDegradedMode = metrics.IsDegradedMode,
                clients = summaries,
                uniqueBrowsers = summaries.Count,
                totalActiveSessions = metrics.ActiveSessions
            }, jsonOptions);
        })
        .WithName("GetActiveSessionsByClient")
        .WithDescription("Get active session counts grouped by browser client (shared cookie across tabs)");

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
            var assessment = monitor.GetDeploymentSafety(maxActiveSessions);
            var metrics = monitor.GetCurrentMetrics();

            return Results.Json(new
            {
                canDeploy = assessment.CanDeploy,
                currentActiveSessions = assessment.ActiveSessions,
                nonAdminActiveSessions = assessment.NonAdminActiveSessions,
                adminMonitorSessions = assessment.AdminMonitorSessions,
                hasAdminMonitorSession = assessment.HasAdminMonitorSession,
                disconnectedSessions = metrics.DisconnectedSessions,
                threshold = maxActiveSessions,
                timestamp = DateTime.UtcNow,
                instanceId = metrics.InstanceId,
                machineName = metrics.MachineName
            }, jsonOptions);
        })
        .WithName("CanDeploy")
        .WithDescription("Check if deployment is safe based on active session count");

        return endpoints;
    }
}
