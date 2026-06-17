using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using TheNerdCollective.Integrations.Dar.Configuration;
using TheNerdCollective.Integrations.Dar.GraphQL;
using TheNerdCollective.Integrations.Dar.Json;
using TheNerdCollective.Integrations.Dar.Models;
using TheNerdCollective.Integrations.Dar.Services.Dar.Internal;
using TheNerdCollective.Integrations.Dar.Services.Internal;

namespace TheNerdCollective.Integrations.Dar.Services.Dar;

/// <summary>Aktive DAR-postnumre via GraphQL med DAWA/REST til kommune-metadata.</summary>
public sealed class DarPostnummerService
{
    private const string ActiveStatus = "3";

    private readonly GraphQlDataAccessor _accessor;
    private readonly DawaPostnummerClient _dawaClient;
    private readonly DarPostnummerRestClient _restClient;
    private readonly DarPostnummerOptions _options;

    public DarPostnummerService(
        GraphQlDataAccessor accessor,
        HttpClient httpClient,
        string apiKey,
        DarPostnummerOptions options)
    {
        _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _dawaClient = new DawaPostnummerClient(httpClient, options);
        _restClient = new DarPostnummerRestClient(httpClient, apiKey, options);
    }

    /// <summary>Returnerer alle aktive postnumre (DAR status 3) sorteret efter postnummer.</summary>
    public async Task<IReadOnlyList<PostnummerDto>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        if (PostnummerCache.TryGetActive(_options.CacheDuration, out var cached))
        {
            return cached;
        }

        var temporal = GraphQlDataAccessor.CreateTemporalVariables();
        var nodes = await _accessor.FetchAllDarNodesAsync(
            GraphQlQueries.GetAllPostnumre,
            after => new PostnummerListVariables(temporal.Virkningstid, temporal.Registreringstid, after),
            "DAR_Postnummer",
            cancellationToken).ConfigureAwait(false);

        var postnumre = DarJsonSerializer.DeserializeList<PostnummerGraphDto>(nodes)
            .Where(p => string.Equals(p.Status, ActiveStatus, StringComparison.Ordinal))
            .Select(MapBasic)
            .Where(p => !string.IsNullOrWhiteSpace(p.Postnummer))
            .GroupBy(p => p.Postnummer, StringComparer.Ordinal)
            .Select(g => g.First())
            .OrderBy(p => p.Postnummer, StringComparer.Ordinal)
            .ToList();

        if (postnumre.Count == 0)
        {
            throw new InvalidOperationException(
                "Ingen aktive postnumre returneret fra DAR GraphQL (DAR_Postnummer).");
        }

        PostnummerCache.SetActive(postnumre, _options.CacheDuration);
        return postnumre;
    }

    /// <summary>Postnumre hvis primære kommune matcher kommunekode (fire cifre, fx "0101").</summary>
    public async Task<IReadOnlyList<PostnummerDto>> GetByMunicipalityCodeAsync(
        string kommunekode,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = NormalizeKommunekode(kommunekode);

        if (_options.EnableDawaEnrichment)
        {
            var dawaResults = await _dawaClient.GetByMunicipalityCodeAsync(normalizedCode, cancellationToken)
                .ConfigureAwait(false);
            if (dawaResults.Count > 0)
            {
                return dawaResults
                    .Select(ToBasic)
                    .OrderBy(p => p.Postnummer, StringComparer.Ordinal)
                    .ToList();
            }
        }

        var enriched = await ResolveAllWithKommuneAsync(cancellationToken).ConfigureAwait(false);
        return enriched
            .Where(p => string.Equals(p.Kommunekode, normalizedCode, StringComparison.Ordinal))
            .Select(ToBasic)
            .OrderBy(p => p.Postnummer, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>Resolver et eller flere postnumre inkl. primær kommune.</summary>
    public async Task<IReadOnlyList<PostnummerMedKommuneDto>> GetByPostalCodesAsync(
        IEnumerable<string> postalCodes,
        CancellationToken cancellationToken = default)
    {
        if (postalCodes is null)
        {
            throw new ArgumentNullException(nameof(postalCodes));
        }

        var normalizedCodes = NormalizePostalCodes(postalCodes);
        if (normalizedCodes.Count == 0)
        {
            throw new ArgumentException("Mindst ét postnummer skal angives.", nameof(postalCodes));
        }

        var activeByCode = await BuildActiveLookupAsync(cancellationToken).ConfigureAwait(false);
        var results = new List<PostnummerMedKommuneDto>(normalizedCodes.Count);

        foreach (var code in normalizedCodes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!activeByCode.TryGetValue(code, out var basic))
            {
                basic = await TryLookupInactivePostalCodeAsync(code, cancellationToken).ConfigureAwait(false);
                if (basic is null)
                {
                    continue;
                }
            }

            var enriched = await ResolveKommuneAsync(basic, cancellationToken).ConfigureAwait(false);
            if (enriched is not null)
            {
                results.Add(enriched);
            }
        }

        return results;
    }

    private async Task<Dictionary<string, PostnummerDto>> BuildActiveLookupAsync(CancellationToken cancellationToken)
    {
        var all = await GetAllActiveAsync(cancellationToken).ConfigureAwait(false);
        return all.ToDictionary(p => p.Postnummer, StringComparer.Ordinal);
    }

    private async Task<PostnummerDto?> TryLookupInactivePostalCodeAsync(
        string postnummer,
        CancellationToken cancellationToken)
    {
        var temporal = GraphQlDataAccessor.CreateTemporalVariables();
        var nodes = await _accessor.FetchDarNodesAsync(
            GraphQlQueries.GetPostnummerByCode,
            new PostnummerByCodeVariables(postnummer, temporal.Virkningstid, temporal.Registreringstid),
            "DAR_Postnummer",
            cancellationToken).ConfigureAwait(false);

        if (nodes.GetArrayLength() == 0)
        {
            return null;
        }

        var dto = DarJsonSerializer.DeserializeRequired<PostnummerGraphDto>(nodes[0]);
        return MapBasic(dto);
    }

    private async Task<PostnummerMedKommuneDto?> ResolveKommuneAsync(
        PostnummerDto basic,
        CancellationToken cancellationToken)
    {
        if (_options.EnableDawaEnrichment)
        {
            var dawa = await _dawaClient.GetByPostalCodeAsync(basic.Postnummer, cancellationToken)
                .ConfigureAwait(false);
            if (dawa is not null)
            {
                return dawa with
                {
                    Postdistrikt = string.IsNullOrWhiteSpace(dawa.Postdistrikt)
                        ? basic.Postdistrikt
                        : dawa.Postdistrikt
                };
            }
        }

        var rest = await _restClient.ResolveKommuneAsync(
            basic.Postnummer,
            basic.Postdistrikt,
            cancellationToken).ConfigureAwait(false);

        return rest ?? new PostnummerMedKommuneDto
        {
            Postnummer = basic.Postnummer,
            Postdistrikt = basic.Postdistrikt
        };
    }

    private async Task<IReadOnlyList<PostnummerMedKommuneDto>> ResolveAllWithKommuneAsync(
        CancellationToken cancellationToken)
    {
        var all = await GetAllActiveAsync(cancellationToken).ConfigureAwait(false);
        var results = new List<PostnummerMedKommuneDto>(all.Count);

        foreach (var batch in Batch(all, Math.Max(1, _options.MaxParallelKommuneLookups)))
        {
            var tasks = batch
                .Select(p => ResolveKommuneAsync(p, cancellationToken))
                .ToArray();

            var resolved = await Task.WhenAll(tasks).ConfigureAwait(false);
            foreach (var item in resolved)
            {
                if (item is not null)
                {
                    results.Add(item);
                }
            }
        }

        return results;
    }

    private static PostnummerDto MapBasic(PostnummerGraphDto dto) =>
        new()
        {
            Postnummer = dto.Postnr?.Trim() ?? string.Empty,
            Postdistrikt = dto.Navn?.Trim() ?? string.Empty
        };

    private static PostnummerDto ToBasic(PostnummerMedKommuneDto dto) =>
        new()
        {
            Postnummer = dto.Postnummer,
            Postdistrikt = dto.Postdistrikt
        };

    private static List<string> NormalizePostalCodes(IEnumerable<string> postalCodes)
    {
        var result = new List<string>();

        foreach (var raw in postalCodes)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                continue;
            }

            foreach (var part in raw.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var normalized = NormalizePostalCode(part.Trim());
                if (normalized is not null && !result.Contains(normalized, StringComparer.Ordinal))
                {
                    result.Add(normalized);
                }
            }
        }

        return result;
    }

    private static string? NormalizePostalCode(string value)
    {
        var digits = new string(value.Where(char.IsDigit).ToArray());
        return digits.Length == 4 ? digits : null;
    }

    private static string NormalizeKommunekode(string kommunekode)
    {
        if (string.IsNullOrWhiteSpace(kommunekode))
        {
            throw new ArgumentException("Kommunekode må ikke være tom.", nameof(kommunekode));
        }

        var digits = new string(kommunekode.Where(char.IsDigit).ToArray());
        if (digits.Length == 0)
        {
            throw new ArgumentException("Kommunekode skal være numerisk.", nameof(kommunekode));
        }

        return digits.PadLeft(4, '0');
    }

    private static IEnumerable<IReadOnlyList<T>> Batch<T>(IReadOnlyList<T> source, int size)
    {
        for (var index = 0; index < source.Count; index += size)
        {
            var count = Math.Min(size, source.Count - index);
            var batch = new List<T>(count);
            for (var offset = 0; offset < count; offset++)
            {
                batch.Add(source[index + offset]);
            }

            yield return batch;
        }
    }

    private sealed class PostnummerGraphDto
    {
        [JsonPropertyName("postnr")]
        public string? Postnr { get; init; }

        [JsonPropertyName("navn")]
        public string? Navn { get; init; }

        [JsonPropertyName("status")]
        public string? Status { get; init; }
    }

    private sealed class PostnummerListVariables
    {
        public PostnummerListVariables(string virkningstid, string registreringstid, string? after)
        {
            Virkningstid = virkningstid;
            Registreringstid = registreringstid;
            After = after;
        }

        public string Virkningstid { get; }

        public string Registreringstid { get; }

        public string? After { get; }
    }

    private sealed class PostnummerByCodeVariables
    {
        public PostnummerByCodeVariables(string postnr, string virkningstid, string registreringstid)
        {
            Postnr = postnr;
            Virkningstid = virkningstid;
            Registreringstid = registreringstid;
        }

        public string Postnr { get; }

        public string Virkningstid { get; }

        public string Registreringstid { get; }
    }
}
