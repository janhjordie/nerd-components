using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using TheNerdCollective.Blazor.SessionMonitor;
using Xunit;

namespace TheNerdCollective.Blazor.SessionMonitor.Tests;

public sealed class SessionMonitorClientIdResolverTests
{
    [Fact]
    public void EnsureClientId_stores_new_id_in_items_for_same_request()
    {
        var context = new DefaultHttpContext();
        const string cookieName = ".bs-sm-client";

        var clientId = SessionMonitorClientIdResolver.EnsureClientId(context, cookieName);

        Assert.False(string.IsNullOrWhiteSpace(clientId));
        Assert.Equal(clientId, SessionMonitorClientIdResolver.Resolve(context, cookieName));
        Assert.Contains("Set-Cookie", context.Response.Headers.Keys);
    }

    [Fact]
    public void Resolve_reads_existing_request_cookie()
    {
        var context = new DefaultHttpContext();
        const string cookieName = ".bs-sm-client";
        context.Request.Headers.Cookie = $"{cookieName}=existing-client-id";

        var clientId = SessionMonitorClientIdResolver.Resolve(context, cookieName);

        Assert.Equal("existing-client-id", clientId);
        Assert.Equal("existing-client-id", context.Items[SessionMonitorClientIdResolver.HttpContextItemKey]);
    }

    [Fact]
    public async Task Middleware_does_not_mint_a_client_id_when_the_cookie_is_missing()
    {
        var context = new DefaultHttpContext();
        var middleware = new SessionMonitorClientIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context, Options.Create(new SessionMonitorOptions()));

        Assert.False(context.Response.Headers.ContainsKey("Set-Cookie"));
        Assert.Null(SessionMonitorClientIdResolver.Resolve(context, ".bs-sm-client"));
    }

    [Fact]
    public void Cookie_options_are_site_wide_and_honor_forwarded_https()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Forwarded-Proto"] = "https";

        var options = SessionMonitorClientIdMiddleware.CreateCookieOptions(context);

        Assert.Equal("/", options.Path);
        Assert.True(options.Secure);
        Assert.Equal(SameSiteMode.Lax, options.SameSite);
        Assert.True(options.HttpOnly);
    }
}
