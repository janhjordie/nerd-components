using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TheNerdCollective.Integrations.Dar.Configuration;
using TheNerdCollective.Integrations.Dar.Models;

namespace TheNerdCollective.Integrations.Dar.Services.Dar.Internal;

/// <summary>
/// DAWA postnummer-opslag (gratis, ingen API-nøgle) til kommune-metadata.
/// <see href="https://dawadocs.dataforsyningen.dk/dok/api/postnummer"/>
/// </summary>
internal sealed class DawaPostnummerClient
{
    private const int PageSize = 100;

    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public DawaPostnummerClient(HttpClient httpClient, DarPostnummerOptions options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _baseUrl = (options.DawaBaseUrl ?? DarPostnummerOptions.DefaultDawaBaseUrl).TrimEnd('/');
    }

    internal async Task<PostnummerMedKommuneDto?> GetByPostalCodeAsync(
        string postnummer,
        CancellationToken cancellationToken = default)
    {
        var url = $"{_baseUrl}/postnumre?nr={Uri.EscapeDataString(postnummer)}";
        using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response).ConfigureAwait(false);
        using var document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync().ConfigureAwait(false),
            cancellationToken: cancellationToken).ConfigureAwait(false);

        if (document.RootElement.ValueKind == JsonValueKind.Array)
        {
            if (document.RootElement.GetArrayLength() == 0)
            {
                return null;
            }

            return MapPostnummer(document.RootElement[0]);
        }

        if (document.RootElement.ValueKind == JsonValueKind.Object)
        {
            return MapPostnummer(document.RootElement);
        }

        return null;
    }

    internal async Task<IReadOnlyList<PostnummerMedKommuneDto>> GetByMunicipalityCodeAsync(
        string kommunekode,
        CancellationToken cancellationToken = default)
    {
        var items = new List<PostnummerMedKommuneDto>();
        var side = 1;

        while (true)
        {
            var url =
                $"{_baseUrl}/postnumre?kommunekode={Uri.EscapeDataString(kommunekode)}" +
                $"&per_side={PageSize}&side={side}";

            using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
            await EnsureSuccessAsync(response).ConfigureAwait(false);

            using var document = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync().ConfigureAwait(false),
                cancellationToken: cancellationToken).ConfigureAwait(false);

            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                break;
            }

            var page = document.RootElement.EnumerateArray()
                .Select(MapPostnummer)
                .Where(p => p is not null)
                .Cast<PostnummerMedKommuneDto>()
                .ToList();

            if (page.Count == 0)
            {
                break;
            }

            items.AddRange(page);

            if (page.Count < PageSize)
            {
                break;
            }

            side++;
        }

        return items
            .OrderBy(p => p.Postnummer, StringComparer.Ordinal)
            .ToList();
    }

    private static PostnummerMedKommuneDto? MapPostnummer(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var postnummer = ReadString(element, "nr");
        var postdistrikt = ReadString(element, "navn");
        if (string.IsNullOrWhiteSpace(postnummer))
        {
            return null;
        }

        var (kommunekode, kommunenavn) = SelectPrimaryKommune(
            postdistrikt,
            element.TryGetProperty("kommuner", out var kommuner) ? kommuner : default);

        return new PostnummerMedKommuneDto
        {
            Postnummer = postnummer!,
            Postdistrikt = postdistrikt ?? string.Empty,
            Kommunekode = kommunekode,
            Kommunenavn = kommunenavn ?? string.Empty
        };
    }

    internal static (string? Kode, string? Navn) SelectPrimaryKommune(string? postdistrikt, JsonElement kommuner)
    {
        if (kommuner.ValueKind != JsonValueKind.Array || kommuner.GetArrayLength() == 0)
        {
            return (null, null);
        }

        var entries = kommuner.EnumerateArray()
            .Select(k => (Kode: ReadString(k, "kode"), Navn: ReadString(k, "navn")))
            .Where(k => !string.IsNullOrWhiteSpace(k.Kode))
            .ToList();

        if (entries.Count == 0)
        {
            return (null, null);
        }

        if (!string.IsNullOrWhiteSpace(postdistrikt))
        {
            var match = entries.FirstOrDefault(k =>
                string.Equals(k.Navn, postdistrikt, StringComparison.CurrentCultureIgnoreCase));
            if (match.Kode is not null)
            {
                return match;
            }
        }

        var last = entries[entries.Count - 1];
        return (last.Kode, last.Navn);
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        throw new InvalidOperationException(
            $"DAWA postnumre returnerede HTTP {(int)response.StatusCode}: {body}");
    }
}
