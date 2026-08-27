using Microsoft.AspNetCore.Http;
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
}
