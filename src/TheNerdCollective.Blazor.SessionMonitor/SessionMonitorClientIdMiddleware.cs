using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace TheNerdCollective.Blazor.SessionMonitor;

/// <summary>
/// Reads the browser client cookie on the request. The id is created by the browser bootstrap, not by this middleware.
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
        if (!string.IsNullOrWhiteSpace(cookieName))
        {
            // Read a client id supplied by the browser bootstrap. Never mint one here:
            // parallel requests would each become a different browser.
            SessionMonitorClientIdResolver.Resolve(context, cookieName);
        }

        await _next(context);
    }

    internal static bool IsSecureRequest(HttpContext context)
    {
        if (context.Request.IsHttps)
        {
            return true;
        }

        var forwarded = context.Request.Headers["X-Forwarded-Proto"].ToString();
        var first = forwarded.Split(',')[0].Trim();
        return first.Equals("https", StringComparison.OrdinalIgnoreCase);
    }

    internal static CookieOptions CreateCookieOptions(HttpContext context)
        => new()
        {
            HttpOnly = true,
            Path = "/",
            Secure = IsSecureRequest(context),
            SameSite = SameSiteMode.Lax,
            MaxAge = TimeSpan.FromDays(365),
            IsEssential = true,
        };
}
