// Licensed under the Apache License, Version 2.0.
// See LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace TheNerdCollective.Integrations.Brevo;

/// <summary>
/// Client for Brevo API v3 contacts and transactional email.
/// https://developers.brevo.com/reference/createcontact
/// </summary>
public sealed class BrevoService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly BrevoOptions _options;

    public BrevoService(HttpClient httpClient, IOptions<BrevoOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        ConfigureHttpClient();
    }

    /// <summary>
    /// Gets whether an API key is configured.
    /// </summary>
    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.ApiKey);

    /// <summary>
    /// Creates a contact, or updates it when the email already exists.
    /// Attribute names must already exist in the Brevo account.
    /// </summary>
    public Task<BrevoResult> CreateOrUpdateContactAsync(
        string email,
        IReadOnlyDictionary<string, string>? attributes = null,
        IReadOnlyList<long>? listIds = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var body = new Dictionary<string, object?>
        {
            ["email"] = email.Trim(),
            ["updateEnabled"] = true
        };

        if (attributes is { Count: > 0 })
        {
            body["attributes"] = attributes;
        }

        if (listIds is { Count: > 0 })
        {
            body["listIds"] = listIds;
        }

        return SendAsync(HttpMethod.Post, "contacts", body, cancellationToken);
    }

    /// <summary>
    /// Sends one transactional email. The sender address must be verified in Brevo.
    /// </summary>
    public Task<BrevoResult> SendEmailAsync(
        string senderEmail,
        string? senderName,
        string toEmail,
        string? toName,
        string subject,
        string textContent,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(senderEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(toEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);

        string text = textContent ?? string.Empty;
        var body = new
        {
            sender = new { email = senderEmail.Trim(), name = string.IsNullOrWhiteSpace(senderName) ? null : senderName.Trim() },
            to = new[] { new { email = toEmail.Trim(), name = string.IsNullOrWhiteSpace(toName) ? null : toName.Trim() } },
            subject = subject.Trim(),
            textContent = text,
            htmlContent = "<p>" + WebUtility.HtmlEncode(text).Replace("\n", "<br>") + "</p>"
        };

        return SendAsync(HttpMethod.Post, "smtp/email", body, cancellationToken);
    }

    private void ConfigureHttpClient()
    {
        if (_httpClient.BaseAddress is null)
        {
            string baseUrl = string.IsNullOrWhiteSpace(_options.BaseUrl)
                ? "https://api.brevo.com/v3/"
                : _options.BaseUrl;
            if (!baseUrl.EndsWith('/'))
            {
                baseUrl += "/";
            }

            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        if (!string.IsNullOrWhiteSpace(_options.ApiKey)
            && !_httpClient.DefaultRequestHeaders.Contains("api-key"))
        {
            _httpClient.DefaultRequestHeaders.Add("api-key", _options.ApiKey);
        }

        if (!_httpClient.DefaultRequestHeaders.Accept.Any())
        {
            _httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        }
    }

    private async Task<BrevoResult> SendAsync(HttpMethod method, string path, object body, CancellationToken cancellationToken)
    {
        if (!IsConfigured)
        {
            return BrevoResult.Fail(0, "Brevo API key is not configured.");
        }

        using HttpRequestMessage request = new(method, path)
        {
            Content = JsonContent.Create(body, options: SerializerOptions)
        };

        try
        {
            using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return BrevoResult.Ok((int)response.StatusCode);
            }

            string error = await response.Content.ReadAsStringAsync(cancellationToken);
            return BrevoResult.Fail((int)response.StatusCode, error);
        }
        catch (HttpRequestException exception)
        {
            return BrevoResult.Fail(0, exception.Message);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            return BrevoResult.Fail(0, exception.Message);
        }
    }
}
