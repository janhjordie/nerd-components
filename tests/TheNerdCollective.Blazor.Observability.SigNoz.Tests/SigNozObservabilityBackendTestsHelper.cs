using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using TheNerdCollective.Blazor.Observability.SigNoz;

namespace TheNerdCollective.Blazor.Observability.SigNoz.Tests;

internal static class SigNozObservabilityBackendTestsHelper
{
    public static SigNozQueryClient CreateQueryClient(
        HttpMessageHandler handler,
        IEnumerable<ISigNozQueryMutator> mutators,
        out TestHttpClientFactory factory)
    {
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://signoz.test") };
        factory = new TestHttpClientFactory(client);
        var signoz = Options.Create(new SigNozBackendOptions
        {
            BaseUrl = "http://signoz.test"
        });
        var parsers = new ISigNozResponseParser[]
        {
            new BuiltInSigNozResponseParser(),
            new DeepWalkSigNozResponseParser()
        };
        var coordinator = new SigNozResponseParserCoordinator(parsers);
        var profileProvider = new SigNozRuntimeProfileProvider(signoz);
        return new SigNozQueryClient(factory, signoz, profileProvider, coordinator, mutators);
    }

    public static HttpResponseMessage JsonResponse(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
}
