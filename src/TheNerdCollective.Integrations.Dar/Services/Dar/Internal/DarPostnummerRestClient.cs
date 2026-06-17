using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TheNerdCollective.Integrations.Dar.Configuration;
using TheNerdCollective.Integrations.Dar.Models;

namespace TheNerdCollective.Integrations.Dar.Services.Dar.Internal;

/// <summary>
/// DAR REST husnummer-opslag til primær kommune når DAWA ikke er tilgængelig.
/// </summary>
internal sealed class DarPostnummerRestClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly DarPostnummerOptions _options;

    public DarPostnummerRestClient(HttpClient httpClient, string apiKey, DarPostnummerOptions options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    internal async Task<PostnummerMedKommuneDto?> ResolveKommuneAsync(
        string postnummer,
        string? postdistrikt,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = (_options.RestUrl ?? DarPostnummerOptions.DefaultRestUrl).TrimEnd('?');
        var url =
            $"{baseUrl}/husnummer?Format=JSON&Status=3&postnr={Uri.EscapeDataString(postnummer)}" +
            "&MedDybde=true&pagesize=1&page=1" +
            $"&username={Uri.EscapeDataString(_apiKey)}&password={Uri.EscapeDataString(_apiKey)}";

        using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            return null;
        }

        using var document = JsonDocument.Parse(body);
        if (document.RootElement.ValueKind != JsonValueKind.Array
            || document.RootElement.GetArrayLength() == 0)
        {
            return null;
        }

        var husnummer = document.RootElement[0];
        if (!husnummer.TryGetProperty("kommuneinddeling", out var kommune)
            || kommune.ValueKind != JsonValueKind.Object)
        {
            return new PostnummerMedKommuneDto
            {
                Postnummer = postnummer,
                Postdistrikt = postdistrikt ?? string.Empty
            };
        }

        return new PostnummerMedKommuneDto
        {
            Postnummer = postnummer,
            Postdistrikt = postdistrikt ?? string.Empty,
            Kommunekode = ReadString(kommune, "kommunekode"),
            Kommunenavn = ReadString(kommune, "navn")
        };
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }
}
