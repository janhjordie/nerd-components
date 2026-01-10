using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;

namespace TheNerdCollective.Services.BlazorServer;

public static class ReconnectionStatusEndpointExtensions
{
    /// <summary>
    /// Maps a lightweight status endpoint used by the Blazor reconnection UI
    /// to improve UX during deployments and restarts.
    /// </summary>
    /// <param name="endpoints">Endpoint route builder</param>
    /// <param name="pattern">Route pattern, defaults to "/reconnection-status.json"</param>
    /// <param name="factory">Optional factory that can produce dynamic status</param>
    public static void MapBlazorReconnectionStatusEndpoint(
        this IEndpointRouteBuilder endpoints,
        string pattern = "/reconnection-status.json",
        Func<HttpContext, Task<ReconnectionStatus>>? factory = null)
    {
        endpoints.MapGet(pattern, async (HttpContext ctx) =>
        {
            ReconnectionStatus status;
            if (factory != null)
            {
                status = await factory(ctx);
            }
            else
            {
                var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
                status = new ReconnectionStatus
                {
                    Status = "ok",
                    ReconnectingMessage = "Reconnecting...",
                    Version = version
                };
            }

            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsJsonAsync(status);
        });
    }

    /// <summary>
    /// Convenience overload for WebApplication usage.
    /// </summary>
    public static void MapBlazorReconnectionStatusEndpoint(
        this WebApplication app,
        string pattern = "/reconnection-status.json",
        Func<HttpContext, Task<ReconnectionStatus>>? factory = null)
        => ((IEndpointRouteBuilder)app).MapBlazorReconnectionStatusEndpoint(pattern, factory);
}
