using System.Net;
using System.Text.Json.Nodes;
using TheNerdCollective.Blazor.Observability;
using TheNerdCollective.Blazor.Observability.SigNoz;

namespace TheNerdCollective.Blazor.Observability.SigNoz.Tests;

public sealed class SigNozExtensionPointTests
{
    [Fact]
    public void SigNozRuntimeProfileProvider_prefers_discovered_profile_over_options()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new SigNozBackendOptions
        {
            QueryRangePath = "/api/v5/query_range",
            SchemaVersion = "v1"
        });
        var provider = new SigNozRuntimeProfileProvider(options);
        provider.SetProfile(new SigNozRuntimeProfile("0.135.0", "/api/v4/query_range", null, DateTimeOffset.UtcNow));

        Assert.Equal("/api/v4/query_range", provider.ResolveQueryRangePath());
        Assert.Null(provider.ResolveSchemaVersion());
    }

    [Fact]
    public void Custom_response_parser_runs_before_built_in()
    {
        var custom = new StubParser(points: 99);
        var coordinator = new SigNozResponseParserCoordinator(
        [
            custom,
            new BuiltInSigNozResponseParser(),
            new DeepWalkSigNozResponseParser()
        ]);

        var result = coordinator.ParseTimeSeries(
            """{"data":{"results":[]}}""",
            new SigNozParseContext(ObservabilityPanelId.RequestRate, "/api/v5/query_range", null, 200));

        Assert.Equal(99, result.Points.Count);
        Assert.True(custom.WasCalled);
    }

    [Fact]
    public async Task Query_mutator_can_override_filter_expression()
    {
        JsonObject? captured = null;
        var mutator = new StubMutator((body, _, _) => captured = body);
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"data":{"result":[{"series":[{"values":[[1,"1"]]}]}]}}""")
        });

        var backend = SigNozObservabilityBackendTestsHelper.CreateQueryClient(
            handler,
            [mutator],
            out _);
        var query = new ObservabilityPanelQuery(
            ObservabilityPanelId.HostCpuUtilization,
            "nerd-consent-host",
            ObservabilityTimeRange.LastMinutes(15));

        await backend.QueryTimeSeriesAsync(
            query,
            new SigNozQueryOverrides(FilterExpression: "host.name = 'nerd-consent'"));

        var expression = captured?["compositeQuery"]?["queries"]?[0]?["spec"]?["filter"]?["expression"]?.GetValue<string>();
        Assert.Equal("host.name = 'nerd-consent'", expression);
    }

    private sealed class StubParser(int points) : ISigNozResponseParser
    {
        public bool WasCalled { get; private set; }

        public ObservabilityTimeSeriesResult? TryParseTimeSeries(string json, SigNozParseContext context)
        {
            WasCalled = true;
            var definition = ObservabilityPanelCatalog.GetDefinition(context.PanelId);
            var series = Enumerable.Range(0, points)
                .Select(i => new ObservabilityTimeSeriesPoint(DateTimeOffset.UtcNow.AddMinutes(i), i))
                .ToList();
            return new ObservabilityTimeSeriesResult(definition.Legend, definition.Unit, series);
        }

        public IReadOnlyList<ObservabilityServiceInfo>? TryParseServices(string json, SigNozParseContext context) =>
            null;
    }

    private sealed class StubMutator(Action<JsonObject, ObservabilityPanelQuery, SigNozQueryContext> mutate) : ISigNozQueryMutator
    {
        public void MutateQueryRangeBody(JsonObject body, ObservabilityPanelQuery query, SigNozQueryContext context) =>
            mutate(body, query, context);
    }
}
