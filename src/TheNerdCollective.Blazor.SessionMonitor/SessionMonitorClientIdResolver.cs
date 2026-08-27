using Microsoft.AspNetCore.Http;

namespace TheNerdCollective.Blazor.SessionMonitor;

/// <summary>
/// Resolves the browser client id from request cookies and same-request HttpContext items.
/// </summary>
internal static class SessionMonitorClientIdResolver
{
    internal const string HttpContextItemKey = "__SessionMonitorClientId";

    internal static string? Resolve(HttpContext? context, string cookieName)
    {
        if (context is null || string.IsNullOrWhiteSpace(cookieName))
        {
            return null;
        }

        if (context.Items.TryGetValue(HttpContextItemKey, out var item)
            && item is string fromItems
            && !string.IsNullOrWhiteSpace(fromItems))
        {
            return fromItems;
        }

        if (context.Request.Cookies.TryGetValue(cookieName, out var existing)
            && !string.IsNullOrWhiteSpace(existing))
        {
            context.Items[HttpContextItemKey] = existing;
            return existing;
        }

        return null;
    }

    internal static string EnsureClientId(HttpContext context, string cookieName)
    {
        var existing = Resolve(context, cookieName);
        if (!string.IsNullOrWhiteSpace(existing))
        {
            return existing;
        }

        var clientId = Guid.NewGuid().ToString("N");
        context.Response.Cookies.Append(
            cookieName,
            clientId,
            SessionMonitorClientIdMiddleware.CreateCookieOptions(context));
        context.Items[HttpContextItemKey] = clientId;
        return clientId;
    }
}
