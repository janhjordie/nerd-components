// Licensed under the Apache License, Version 2.0.
// See LICENSE file in the project root for full license information.

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using TheNerdCollective.Integrations.Mollie.Models;

namespace TheNerdCollective.Integrations.Mollie;

/// <summary>
/// Typed client for Mollie's Payments API.
/// </summary>
public sealed class MollieApiClient
{
    private static readonly Uri ApiBaseAddress = new("https://api.mollie.com/v2/");
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="MollieApiClient"/> class.
    /// </summary>
    public MollieApiClient(HttpClient httpClient, IOptions<MollieOptions> options)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);

        var apiKey = options.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("A Mollie API key is required.");
        }

        _httpClient = httpClient;
        _httpClient.BaseAddress = ApiBaseAddress;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("TheNerdCollective.Integrations.Mollie/0.1.0");
    }

    /// <summary>
    /// Gets the current Mollie profile associated with the API key.
    /// </summary>
    public Task<MollieProfile> GetCurrentProfileAsync(CancellationToken cancellationToken = default) =>
        SendAsync<MollieProfile>(HttpMethod.Get, "profiles/me", null, null, cancellationToken);

    /// <summary>
    /// Gets available payment methods for the profile.
    /// </summary>
    public async Task<IReadOnlyList<MollieMethod>> GetPaymentMethodsAsync(
        string resource = "payments",
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resource);
        var path = $"methods?resource={Uri.EscapeDataString(resource)}";
        var response = await SendAsync<MollieMethodsResponse>(HttpMethod.Get, path, null, null, cancellationToken);
        return response.Embedded?.Methods ?? [];
    }

    /// <summary>
    /// Creates a payment. Mollie resources created by this method are real provider-side resources.
    /// </summary>
    public Task<MolliePayment> CreatePaymentAsync(
        MolliePaymentCreateRequest request,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Amount);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Description);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.RedirectUrl);

        if (request.SequenceType is not null
            && request.SequenceType is not ("oneoff" or "first" or "recurring"))
        {
            throw new ArgumentException("SequenceType must be 'oneoff', 'first', or 'recurring'.", nameof(request));
        }

        if (request.SequenceType is "first" or "recurring" && string.IsNullOrWhiteSpace(request.CustomerId))
        {
            throw new ArgumentException("CustomerId is required for first and recurring payments.", nameof(request));
        }

        return SendAsync<MolliePayment>(HttpMethod.Post, "payments", request, idempotencyKey, cancellationToken);
    }

    /// <summary>
    /// Gets a payment by its Mollie identifier.
    /// </summary>
    public Task<MolliePayment> GetPaymentAsync(string paymentId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(paymentId);
        return SendAsync<MolliePayment>(
            HttpMethod.Get,
            $"payments/{Uri.EscapeDataString(paymentId)}",
            null,
            null,
            cancellationToken);
    }

    /// <summary>
    /// Creates a Mollie customer.
    /// </summary>
    public Task<MollieCustomer> CreateCustomerAsync(
        MollieCustomerCreateRequest request,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Email);
        return SendAsync<MollieCustomer>(HttpMethod.Post, "customers", request, idempotencyKey, cancellationToken);
    }

    /// <summary>
    /// Gets a customer by its Mollie identifier.
    /// </summary>
    public Task<MollieCustomer> GetCustomerAsync(string customerId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customerId);
        return SendAsync<MollieCustomer>(
            HttpMethod.Get,
            $"customers/{Uri.EscapeDataString(customerId)}",
            null,
            null,
            cancellationToken);
    }

    /// <summary>
    /// Creates a recurring subscription for a customer with an active mandate.
    /// </summary>
    public Task<MollieSubscription> CreateSubscriptionAsync(
        string customerId,
        MollieSubscriptionCreateRequest request,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customerId);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Amount);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Description);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Interval);

        var path = $"customers/{Uri.EscapeDataString(customerId)}/subscriptions";
        return SendAsync<MollieSubscription>(HttpMethod.Post, path, request, idempotencyKey, cancellationToken);
    }

    /// <summary>
    /// Gets a subscription by customer and subscription identifiers.
    /// </summary>
    public Task<MollieSubscription> GetSubscriptionAsync(
        string customerId,
        string subscriptionId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionId);
        var path = $"customers/{Uri.EscapeDataString(customerId)}/subscriptions/{Uri.EscapeDataString(subscriptionId)}";
        return SendAsync<MollieSubscription>(HttpMethod.Get, path, null, null, cancellationToken);
    }

    /// <summary>
    /// Cancels a subscription.
    /// </summary>
    public Task CancelSubscriptionAsync(
        string customerId,
        string subscriptionId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionId);
        var path = $"customers/{Uri.EscapeDataString(customerId)}/subscriptions/{Uri.EscapeDataString(subscriptionId)}";
        return SendForStatusAsync(HttpMethod.Delete, path, cancellationToken);
    }

    private async Task<TResponse> SendAsync<TResponse>(
        HttpMethod method,
        string path,
        object? body,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        AddIdempotencyKey(request, idempotencyKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new MollieApiException(response.StatusCode);
        }

        var result = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);
        return result ?? throw new InvalidOperationException("Mollie returned an empty response.");
    }

    private async Task SendForStatusAsync(HttpMethod method, string path, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new MollieApiException(response.StatusCode);
        }
    }

    private static void AddIdempotencyKey(HttpRequestMessage request, string? idempotencyKey)
    {
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            request.Headers.Add("Idempotency-Key", idempotencyKey);
        }
    }
}
