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

    /// <summary>
    /// Returns the authenticated Brevo account (connection test).
    /// </summary>
    public Task<BrevoDataResult<BrevoAccount>> GetAccountAsync(CancellationToken cancellationToken = default)
        => GetAsync(
            "account",
            document =>
            {
                string email = document.TryGetProperty("email", out JsonElement emailElement)
                    ? emailElement.GetString() ?? string.Empty
                    : string.Empty;
                string? company = document.TryGetProperty("companyName", out JsonElement companyElement)
                    ? companyElement.GetString()
                    : null;
                return new BrevoAccount(email, company, ReadPlanType(document));
            },
            cancellationToken);

    /// <summary>
    /// Lists a page of contacts.
    /// </summary>
    public Task<BrevoDataResult<BrevoContactListResult>> ListContactsAsync(
        int limit = 10,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 50);
        offset = Math.Max(0, offset);
        return GetAsync(
            $"contacts?limit={limit}&offset={offset}&sort=desc",
            document =>
            {
                int count = document.TryGetProperty("count", out JsonElement countElement)
                    ? countElement.GetInt32()
                    : 0;
                List<BrevoContactSummary> contacts = new();
                if (document.TryGetProperty("contacts", out JsonElement contactsElement)
                    && contactsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement item in contactsElement.EnumerateArray())
                    {
                        string contactEmail = item.TryGetProperty("email", out JsonElement emailElement)
                            ? emailElement.GetString() ?? string.Empty
                            : string.Empty;
                        string? firstName = null;
                        string? lastName = null;
                        if (item.TryGetProperty("attributes", out JsonElement attributes)
                            && attributes.ValueKind == JsonValueKind.Object)
                        {
                            if (attributes.TryGetProperty("FIRSTNAME", out JsonElement first))
                            {
                                firstName = first.GetString();
                            }

                            if (attributes.TryGetProperty("LASTNAME", out JsonElement last))
                            {
                                lastName = last.GetString();
                            }
                        }

                        contacts.Add(new BrevoContactSummary(contactEmail, firstName, lastName));
                    }
                }

                return new BrevoContactListResult(count, contacts);
            },
            cancellationToken);
    }

    /// <summary>Lists contact lists (newsletter lists).</summary>
    public Task<BrevoDataResult<BrevoPagedResult<BrevoListSummary>>> ListContactListsAsync(
        int limit = 50,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 50);
        offset = Math.Max(0, offset);
        return GetAsync(
            $"contacts/lists?limit={limit}&offset={offset}&sort=desc",
            ReadListPage,
            cancellationToken);
    }

    /// <summary>Lists contact segments.</summary>
    public Task<BrevoDataResult<BrevoPagedResult<BrevoSegmentSummary>>> ListSegmentsAsync(
        int limit = 50,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 50);
        offset = Math.Max(0, offset);
        return GetAsync(
            $"contacts/segments?limit={limit}&offset={offset}&sort=desc",
            ReadSegmentPage,
            cancellationToken);
    }

    /// <summary>Lists email campaigns.</summary>
    public Task<BrevoDataResult<BrevoPagedResult<BrevoCampaignSummary>>> ListEmailCampaignsAsync(
        int limit = 50,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 50);
        offset = Math.Max(0, offset);
        return GetAsync(
            $"emailCampaigns?limit={limit}&offset={offset}&sort=desc",
            ReadCampaignPage,
            cancellationToken);
    }

    private static BrevoPagedResult<BrevoListSummary> ReadListPage(JsonElement document)
    {
        int count = document.TryGetProperty("count", out JsonElement countElement)
            ? countElement.GetInt32()
            : 0;
        List<BrevoListSummary> items = new();
        if (document.TryGetProperty("lists", out JsonElement listsElement)
            && listsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in listsElement.EnumerateArray())
            {
                long id = item.TryGetProperty("id", out JsonElement idElement) ? idElement.GetInt64() : 0;
                string name = item.TryGetProperty("name", out JsonElement nameElement)
                    ? nameElement.GetString() ?? string.Empty
                    : string.Empty;
                int? total = item.TryGetProperty("totalSubscribers", out JsonElement totalElement)
                    ? totalElement.GetInt32()
                    : null;
                items.Add(new BrevoListSummary(id, name, total));
            }
        }

        return new BrevoPagedResult<BrevoListSummary>(count, items);
    }

    private static BrevoPagedResult<BrevoSegmentSummary> ReadSegmentPage(JsonElement document)
    {
        int count = document.TryGetProperty("count", out JsonElement countElement)
            ? countElement.GetInt32()
            : 0;
        List<BrevoSegmentSummary> items = new();
        if (document.TryGetProperty("segments", out JsonElement segmentsElement)
            && segmentsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in segmentsElement.EnumerateArray())
            {
                long id = item.TryGetProperty("id", out JsonElement idElement) ? idElement.GetInt64() : 0;
                string name = item.TryGetProperty("name", out JsonElement nameElement)
                    ? nameElement.GetString() ?? string.Empty
                    : string.Empty;
                string? category = item.TryGetProperty("categoryName", out JsonElement categoryElement)
                    ? categoryElement.GetString()
                    : null;
                items.Add(new BrevoSegmentSummary(id, name, category));
            }
        }

        return new BrevoPagedResult<BrevoSegmentSummary>(count, items);
    }

    private static BrevoPagedResult<BrevoCampaignSummary> ReadCampaignPage(JsonElement document)
    {
        int count = document.TryGetProperty("count", out JsonElement countElement)
            ? countElement.GetInt32()
            : 0;
        List<BrevoCampaignSummary> items = new();
        if (document.TryGetProperty("campaigns", out JsonElement campaignsElement)
            && campaignsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in campaignsElement.EnumerateArray())
            {
                long id = item.TryGetProperty("id", out JsonElement idElement) ? idElement.GetInt64() : 0;
                string name = item.TryGetProperty("name", out JsonElement nameElement)
                    ? nameElement.GetString() ?? string.Empty
                    : string.Empty;
                string? subject = item.TryGetProperty("subject", out JsonElement subjectElement)
                    ? subjectElement.GetString()
                    : null;
                string? status = item.TryGetProperty("status", out JsonElement statusElement)
                    ? statusElement.GetString()
                    : null;
                string? createdAt = item.TryGetProperty("createdAt", out JsonElement createdElement)
                    ? createdElement.GetString()
                    : null;
                string? sentDate = item.TryGetProperty("sentDate", out JsonElement sentElement)
                    ? sentElement.GetString()
                    : null;
                items.Add(new BrevoCampaignSummary(id, name, subject, status, createdAt, sentDate));
            }
        }

        return new BrevoPagedResult<BrevoCampaignSummary>(count, items);
    }

    private static string? ReadPlanType(JsonElement document)
    {
        if (!document.TryGetProperty("plan", out JsonElement planElement))
        {
            return null;
        }

        if (planElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in planElement.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object
                    && item.TryGetProperty("type", out JsonElement typeElement))
                {
                    return typeElement.GetString();
                }
            }

            return null;
        }

        if (planElement.ValueKind == JsonValueKind.Object
            && planElement.TryGetProperty("type", out JsonElement planTypeElement))
        {
            return planTypeElement.GetString();
        }

        return null;
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

    private Task<BrevoDataResult<T>> GetAsync<T>(
        string path,
        Func<JsonElement, T> read,
        CancellationToken cancellationToken)
        => SendJsonAsync(HttpMethod.Get, path, null, async response =>
        {
            using JsonDocument document = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                cancellationToken: cancellationToken);
            return read(document.RootElement);
        }, cancellationToken);

    private async Task<BrevoResult> SendAsync(HttpMethod method, string path, object body, CancellationToken cancellationToken)
    {
        BrevoDataResult<bool> result = await SendJsonAsync<bool>(
            method,
            path,
            body,
            static _ => Task.FromResult(true),
            cancellationToken);
        return result.Succeeded
            ? BrevoResult.Ok(result.StatusCode)
            : BrevoResult.Fail(result.StatusCode, result.Error);
    }

    private async Task<BrevoDataResult<T>> SendJsonAsync<T>(
        HttpMethod method,
        string path,
        object? body,
        Func<HttpResponseMessage, Task<T>> readSuccess,
        CancellationToken cancellationToken)
    {
        if (!IsConfigured)
        {
            return BrevoDataResult<T>.Fail(0, "Brevo API key is not configured.");
        }

        using HttpRequestMessage request = new(method, path);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: SerializerOptions);
        }

        try
        {
            using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                T data = await readSuccess(response);
                return BrevoDataResult<T>.Ok(data, (int)response.StatusCode);
            }

            string error = await response.Content.ReadAsStringAsync(cancellationToken);
            return BrevoDataResult<T>.Fail((int)response.StatusCode, error);
        }
        catch (HttpRequestException exception)
        {
            return BrevoDataResult<T>.Fail(0, exception.Message);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            return BrevoDataResult<T>.Fail(0, exception.Message);
        }
        catch (JsonException exception)
        {
            return BrevoDataResult<T>.Fail(0, exception.Message);
        }
    }
}
