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
    private readonly IBrevoApiKeyProvider _apiKeyProvider;

    public BrevoService(HttpClient httpClient, IOptions<BrevoOptions> options)
        : this(httpClient, options, new BrevoApiKeyProvider(options))
    {
    }

    public BrevoService(HttpClient httpClient, IOptions<BrevoOptions> options, IBrevoApiKeyProvider apiKeyProvider)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _apiKeyProvider = apiKeyProvider;
        ConfigureHttpClient();
    }

    /// <summary>
    /// Gets whether an API key is configured directly in options.
    /// </summary>
    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.ApiKey);

    /// <summary>
    /// Gets whether an API key is configured.
    /// </summary>
    public async ValueTask<bool> IsConfiguredAsync(CancellationToken cancellationToken = default)
        => !string.IsNullOrWhiteSpace(await _apiKeyProvider.GetApiKeyAsync(cancellationToken));

    /// <summary>
    /// Creates a contact, or updates it when the email already exists.
    /// Attribute names must already exist in the Brevo account.
    /// </summary>
    public Task<BrevoResult> CreateOrUpdateContactAsync(
        string email,
        IReadOnlyDictionary<string, string>? attributes = null,
        IReadOnlyList<long>? listIds = null,
        CancellationToken cancellationToken = default)
        => CreateOrUpdateContactWithAttributesAsync(
            email,
            ConvertAttributes(attributes),
            listIds,
            cancellationToken);

    /// <summary>
    /// Creates or updates a contact with arbitrary typed attribute values.
    /// Attribute names must already exist in the Brevo account.
    /// </summary>
    public Task<BrevoResult> CreateOrUpdateContactWithAttributesAsync(
        string email,
        IReadOnlyDictionary<string, object?>? attributes = null,
        IReadOnlyList<long>? listIds = null,
        CancellationToken cancellationToken = default)
    {
        Dictionary<string, object?> body = CreateContactRequestBody(email, attributes, listIds);
        return SendAsync(HttpMethod.Post, "contacts", body, cancellationToken);
    }

    /// <summary>
    /// Creates or updates a contact and returns its Brevo contact ID.
    /// </summary>
    public async Task<BrevoDataResult<long>> CreateOrUpdateContactAndGetIdAsync(
        string email,
        IReadOnlyDictionary<string, string>? attributes = null,
        IReadOnlyList<long>? listIds = null,
        CancellationToken cancellationToken = default)
        => await CreateOrUpdateContactWithAttributesAndGetIdAsync(
            email,
            ConvertAttributes(attributes),
            listIds,
            cancellationToken);

    /// <summary>
    /// Creates or updates a contact with arbitrary typed attribute values and returns its Brevo contact ID.
    /// </summary>
    public async Task<BrevoDataResult<long>> CreateOrUpdateContactWithAttributesAndGetIdAsync(
        string email,
        IReadOnlyDictionary<string, object?>? attributes = null,
        IReadOnlyList<long>? listIds = null,
        CancellationToken cancellationToken = default)
    {
        Dictionary<string, object?> body = CreateContactRequestBody(email, attributes, listIds);
        return await CreateOrUpdateContactAndGetIdAsync(body, email.Trim(), cancellationToken);
    }

    /// <summary>
    /// Creates or updates a contact using its SMS phone number as the unique identifier.
    /// </summary>
    public async Task<BrevoDataResult<long>> CreateOrUpdateContactByPhoneAndGetIdAsync(
        string phone,
        IReadOnlyDictionary<string, string>? attributes = null,
        IReadOnlyList<long>? listIds = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(phone);

        var normalizedPhone = phone.Trim();
        var contactAttributes = attributes is null
            ? new Dictionary<string, object?>(StringComparer.Ordinal)
            : ConvertAttributes(attributes)!.ToDictionary(
                attribute => attribute.Key,
                attribute => attribute.Value,
                StringComparer.Ordinal);
        contactAttributes["SMS"] = normalizedPhone;

        var body = CreateContactRequestBodyWithoutEmail(contactAttributes, listIds);
        return await CreateOrUpdateContactAndGetIdAsync(body, normalizedPhone, cancellationToken);
    }

    private async Task<BrevoDataResult<long>> CreateOrUpdateContactAndGetIdAsync(
        Dictionary<string, object?> body,
        string identifier,
        CancellationToken cancellationToken)
    {
        body["getId"] = true;
        BrevoDataResult<long?> result = await SendJsonAsync<long?>(
            HttpMethod.Post,
            "contacts",
            body,
            response => ReadOptionalContactIdAsync(response, cancellationToken),
            cancellationToken);

        if (!result.Succeeded)
        {
            return BrevoDataResult<long>.Fail(result.StatusCode, result.Error);
        }

        if (result.Data is long contactId)
        {
            return BrevoDataResult<long>.Ok(contactId, result.StatusCode);
        }

        BrevoDataResult<long> lookup = await GetAsync(
            $"contacts/{Uri.EscapeDataString(identifier)}",
            ReadContactId,
            cancellationToken);

        if (!lookup.Succeeded || lookup.Data <= 0)
        {
            return BrevoDataResult<long>.Fail(
                lookup.StatusCode,
                lookup.Error ?? "Brevo did not return a contact ID after create or update.");
        }

        return BrevoDataResult<long>.Ok(lookup.Data, result.StatusCode);
    }

    /// <summary>
    /// Creates a CRM note associated with a contact.
    /// </summary>
    public Task<BrevoResult> CreateContactNoteAsync(
        long contactId,
        string text,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(contactId);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var body = new
        {
            text,
            contactIds = new[] { contactId }
        };

        return SendAsync(HttpMethod.Post, "crm/notes", body, cancellationToken);
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
    /// Lists the contact attributes configured in Brevo.
    /// </summary>
    public Task<BrevoDataResult<IReadOnlyList<BrevoContactAttribute>>> ListContactAttributesAsync(
        CancellationToken cancellationToken = default)
        => GetAsync("contacts/attributes", ReadContactAttributes, cancellationToken);

    /// <summary>
    /// Finds an attribute in Brevo's attribute catalog.
    /// </summary>
    public async Task<BrevoDataResult<BrevoContactAttribute>> GetContactAttributeAsync(
        string name,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (!string.IsNullOrWhiteSpace(category))
        {
            category = NormalizeAttributeCategory(category);
        }

        BrevoDataResult<IReadOnlyList<BrevoContactAttribute>> result =
            await ListContactAttributesAsync(cancellationToken);
        if (!result.Succeeded || result.Data is null)
        {
            return BrevoDataResult<BrevoContactAttribute>.Fail(result.StatusCode, result.Error);
        }

        BrevoContactAttribute? attribute = result.Data.FirstOrDefault(item =>
            string.Equals(item.Name, name.Trim(), StringComparison.OrdinalIgnoreCase)
            && (category is null || string.Equals(item.Category, category, StringComparison.OrdinalIgnoreCase)));

        return attribute is null
            ? BrevoDataResult<BrevoContactAttribute>.Fail(404, $"Contact attribute '{name}' was not found.")
            : BrevoDataResult<BrevoContactAttribute>.Ok(attribute, result.StatusCode);
    }

    /// <summary>
    /// Creates a contact attribute. The category and type determine which options are accepted.
    /// </summary>
    public Task<BrevoResult> CreateContactAttributeAsync(
        string category,
        string name,
        BrevoContactAttributeWriteOptions options,
        CancellationToken cancellationToken = default)
    {
        string normalizedCategory = NormalizeAttributeCategory(category);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(options);
        ValidateAttributeOptions(normalizedCategory, options, isUpdate: false);

        return SendAsync(
            HttpMethod.Post,
            BuildContactAttributePath(normalizedCategory, name),
            CreateAttributeRequestBody(options),
            cancellationToken);
    }

    /// <summary>
    /// Updates an attribute's supported options or formula. Brevo does not allow changing its type here.
    /// </summary>
    public Task<BrevoResult> UpdateContactAttributeAsync(
        string category,
        string name,
        BrevoContactAttributeWriteOptions options,
        CancellationToken cancellationToken = default)
    {
        string normalizedCategory = NormalizeAttributeCategory(category);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(options);
        ValidateAttributeOptions(normalizedCategory, options, isUpdate: true);

        return SendAsync(
            HttpMethod.Put,
            BuildContactAttributePath(normalizedCategory, name),
            CreateAttributeRequestBody(options),
            cancellationToken);
    }

    /// <summary>
    /// Permanently deletes a contact attribute and its values from contacts.
    /// </summary>
    public Task<BrevoResult> DeleteContactAttributeAsync(
        string category,
        string name,
        CancellationToken cancellationToken = default)
    {
        string normalizedCategory = NormalizeAttributeCategory(category);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return SendAsync(
            HttpMethod.Delete,
            BuildContactAttributePath(normalizedCategory, name),
            null,
            cancellationToken);
    }

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

    private static IReadOnlyList<BrevoContactAttribute> ReadContactAttributes(JsonElement document)
    {
        if (!document.TryGetProperty("attributes", out JsonElement attributesElement)
            || attributesElement.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException("Brevo response did not contain a contact attributes array.");
        }

        List<BrevoContactAttribute> attributes = new();
        foreach (JsonElement item in attributesElement.EnumerateArray())
        {
            string? name = ReadOptionalString(item, "name");
            string? category = ReadOptionalString(item, "category");
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(category))
            {
                throw new JsonException("Brevo contact attribute is missing its name or category.");
            }

            attributes.Add(new BrevoContactAttribute(
                name,
                category,
                ReadOptionalString(item, "type"),
                ReadAttributeEnumeration(item),
                ReadAttributeOptions(item),
                item.TryGetProperty("isRecurring", out JsonElement recurringElement)
                    && recurringElement.ValueKind is JsonValueKind.True or JsonValueKind.False
                        ? recurringElement.GetBoolean()
                        : null,
                ReadOptionalString(item, "value"),
                ReadOptionalString(item, "calculatedValue")));
        }

        return attributes;
    }

    private static IReadOnlyList<BrevoContactAttributeOption>? ReadAttributeEnumeration(JsonElement attribute)
    {
        if (!attribute.TryGetProperty("enumeration", out JsonElement values)
            || values.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (values.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException("Brevo contact attribute enumeration was not an array.");
        }

        List<BrevoContactAttributeOption> options = new();
        foreach (JsonElement item in values.EnumerateArray())
        {
            if (!item.TryGetProperty("label", out JsonElement labelElement)
                || labelElement.ValueKind != JsonValueKind.String)
            {
                throw new JsonException("Brevo contact attribute enumeration contains an invalid option.");
            }

            long? value = null;
            string? valueStr = ReadOptionalString(item, "valueStr");
            if (item.TryGetProperty("value", out JsonElement valueElement))
            {
                if (valueElement.ValueKind == JsonValueKind.Number && valueElement.TryGetInt64(out long numericValue))
                {
                    value = numericValue;
                }
                else if (valueElement.ValueKind == JsonValueKind.String)
                {
                    valueStr ??= valueElement.GetString();
                }
                else
                {
                    valueStr ??= valueElement.GetRawText();
                }
            }

            if (!value.HasValue && string.IsNullOrWhiteSpace(valueStr))
            {
                throw new JsonException("Brevo contact attribute enumeration option did not include a value.");
            }

            options.Add(new BrevoContactAttributeOption(value, valueStr, labelElement.GetString() ?? string.Empty));
        }

        return options;
    }

    private static IReadOnlyList<string>? ReadAttributeOptions(JsonElement attribute)
    {
        if (!attribute.TryGetProperty("multiCategoryOptions", out JsonElement values)
            || values.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (values.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException("Brevo multiple-choice options were not an array.");
        }

        List<string> options = new();
        foreach (JsonElement item in values.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String)
            {
                throw new JsonException("Brevo multiple-choice option was not a string.");
            }

            options.Add(item.GetString() ?? string.Empty);
        }

        return options;
    }

    private static string? ReadOptionalString(JsonElement document, string propertyName)
        => document.TryGetProperty(propertyName, out JsonElement element)
           && element.ValueKind == JsonValueKind.String
            ? element.GetString()
            : null;

    private static string NormalizeAttributeCategory(string category)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        string normalized = category.Trim().ToLowerInvariant();
        if (normalized is not ("normal" or "transactional" or "category" or "calculated" or "global"))
        {
            throw new ArgumentException("Unsupported Brevo contact attribute category.", nameof(category));
        }

        return normalized;
    }

    private static string BuildContactAttributePath(string category, string name)
        => $"contacts/attributes/{category}/{Uri.EscapeDataString(name.Trim().ToUpperInvariant())}";

    private static Dictionary<string, object?> CreateAttributeRequestBody(
        BrevoContactAttributeWriteOptions options)
    {
        Dictionary<string, object?> body = new();
        if (options.Type is not null)
        {
            body["type"] = options.Type;
        }

        if (options.Enumeration is not null)
        {
            body["enumeration"] = options.Enumeration;
        }

        if (options.IsRecurring.HasValue)
        {
            body["isRecurring"] = options.IsRecurring.Value;
        }

        if (options.MultiCategoryOptions is not null)
        {
            body["multiCategoryOptions"] = options.MultiCategoryOptions;
        }

        if (options.Value is not null)
        {
            body["value"] = options.Value;
        }

        return body;
    }

    private static void ValidateAttributeOptions(
        string category,
        BrevoContactAttributeWriteOptions options,
        bool isUpdate)
    {
        if (isUpdate && options.Type is not null)
        {
            throw new ArgumentException("Brevo does not support changing an attribute's type.");
        }

        if (isUpdate && options.IsRecurring.HasValue)
        {
            throw new ArgumentException("Brevo does not support changing the recurring flag through the update endpoint.");
        }

        if (options.MultiCategoryOptions is not null
            && (category != "normal" || (options.Type is not null && options.Type != "multiple-choice")))
        {
            throw new ArgumentException("Multiple-choice options are only valid for normal multiple-choice attributes.");
        }

        if (options.Enumeration is not null && category != "category")
        {
            throw new ArgumentException("Enumeration options are only valid for category attributes.");
        }

        if ((options.Value is not null || options.IsRecurring.HasValue)
            && category is not ("calculated" or "global"))
        {
            throw new ArgumentException("Value and recurring options are only valid for calculated or global attributes.");
        }

        if (!isUpdate)
        {
            if (category is "normal" or "transactional" or "category"
                && string.IsNullOrWhiteSpace(options.Type))
            {
                throw new ArgumentException("A type is required for this contact attribute category.");
            }

            if (options.Type == "multiple-choice" && options.MultiCategoryOptions is not { Count: > 0 })
            {
                throw new ArgumentException("A multiple-choice attribute requires at least one option.");
            }

            if (category is "calculated" or "global" && options.Value is null)
            {
                throw new ArgumentException("A value is required for calculated or global attributes.");
            }
        }
        else if (options.MultiCategoryOptions is null
                 && options.Enumeration is null
                 && options.Value is null)
        {
            throw new ArgumentException("Provide supported attribute options to update.");
        }
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

    private static Dictionary<string, object?> CreateContactRequestBody(
        string email,
        IReadOnlyDictionary<string, object?>? attributes,
        IReadOnlyList<long>? listIds)
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

        return body;
    }

    private static Dictionary<string, object?> CreateContactRequestBodyWithoutEmail(
        IReadOnlyDictionary<string, object?> attributes,
        IReadOnlyList<long>? listIds)
    {
        var body = new Dictionary<string, object?>
        {
            ["updateEnabled"] = true,
            ["attributes"] = attributes
        };

        if (listIds is { Count: > 0 })
        {
            body["listIds"] = listIds;
        }

        return body;
    }

    private static IReadOnlyDictionary<string, object?>? ConvertAttributes(
        IReadOnlyDictionary<string, string>? attributes)
    {
        if (attributes is null)
        {
            return null;
        }

        Dictionary<string, object?> converted = new(StringComparer.Ordinal);
        foreach ((string name, string value) in attributes)
        {
            converted[name] = value;
        }

        return converted;
    }

    private static async Task<long?> ReadOptionalContactIdAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        using JsonDocument document = JsonDocument.Parse(content);
        return document.RootElement.TryGetProperty("id", out JsonElement idElement)
            && idElement.TryGetInt64(out long contactId)
            && contactId > 0
                ? contactId
                : null;
    }

    private static long ReadContactId(JsonElement document)
    {
        if (document.TryGetProperty("id", out JsonElement idElement)
            && idElement.TryGetInt64(out long contactId)
            && contactId > 0)
        {
            return contactId;
        }

        throw new JsonException("Brevo response did not include a valid contact ID.");
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

    private async Task<BrevoResult> SendAsync(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
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
        string? apiKey = await _apiKeyProvider.GetApiKeyAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return BrevoDataResult<T>.Fail(0, "Brevo API key is not configured.");
        }

        using HttpRequestMessage request = new(method, path);
        request.Headers.TryAddWithoutValidation("api-key", apiKey);
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
