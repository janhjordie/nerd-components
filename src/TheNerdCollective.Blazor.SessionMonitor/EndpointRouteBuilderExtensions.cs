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
    /// <param name="configureGroup">Optional group convention, for example authorization. Existing routes stay unchanged.</param>
    /// <returns>The endpoint route builder for chaining.</returns>
    public static IEndpointRouteBuilder MapSessionMonitoringEndpoints(
        this IEndpointRouteBuilder endpoints,
        string pattern = "/api/session-monitor",
        Action<RouteGroupBuilder>? configureGroup = null)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var group = endpoints.MapGroup(pattern);
        configureGroup?.Invoke(group);

        group.MapGet("/current", (ISessionMonitorService monitor) =>
        {
            var metrics = monitor.GetCurrentMetrics();
            return Results.Json(metrics, jsonOptions);
        })
        .WithName("GetCurrentSessionMetrics")
        .WithDescription("Get current session metrics");

        // GET /api/session-monitor/history?since=<timestamp>&maxCount=100
        group.MapGet("/history", (
            ISessionMonitorService monitor,
            DateTime? since,
            int maxCount = 100) =>
        {
            var error = SessionMonitorQueryLimits.ValidateHistoryCount(maxCount);
            if (error is not null)
            {
                return Results.BadRequest(new { message = error });
            }

            var history = monitor.GetHistory(since, maxCount);
            return Results.Json(history, jsonOptions);
        })
        .WithName("GetSessionHistory")
        .WithDescription("Get historical session snapshots");

        // GET /api/session-monitor/active-circuits
        group.MapGet("/active-circuits", (ISessionMonitorService monitor) =>
        {
            var metrics = monitor.GetCurrentMetrics();
            var clients = monitor.GetActiveSessionsByClient().ToList();
            var sessions = clients.SelectMany(c => c.Circuits).ToList();
            return Results.Json(new
            {
                trackingMode = metrics.TrackingMode,
                isDegradedMode = metrics.IsDegradedMode,
                degradedModeThreshold = metrics.DegradedModeThreshold,
                activeCircuits = sessions.Where(s => !s.IsDisconnected).Select(s => s.CircuitId),
                retainedCircuitIds = sessions.Where(s => s.IsDisconnected).Select(s => s.CircuitId),
                clients,
                sessions,
                count = metrics.ActiveSessions,
                connectedCount = metrics.ActiveSessions,
                retainedSessions = metrics.DisconnectedSessions,
                instanceId = metrics.InstanceId,
                trackingSince = metrics.TrackingSince,
                uniqueBrowsers = clients.Count,
                detailAvailable = !metrics.IsDegradedMode
            }, jsonOptions);
        })
        .WithName("GetActiveCircuits")
        .WithDescription("Get list of active circuit IDs");

        // GET /api/session-monitor/active-by-path
        group.MapGet("/active-by-path", (ISessionMonitorService monitor) =>
        {
            var metrics = monitor.GetCurrentMetrics();
            var summaries = monitor.GetActiveSessionsByPath().ToList();
            return Results.Json(new
            {
                trackingMode = metrics.TrackingMode,
                isDegradedMode = metrics.IsDegradedMode,
                paths = summaries,
                uniquePaths = summaries.Count,
                totalActiveSessions = metrics.ActiveSessions,
                retainedSessions = metrics.DisconnectedSessions,
                instanceId = metrics.InstanceId,
                trackingSince = metrics.TrackingSince
            }, jsonOptions);
        })
        .WithName("GetActiveSessionsByPath")
        .WithDescription("Get active session counts grouped by URL path");

        // GET /api/session-monitor/active-by-client
        group.MapGet("/active-by-client", (ISessionMonitorService monitor) =>
        {
            var metrics = monitor.GetCurrentMetrics();
            var summaries = monitor.GetActiveSessionsByClient().ToList();
            return Results.Json(new
            {
                trackingMode = metrics.TrackingMode,
                isDegradedMode = metrics.IsDegradedMode,
                clients = summaries,
                uniqueBrowsers = summaries.Count,
                totalActiveSessions = metrics.ActiveSessions,
                retainedSessions = metrics.DisconnectedSessions,
                instanceId = metrics.InstanceId,
                trackingSince = metrics.TrackingSince
            }, jsonOptions);
        })
        .WithName("GetActiveSessionsByClient")
        .WithDescription("Get active session counts grouped by browser client (shared cookie across tabs)");

        // GET /api/session-monitor/deployment-windows?windowMinutes=5&lookbackHours=24
        group.MapGet("/deployment-windows", (
            ISessionMonitorService monitor,
            int windowMinutes = 5,
            int lookbackHours = 24) =>
        {
            var error = SessionMonitorQueryLimits.ValidateDeploymentWindow(windowMinutes, lookbackHours);
            if (error is not null)
            {
                return Results.BadRequest(new { message = error });
            }

            var windows = monitor.FindOptimalDeploymentWindows(windowMinutes, lookbackHours);
            return Results.Json(windows, jsonOptions);
        })
        .WithName("GetOptimalDeploymentWindows")
        .WithDescription("Find optimal deployment windows with minimal active sessions");

        // GET /api/session-monitor/can-deploy?maxActiveSessions=0
        group.MapGet("/can-deploy", (
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
                machineName = metrics.MachineName,
                trackingSince = metrics.TrackingSince,
                scope = "per-replica"
            }, jsonOptions);
        })
        .WithName("CanDeploy")
        .WithDescription("Check if deployment is safe based on active session count");

        return endpoints;
    }
}
