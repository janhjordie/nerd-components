using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TheNerdCollective.Integrations.Dar.Configuration;
using TheNerdCollective.Integrations.Dar.GraphQL;
using TheNerdCollective.Integrations.Dar.Json;
using TheNerdCollective.Integrations.Dar.Models;
using TheNerdCollective.Integrations.Dar.Services.Internal;

namespace TheNerdCollective.Integrations.Dar.Services.Dar.Internal;

/// <summary>
/// DAR REST/GraphQL husnummer-opslag til postnummer med alle tilknyttede kommuner (Datafordeler-only).
/// </summary>
internal sealed class PostnummerKommuneRestResolver
{
    private const int PageSize = 100;

    private readonly GraphQlDataAccessor _accessor;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly DarPostnummerOptions _options;

    public PostnummerKommuneRestResolver(
        GraphQlDataAccessor accessor,
        HttpClient httpClient,
        string apiKey,
        DarPostnummerOptions options)
    {
        _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    internal async Task<PostnummerMedKommunerDto?> ResolveAllKommunerAsync(
        string postnummer,
        string? postdistrikt,
        CancellationToken cancellationToken = default)
    {
        var kommuner = new Dictionary<string, KommuneRefDto>(StringComparer.Ordinal);
        var resolvedPostdistrikt = postdistrikt ?? string.Empty;
        var page = 1;

        while (true)
        {
            var url = BuildUrl(
                $"postnr={Uri.EscapeDataString(postnummer)}",
                page);

            var husnumre = await FetchPageAsync(url, cancellationToken).ConfigureAwait(false);
            if (husnumre.Count == 0)
            {
                break;
            }

            foreach (var husnummer in husnumre)
            {
                if (string.IsNullOrWhiteSpace(resolvedPostdistrikt)
                    && husnummer.TryGetProperty("adgangsadressebetegnelse", out var betegnelse))
                {
                    resolvedPostdistrikt = ExtractPostdistrikt(betegnelse.GetString(), postnummer);
                }

                AddKommune(kommuner, husnummer);
            }

            if (husnumre.Count < PageSize)
            {
                break;
            }

            page++;
        }

        if (kommuner.Count == 0)
        {
            return null;
        }

        return new PostnummerMedKommunerDto
        {
            Postnummer = postnummer,
            Postdistrikt = resolvedPostdistrikt,
            Kommuner = SortKommuner(kommuner.Values)
        };
    }

    internal async Task<IReadOnlyList<PostnummerMedKommunerDto>> GetByKommunekodeAsync(
        string kommunekode,
        CancellationToken cancellationToken = default)
    {
        var graphQlResults = await TryGetByKommunekodeViaGraphQlAsync(kommunekode, cancellationToken)
            .ConfigureAwait(false);
        if (graphQlResults.Count > 0)
        {
            return graphQlResults;
        }

        return await GetByKommunekodeViaRestAsync(kommunekode, cancellationToken).ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<PostnummerMedKommunerDto>> TryGetByKommunekodeViaGraphQlAsync(
        string kommunekode,
        CancellationToken cancellationToken)
    {
        var kommuneId = await ResolveKommuneIdViaRestAsync(kommunekode, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(kommuneId))
        {
            return Array.Empty<PostnummerMedKommunerDto>();
        }

        var postnumre = new HashSet<string>(StringComparer.Ordinal);
        var temporal = GraphQlDataAccessor.CreateTemporalVariables();
        var nodes = await _accessor.FetchAllDarNodesAsync(
            GraphQlQueries.GetHusnumreByKommuneId,
            after => new HusnumreByKommuneVariables(kommuneId, temporal.Virkningstid, temporal.Registreringstid, after),
            "DAR_Husnummer",
            cancellationToken).ConfigureAwait(false);

        foreach (var node in nodes.EnumerateArray())
        {
            var postnr = ReadGraphQlPostnummer(node);
            if (!string.IsNullOrWhiteSpace(postnr))
            {
                postnumre.Add(postnr);
            }
        }

        if (postnumre.Count == 0)
        {
            return Array.Empty<PostnummerMedKommunerDto>();
        }

        var results = new List<PostnummerMedKommunerDto>(postnumre.Count);
        foreach (var postnummer in postnumre.OrderBy(p => p, StringComparer.Ordinal))
        {
            var enriched = await ResolveAllKommunerAsync(postnummer, null, cancellationToken).ConfigureAwait(false);
            if (enriched is not null)
            {
                results.Add(enriched);
            }
        }

        return results;
    }

    internal async Task<IReadOnlyList<string>> GetPostnumreByKommunekodeAsync(
        string kommunekode,
        CancellationToken cancellationToken = default)
    {
        var kommuneId = await ResolveKommuneIdViaRestAsync(kommunekode, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(kommuneId))
        {
            return Array.Empty<string>();
        }

        var postnumre = new HashSet<string>(StringComparer.Ordinal);
        var temporal = GraphQlDataAccessor.CreateTemporalVariables();
        var nodes = await _accessor.FetchAllDarNodesAsync(
            GraphQlQueries.GetHusnumreByKommuneId,
            after => new HusnumreByKommuneVariables(kommuneId, temporal.Virkningstid, temporal.Registreringstid, after),
            "DAR_Husnummer",
            cancellationToken).ConfigureAwait(false);

        foreach (var node in nodes.EnumerateArray())
        {
            var postnr = ReadGraphQlPostnummer(node);
            if (!string.IsNullOrWhiteSpace(postnr))
            {
                postnumre.Add(postnr);
            }
        }

        return postnumre.OrderBy(p => p, StringComparer.Ordinal).ToList();
    }

    private async Task<string?> ResolveKommuneIdViaRestAsync(
        string kommunekode,
        CancellationToken cancellationToken)
    {
        var url = BuildUrl($"kommunekode={Uri.EscapeDataString(kommunekode)}", 1);
        var husnumre = await FetchPageAsync(url, cancellationToken).ConfigureAwait(false);
        if (husnumre.Count == 0)
        {
            return null;
        }

        if (!husnumre[0].TryGetProperty("kommuneinddeling", out var kommune)
            || kommune.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return ReadString(kommune, "id");
    }

    private static string? ReadGraphQlPostnummer(JsonElement node)
    {
        if (node.TryGetProperty("adgangsadressebetegnelse", out var betegnelse))
        {
            return ExtractPostalCode(betegnelse.GetString());
        }

        return null;
    }

    private async Task<IReadOnlyList<PostnummerMedKommunerDto>> GetByKommunekodeViaRestAsync(
        string kommunekode,
        CancellationToken cancellationToken)
    {
        var byPostnummer = new Dictionary<string, (string Postdistrikt, Dictionary<string, KommuneRefDto> Kommuner)>(
            StringComparer.Ordinal);
        var page = 1;

        while (true)
        {
            var url = BuildUrl(
                $"kommunekode={Uri.EscapeDataString(kommunekode)}",
                page);

            var husnumre = await FetchPageAsync(url, cancellationToken).ConfigureAwait(false);
            if (husnumre.Count == 0)
            {
                break;
            }

            foreach (var husnummer in husnumre)
            {
                var postnr = ReadPostnummer(husnummer);
                if (string.IsNullOrWhiteSpace(postnr))
                {
                    continue;
                }

                if (!byPostnummer.TryGetValue(postnr, out var entry))
                {
                    entry = (string.Empty, new Dictionary<string, KommuneRefDto>(StringComparer.Ordinal));
                    byPostnummer[postnr] = entry;
                }

                var postdistrikt = entry.Postdistrikt;
                if (string.IsNullOrWhiteSpace(postdistrikt)
                    && husnummer.TryGetProperty("adgangsadressebetegnelse", out var betegnelse))
                {
                    postdistrikt = ExtractPostdistrikt(betegnelse.GetString(), postnr);
                }

                AddKommune(entry.Kommuner, husnummer);
                byPostnummer[postnr] = (postdistrikt, entry.Kommuner);
            }

            if (husnumre.Count < PageSize)
            {
                break;
            }

            page++;
        }

        return byPostnummer
            .Select(pair => new PostnummerMedKommunerDto
            {
                Postnummer = pair.Key,
                Postdistrikt = pair.Value.Postdistrikt,
                Kommuner = SortKommuner(pair.Value.Kommuner.Values)
            })
            .OrderBy(p => p.Postnummer, StringComparer.Ordinal)
            .ToList();
    }

    private string BuildUrl(string filter, int page)
    {
        var baseUrl = (_options.RestUrl ?? DarPostnummerOptions.DefaultRestUrl).TrimEnd('?');
        return
            $"{baseUrl}/husnummer?Format=JSON&Status=3&{filter}" +
            $"&MedDybde=true&pagesize={PageSize}&page={page}" +
            $"&username={Uri.EscapeDataString(_apiKey)}&password={Uri.EscapeDataString(_apiKey)}";
    }

    private async Task<IReadOnlyList<JsonElement>> FetchPageAsync(string url, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return Array.Empty<JsonElement>();
            }

            throw new InvalidOperationException(
                $"DAR REST husnummer returnerede HTTP {(int)response.StatusCode}: {body}");
        }

        using var document = JsonDocument.Parse(body);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<JsonElement>();
        }

        var items = new List<JsonElement>();
        foreach (var item in document.RootElement.EnumerateArray())
        {
            items.Add(item.Clone());
        }

        return items;
    }

    private static void AddKommune(Dictionary<string, KommuneRefDto> kommuner, JsonElement husnummer)
    {
        if (!husnummer.TryGetProperty("kommuneinddeling", out var kommune)
            || kommune.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        var kode = ReadString(kommune, "kommunekode");
        var navn = ReadString(kommune, "navn");
        if (string.IsNullOrWhiteSpace(kode))
        {
            return;
        }

        var normalized = kode.PadLeft(4, '0');
        kommuner[normalized] = new KommuneRefDto
        {
            Kommunekode = normalized,
            Navn = navn ?? string.Empty
        };
    }

    private static string? ReadPostnummer(JsonElement husnummer)
    {
        if (husnummer.TryGetProperty("postnummer", out var postnummerRef))
        {
            if (postnummerRef.ValueKind == JsonValueKind.Object
                && postnummerRef.TryGetProperty("postnr", out var postnr))
            {
                return postnr.GetString()?.Trim();
            }

            if (postnummerRef.ValueKind == JsonValueKind.String)
            {
                var value = postnummerRef.GetString();
                if (value?.Length == 4 && int.TryParse(value, out _))
                {
                    return value;
                }
            }
        }

        if (husnummer.TryGetProperty("adgangsadressebetegnelse", out var betegnelse))
        {
            return ExtractPostalCode(betegnelse.GetString());
        }

        return null;
    }

    internal static string ExtractPostdistrikt(string? adgangsadresse, string postnummer)
    {
        if (string.IsNullOrWhiteSpace(adgangsadresse))
        {
            return string.Empty;
        }

        var marker = $", {postnummer} ";
        var index = adgangsadresse.IndexOf(marker, StringComparison.Ordinal);
        if (index >= 0)
        {
            return adgangsadresse.Substring(index + marker.Length).Trim();
        }

        var parts = adgangsadresse.Split(',');
        return parts.Length >= 2 ? parts[parts.Length - 1].Trim() : string.Empty;
    }

    internal static string? ExtractPostalCode(string? adgangsadresse)
    {
        if (string.IsNullOrWhiteSpace(adgangsadresse))
        {
            return null;
        }

        var parts = adgangsadresse.Split(',');
        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (trimmed.Length >= 4)
            {
                var digits = new string(trimmed.TakeWhile(c => char.IsDigit(c)).ToArray());
                if (digits.Length == 4)
                {
                    return digits;
                }
            }
        }

        return null;
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }

    private static IReadOnlyList<KommuneRefDto> SortKommuner(IEnumerable<KommuneRefDto> kommuner) =>
        kommuner
            .OrderBy(k => k.Kommunekode, StringComparer.Ordinal)
            .ToList();

    private sealed class HusnumreByKommuneVariables
    {
        public HusnumreByKommuneVariables(string kommuneId, string virkningstid, string registreringstid, string? after)
        {
            KommuneId = kommuneId;
            Virkningstid = virkningstid;
            Registreringstid = registreringstid;
            After = after;
        }

        public string KommuneId { get; }

        public string Virkningstid { get; }

        public string Registreringstid { get; }

        public string? After { get; }
    }
}
