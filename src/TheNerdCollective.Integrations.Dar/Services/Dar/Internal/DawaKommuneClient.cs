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
/// DAWA kommune-opslag (gratis, ingen API-nøgle) — fallback når Datafordeler DAGI GraphQL er tom.
/// <see href="https://dawadocs.dataforsyningen.dk/dok/api/kommune"/>
/// </summary>
internal sealed class DawaKommuneClient
{
    private const int PageSize = 100;

    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public DawaKommuneClient(HttpClient httpClient, DarDagiOptions options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _baseUrl = (options.DawaBaseUrl ?? DarDagiOptions.DefaultDawaBaseUrl).TrimEnd('/');
    }

    internal async Task<IReadOnlyList<KommuneDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = new List<KommuneDto>();
        var side = 1;

        while (true)
        {
            var url =
                $"{_baseUrl}/kommuner?per_side={PageSize}&side={side}";

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
                .Select(MapKommune)
                .Where(k => k is not null)
                .Cast<KommuneDto>()
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
            .GroupBy(k => k.Kommunekode ?? k.IdLokalId ?? k.Navn ?? string.Empty, StringComparer.Ordinal)
            .Select(g => g.First())
            .OrderBy(k => k.Navn, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    internal Task<KommuneDto?> FindByWgs84Async(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default) =>
        ReverseAsync(
            longitude.ToString(CultureInfo.InvariantCulture),
            latitude.ToString(CultureInfo.InvariantCulture),
            srid: null,
            cancellationToken);

    internal Task<KommuneDto?> FindByEtrs89Async(
        double easting,
        double northing,
        CancellationToken cancellationToken = default) =>
        ReverseAsync(
            easting.ToString(CultureInfo.InvariantCulture),
            northing.ToString(CultureInfo.InvariantCulture),
            srid: "25832",
            cancellationToken);

    private async Task<KommuneDto?> ReverseAsync(
        string x,
        string y,
        string? srid,
        CancellationToken cancellationToken)
    {
        var url = $"{_baseUrl}/kommuner/reverse?x={Uri.EscapeDataString(x)}&y={Uri.EscapeDataString(y)}";
        if (!string.IsNullOrEmpty(srid))
        {
            url += $"&srid={Uri.EscapeDataString(srid)}";
        }

        using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response).ConfigureAwait(false);

        using var document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync().ConfigureAwait(false),
            cancellationToken: cancellationToken).ConfigureAwait(false);

        if (document.RootElement.TryGetProperty("type", out var type)
            && type.GetString()?.EndsWith("Error", StringComparison.Ordinal) == true)
        {
            return null;
        }

        return MapKommune(document.RootElement);
    }

    private static KommuneDto? MapKommune(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var navn = ReadString(element, "navn");
        var kode = ReadString(element, "kode");
        if (string.IsNullOrWhiteSpace(navn) && string.IsNullOrWhiteSpace(kode))
        {
            return null;
        }

        return new KommuneDto
        {
            IdLokalId = ReadString(element, "dagi_id"),
            Navn = navn,
            Kommunekode = kode
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

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        throw new InvalidOperationException(
            $"DAWA returnerede HTTP {(int)response.StatusCode}: {body}");
    }
}
