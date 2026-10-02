using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TheNerdCollective.Integrations.Dar.Services.Dar.Internal;
using Xunit;

namespace TheNerdCollective.Integrations.Dar.IntegrationTests;

public sealed class DawaHttpHelperUnitTests
{
    [Fact]
    public void IsUnavailableStatus_returns_true_for_Gone_and_ServiceUnavailable()
    {
        Assert.True(DawaHttpHelper.IsUnavailableStatus(HttpStatusCode.Gone));
        Assert.True(DawaHttpHelper.IsUnavailableStatus(HttpStatusCode.ServiceUnavailable));
        Assert.False(DawaHttpHelper.IsUnavailableStatus(HttpStatusCode.NotFound));
    }

    [Fact]
    public async Task TryEnsureSuccessAsync_returns_false_for_410_without_throwing()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.Gone)
        {
            Content = new StringContent("{\"status\":410}")
        };

        var ok = await DawaHttpHelper.TryEnsureSuccessAsync(response, "DAWA test");
        Assert.False(ok);
    }

    [Fact]
    public async Task TryEnsureSuccessAsync_returns_true_for_200()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        var ok = await DawaHttpHelper.TryEnsureSuccessAsync(response, "DAWA test");
        Assert.True(ok);
    }
}
