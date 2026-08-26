using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace TheNerdCollective.Blazor.SessionMonitor;

/// <summary>
/// Ensures the reload-dedup client cookie exists on the initial HTTP response.
/// Must not set cookies from <see cref="CircuitHandler"/> — the response is already committed when the circuit opens.
/// </summary>
internal sealed class SessionMonitorClientIdMiddleware
{
    private readonly RequestDelegate _next;

    public SessionMonitorClientIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IOptions<SessionMonitorOptions> options)
    {
        var cookieName = options.Value.ClientIdCookieName;
        if (!string.IsNullOrWhiteSpace(cookieName)
            && (!context.Request.Cookies.TryGetValue(cookieName, out var existing)
                || string.IsNullOrWhiteSpace(existing)))
        {
            context.Response.Cookies.Append(cookieName, Guid.NewGuid().ToString("N"), CreateCookieOptions(context));
        }

        await _next(context);
    }

    internal static CookieOptions CreateCookieOptions(HttpContext context)
        => new()
        {
            HttpOnly = true,
            Secure = context.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            MaxAge = TimeSpan.FromDays(365),
            IsEssential = true,
        };
}
