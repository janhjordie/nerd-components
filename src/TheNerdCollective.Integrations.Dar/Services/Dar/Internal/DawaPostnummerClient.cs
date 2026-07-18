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
        var items = await FetchPagedPostnumreAsync(
            $"kommunekode={Uri.EscapeDataString(kommunekode)}",
            cancellationToken).ConfigureAwait(false);

        return items
            .Select(MapPostnummer)
            .Where(p => p is not null)
            .Cast<PostnummerMedKommuneDto>()
            .OrderBy(p => p.Postnummer, StringComparer.Ordinal)
            .ToList();
    }

    internal async Task<IReadOnlyList<PostnummerMedKommunerDto>> GetByMunicipalityCodeWithAllKommunerAsync(
        string kommunekode,
        CancellationToken cancellationToken = default)
    {
        var items = await FetchPagedPostnumreAsync(
            $"kommunekode={Uri.EscapeDataString(kommunekode)}",
            cancellationToken).ConfigureAwait(false);

        return items
            .Select(MapPostnummerMedKommuner)
            .Where(p => p is not null)
            .Cast<PostnummerMedKommunerDto>()
            .OrderBy(p => p.Postnummer, StringComparer.Ordinal)
            .ToList();
    }

    internal async Task<IReadOnlyList<PostnummerMedKommunerDto>> GetByCircleAsync(
        double longitude,
        double latitude,
        int radiusMeters,
        CancellationToken cancellationToken = default)
    {
        var cirkel = string.Join(
            ",",
            longitude.ToString(CultureInfo.InvariantCulture),
            latitude.ToString(CultureInfo.InvariantCulture),
            radiusMeters.ToString(CultureInfo.InvariantCulture));

        var items = await FetchPagedPostnumreAsync(
            $"cirkel={Uri.EscapeDataString(cirkel)}",
            cancellationToken).ConfigureAwait(false);

        return items
            .Select(MapPostnummerMedKommuner)
            .Where(p => p is not null)
            .Cast<PostnummerMedKommunerDto>()
            .OrderBy(p => p.Postnummer, StringComparer.Ordinal)
            .ToList();
    }

    internal async Task<PostnummerMedKommunerDto?> GetByPostalCodeWithAllKommunerAsync(
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

            return MapPostnummerMedKommuner(document.RootElement[0]);
        }

        if (document.RootElement.ValueKind == JsonValueKind.Object)
        {
            return MapPostnummerMedKommuner(document.RootElement);
        }

        return null;
    }

    private async Task<List<JsonElement>> FetchPagedPostnumreAsync(
        string query,
        CancellationToken cancellationToken)
    {
        var items = new List<JsonElement>();
        var side = 1;

        while (true)
        {
            var url = $"{_baseUrl}/postnumre?{query}&per_side={PageSize}&side={side}";

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
                .Select(element => element.Clone())
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

        return items;
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
            Kommunekode = kommunekode!,
            Kommunenavn = kommunenavn ?? string.Empty
        };
    }

    private static PostnummerMedKommunerDto? MapPostnummerMedKommuner(JsonElement element)
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

        var kommuner = MapKommuner(element.TryGetProperty("kommuner", out var kommunerElement) ? kommunerElement : default);
        if (kommuner.Count == 0)
        {
            var (kommunekode, kommunenavn) = SelectPrimaryKommune(postdistrikt, default);
            if (!string.IsNullOrWhiteSpace(kommunekode))
            {
                kommuner.Add(new KommuneRefDto
                {
            Kommunekode = kommunekode!,
                    Navn = kommunenavn ?? string.Empty
                });
            }
        }

        return new PostnummerMedKommunerDto
        {
            Postnummer = postnummer!,
            Postdistrikt = postdistrikt ?? string.Empty,
            Kommuner = kommuner
        };
    }

    private static List<KommuneRefDto> MapKommuner(JsonElement kommuner)
    {
        if (kommuner.ValueKind != JsonValueKind.Array || kommuner.GetArrayLength() == 0)
        {
            return new List<KommuneRefDto>();
        }

        return kommuner.EnumerateArray()
            .Select(k => new
            {
                Kode = ReadString(k, "kode"),
                Navn = ReadString(k, "navn")
            })
            .Where(k => !string.IsNullOrWhiteSpace(k.Kode))
            .Select(k => new KommuneRefDto
            {
                Kommunekode = k.Kode!,
                Navn = k.Navn ?? string.Empty
            })
            .OrderBy(k => k.Kommunekode, StringComparer.Ordinal)
            .ToList();
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
            if (!string.IsNullOrWhiteSpace(match.Kode))
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
