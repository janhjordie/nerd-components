using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TheNerdCollective.Integrations.Dar.Configuration;
using TheNerdCollective.Integrations.Dar.Models;

namespace TheNerdCollective.Integrations.Dar.Services.Dar.Internal;

/// <summary>
/// DAWA region-opslag (gratis, ingen API-nøgle) — fallback når Datafordeler DAGI GraphQL er tom.
/// <see href="https://dawadocs.dataforsyningen.dk/dok/api/region"/>
/// </summary>
internal sealed class DawaRegionClient
{
    private const int PageSize = 100;

    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public DawaRegionClient(HttpClient httpClient, DarDagiOptions options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _baseUrl = (options.DawaBaseUrl ?? DarDagiOptions.DefaultDawaBaseUrl).TrimEnd('/');
    }

    internal async Task<IReadOnlyList<RegionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = new List<RegionDto>();
        var side = 1;

        while (true)
        {
            var url = $"{_baseUrl}/regioner?per_side={PageSize}&side={side}";
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
                .Select(MapRegion)
                .Where(r => r is not null)
                .Cast<RegionDto>()
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
            .GroupBy(r => r.Regionskode ?? r.IdLokalId ?? r.Regionnavn ?? string.Empty, StringComparer.Ordinal)
            .Select(g => g.First())
            .OrderBy(r => r.Regionnavn, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private static RegionDto? MapRegion(JsonElement element)
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

        return new RegionDto
        {
            IdLokalId = ReadString(element, "dagi_id"),
            Regionnavn = navn,
            Regionskode = kode
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
