// Licensed under the Apache License, Version 2.0.
// See LICENSE file in the project root for full license information.

using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TheNerdCollective.Cli.Mollie;
using TheNerdCollective.Integrations.Mollie;
using TheNerdCollective.Integrations.Mollie.Extensions;
using TheNerdCollective.Integrations.Mollie.Models;
using Xunit;

namespace TheNerdCollective.Integrations.Mollie.Tests;

public sealed class MollieApiClientTests
{
    [Fact]
    public async Task GetCurrentProfileAsync_UsesBearerKeyAndReturnsProfile()
    {
        string? authorization = null;
        Uri? requestUri = null;
        using var httpClient = CreateHttpClient((request, _) =>
        {
            authorization = request.Headers.Authorization?.ToString();
            requestUri = request.RequestUri;
            return JsonResponse("""{"id":"pfl_test","mode":"test","name":"Example"}""");
        });
        var client = CreateClient(httpClient);

        var profile = await client.GetCurrentProfileAsync();

        Assert.Equal("Bearer test_placeholder", authorization);
        Assert.Equal("https://api.mollie.com/v2/profiles/me", requestUri?.ToString());
        Assert.Equal("pfl_test", profile.Id);
        Assert.Equal("test", profile.Mode);
    }

    [Fact]
    public async Task CreatePaymentAsync_SendsRecurringFieldsAndIdempotencyKey()
    {
        string? requestBody = null;
        string? idempotencyKey = null;
        using var httpClient = CreateHttpClient(async (request, cancellationToken) =>
        {
            requestBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            idempotencyKey = request.Headers.GetValues("Idempotency-Key").Single();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"id":"tr_test","status":"open","sequenceType":"first","customerId":"cst_test"}""",
                    Encoding.UTF8,
                    "application/json")
            };
        });
        var client = CreateClient(httpClient);
        var paymentRequest = new MolliePaymentCreateRequest
        {
            Amount = new MollieAmount("DKK", "1.00"),
            Description = "Mandate setup",
            RedirectUrl = "https://example.test/paid",
            CustomerId = "cst_test",
            SequenceType = "first"
        };

        var payment = await client.CreatePaymentAsync(paymentRequest, "attempt-1");

        using var json = JsonDocument.Parse(requestBody!);
        Assert.Equal("attempt-1", idempotencyKey);
        Assert.Equal("first", json.RootElement.GetProperty("sequenceType").GetString());
        Assert.Equal("cst_test", json.RootElement.GetProperty("customerId").GetString());
        Assert.Equal("1.00", json.RootElement.GetProperty("amount").GetProperty("value").GetString());
        Assert.Equal("tr_test", payment.Id);
    }

    [Fact]
    public async Task CreatePaymentAsync_RequiresCustomerForFirstSequence()
    {
        using var httpClient = CreateHttpClient((_, _) => throw new InvalidOperationException("The request must not be sent."));
        var client = CreateClient(httpClient);
        var request = new MolliePaymentCreateRequest
        {
            Amount = new MollieAmount("DKK", "1.00"),
            Description = "Mandate setup",
            RedirectUrl = "https://example.test/paid",
            SequenceType = "first"
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => client.CreatePaymentAsync(request));

        Assert.Contains("CustomerId is required", exception.Message);
    }

    [Fact]
    public async Task CreateSubscriptionAsync_UsesCustomerScopedEndpointAndIdempotencyKey()
    {
        Uri? requestUri = null;
        string? idempotencyKey = null;
        using var httpClient = CreateHttpClient(async (request, cancellationToken) =>
        {
            requestUri = request.RequestUri;
            idempotencyKey = request.Headers.GetValues("Idempotency-Key").Single();
            _ = await request.Content!.ReadAsStringAsync(cancellationToken);
            return await JsonResponse("""{"id":"sub_test","status":"active"}""");
        });
        var client = CreateClient(httpClient);

        var subscription = await client.CreateSubscriptionAsync(
            "cst_test",
            new MollieSubscriptionCreateRequest
            {
                Amount = new MollieAmount("DKK", "99.00"),
                Description = "Consent monthly",
                Interval = "1 month",
                StartDate = "2026-11-01"
            },
            "subscription-attempt");

        Assert.Equal("https://api.mollie.com/v2/customers/cst_test/subscriptions", requestUri?.ToString());
        Assert.Equal("subscription-attempt", idempotencyKey);
        Assert.Equal("sub_test", subscription.Id);
    }

    [Fact]
    public async Task CancelSubscriptionAsync_UsesCustomerScopedDelete()
    {
        HttpMethod? method = null;
        Uri? requestUri = null;
        using var httpClient = CreateHttpClient((request, _) =>
        {
            method = request.Method;
            requestUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
        });
        var client = CreateClient(httpClient);

        await client.CancelSubscriptionAsync("cst_test", "sub_test");

        Assert.Equal(HttpMethod.Delete, method);
        Assert.Equal("https://api.mollie.com/v2/customers/cst_test/subscriptions/sub_test", requestUri?.ToString());
    }

    [Fact]
    public async Task ApiFailureDoesNotIncludeResponseBodyOrApiKey()
    {
        const string sensitiveText = "test_placeholder";
        using var httpClient = CreateHttpClient((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent(sensitiveText, Encoding.UTF8, "application/json")
            }));
        var client = CreateClient(httpClient);

        var exception = await Assert.ThrowsAsync<MollieApiException>(() => client.GetCurrentProfileAsync());

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        Assert.DoesNotContain(sensitiveText, exception.Message);
    }

    [Fact]
    public void AddMollieIntegration_BindsConfiguredApiKey()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Mollie:ApiKey"] = "test_placeholder"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddMollieIntegration(configuration);

        using var provider = services.BuildServiceProvider();

        Assert.Equal("test_placeholder", provider.GetRequiredService<IOptions<MollieOptions>>().Value.ApiKey);
    }

    [Fact]
    public async Task ProductTemplateStore_PersistsAddsAndRemovesLocalTemplates()
    {
        var root = Path.Combine(Path.GetTempPath(), $"tnc-mollie-tests-{Guid.NewGuid():N}");
        var store = new ProductTemplateStore(Path.Combine(root, "templates.json"));
        try
        {
            await store.AddAsync(new ProductTemplate("monthly", "Monthly", "99.00", "dkk", ""));
            var listed = await store.ListAsync();

            Assert.Collection(listed, template =>
            {
                Assert.Equal("monthly", template.Sku);
                Assert.Equal("Monthly", template.Description);
                Assert.Equal("DKK", template.Currency);
            });

            await store.RemoveAsync("MONTHLY");
            Assert.Empty(await store.ListAsync());
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    private static MollieApiClient CreateClient(HttpClient httpClient) =>
        new(httpClient, Options.Create(new MollieOptions { ApiKey = "test_placeholder" }));

    private static HttpClient CreateHttpClient(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) =>
        new(new StubHttpMessageHandler(handler));

    private static Task<HttpResponseMessage> JsonResponse(string json) =>
        Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            handler(request, cancellationToken);
    }
}
