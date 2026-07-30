using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using TheNerdCollective.Blazor.Observability;
using TheNerdCollective.Blazor.Observability.SigNoz;

namespace TheNerdCollective.Blazor.Observability.SigNoz.Tests;

public sealed class SigNozObservabilityBackendTests
{
    [Fact]
    public async Task QueryTimeSeriesAsync_posts_query_range_and_parses_response()
    {
        var capturedPath = string.Empty;
        var handler = new StubHttpMessageHandler(request =>
        {
            capturedPath = request.RequestUri?.AbsolutePath ?? string.Empty;
            return JsonResponse(ReadFixture("query_range_series.json"));
        });

        var backend = CreateBackend(handler, apiToken: "test-token");
        var query = new ObservabilityPanelQuery(
            ObservabilityPanelId.RequestRate,
            "nerd-consent-host",
            ObservabilityTimeRange.LastMinutes(15));

        var result = await backend.QueryTimeSeriesAsync(query);

        Assert.Equal("/api/v4/query_range", capturedPath);
        Assert.Equal(2, result.Points.Count);
    }

    [Fact]
    public async Task ListServicesAsync_sends_signoz_api_key_when_configured()
    {
        string? apiKeyHeader = null;
        var handler = new StubHttpMessageHandler(request =>
        {
            apiKeyHeader = request.Headers.TryGetValues("SIGNOZ-API-KEY", out var values)
                ? values.FirstOrDefault()
                : null;
            return JsonResponse(ReadFixture("services_list.json"));
        });

        var backend = CreateBackend(handler, apiToken: "secret-key");
        var context = new ObservabilityQueryContext(
            DateTimeOffset.UtcNow.AddMinutes(-15),
            DateTimeOffset.UtcNow);

        var services = await backend.ListServicesAsync(context);

        Assert.Equal("secret-key", apiKeyHeader);
        Assert.Single(services, s => s.Name == "nerd-consent-host");
    }

    [Fact]
    public async Task QueryScalarAsync_returns_latest_point_value()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse(ReadFixture("query_range_series.json")));
        var backend = CreateBackend(handler);
        var query = new ObservabilityPanelQuery(
            ObservabilityPanelId.RequestRate,
            "nerd-consent-host",
            ObservabilityTimeRange.LastMinutes(15));

        var scalar = await backend.QueryScalarAsync(query);

        Assert.Equal(14.2, scalar.Value, precision: 3);
        Assert.Equal("reqps", scalar.Unit);
    }

    [Fact]
    public async Task GetHealthSummaryAsync_marks_unhealthy_when_error_percentage_exceeds_threshold()
    {
        var callCount = 0;
        var handler = new StubHttpMessageHandler(_ =>
        {
            callCount++;
            return JsonResponse(callCount switch
            {
                1 => """{"data":{"result":[{"series":[{"values":[[1,"0"]]}]}]}}""",
                2 => """{"data":{"result":[{"series":[{"values":[[1,"0.10"]]}]}]}}""",
                _ => """{"data":{"result":[{"series":[{"values":[[1,"50"]]}]}]}}"""
            });
        });

        var backend = CreateBackend(handler, unhealthyErrorPercentage: 0.05);
        var context = new ObservabilityQueryContext(
            DateTimeOffset.UtcNow.AddMinutes(-15),
            DateTimeOffset.UtcNow);

        var health = await backend.GetHealthSummaryAsync("nerd-consent-host", context);

        Assert.Equal(ObservabilityHealthStatus.Unhealthy, health.Status);
        Assert.Contains("Error rate", health.Message);
    }

    private static SigNozObservabilityBackend CreateBackend(
        HttpMessageHandler handler,
        string? apiToken = null,
        double unhealthyErrorPercentage = 0.05)
    {
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://signoz.test") };
        var options = Options.Create(new ObservabilityDashboardOptions
        {
            UnhealthyErrorPercentage = unhealthyErrorPercentage
        });
        var signoz = Options.Create(new SigNozBackendOptions
        {
            BaseUrl = "http://signoz.test",
            ApiToken = apiToken
        });

        return new SigNozObservabilityBackend(new TestHttpClientFactory(client), signoz, options);
    }

    private static HttpResponseMessage JsonResponse(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

    private static string ReadFixture(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName);
        return File.ReadAllText(path);
    }
}
