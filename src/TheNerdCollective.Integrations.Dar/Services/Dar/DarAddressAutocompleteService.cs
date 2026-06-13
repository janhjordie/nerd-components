using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TheNerdCollective.Integrations.Dar.Configuration;
using TheNerdCollective.Integrations.Dar.Models;

namespace TheNerdCollective.Integrations.Dar.Services.Dar
{
    /// <summary>
    /// Fri-tekst adresse-autocomplete via Adressevælger (Klimadatastyrelsen).
    /// </summary>
    public sealed class DarAddressAutocompleteService
    {
        private const int MaxResults = 10;
        private const int ApiFetchLimit = 20;
        private const int MinSearchLength = 2;
        private static readonly TimeSpan SearchCacheDuration = TimeSpan.FromMinutes(5);
        private static readonly CultureInfo DanishCulture = new("da-DK");
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
        private static readonly Regex AddressPattern = new(
            "^(?<line1>.+),\\s*(?<postal>\\d{4})\\s+(?<city>.+)$",
            RegexOptions.Compiled);
        private static readonly Regex AddressWithUnitPattern = new(
            "^(?<street>[^,]+),\\s*(?<unit>[^,]+),\\s*(?<postal>\\d{4})\\s+(?<city>.+)$",
            RegexOptions.Compiled);
        private static readonly Regex MultiSpacePattern = new("\\s+", RegexOptions.Compiled);
        private static readonly Regex UnitSearchPattern = new(
            @"\b\d+\s*(?:\.?\s*)?(?:sal|th|tv|mf|kl)\b|\b\d+(?:th|sal|tv)\b|\b(?:st|kl|mf)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly ConcurrentDictionary<string, CacheEntry> SearchCache = new();

        private readonly HttpClient _httpClient;
        private readonly AdressevaelgerOptions _options;

        public DarAddressAutocompleteService(AdressevaelgerOptions options, HttpClient httpClient)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<IReadOnlyList<DanishAddressAutocompleteResult>> SearchAsync(
            string searchText,
            CancellationToken cancellationToken = default)
        {
            var trimmedSearchText = searchText?.Trim();
            if (string.IsNullOrWhiteSpace(trimmedSearchText) || trimmedSearchText.Length < MinSearchLength)
            {
                return Array.Empty<DanishAddressAutocompleteResult>();
            }

            if (string.IsNullOrWhiteSpace(_options.Token))
            {
                return Array.Empty<DanishAddressAutocompleteResult>();
            }

            var normalizedSearchText = MultiSpacePattern.Replace(trimmedSearchText, " ");
            var cacheKey = $"adressevaelger:{normalizedSearchText.ToLower(DanishCulture)}";

            if (TryGetCached(cacheKey, out var cachedResults))
            {
                return cachedResults;
            }

            var baseUrl = string.IsNullOrWhiteSpace(_options.BaseUrl)
                ? "https://adressevaelger.dk"
                : _options.BaseUrl;

            var searchUnit = ShouldSearchUnits(normalizedSearchText);
            var unitHint = ExtractUnitHint(normalizedSearchText);

            var husnummerFunds = await FetchFundsAsync(
                baseUrl, _options.Token, "husnumre", normalizedSearchText, cancellationToken);

            List<AdressevaelgerFund> allFunds = husnummerFunds;
            if (searchUnit)
            {
                var adresseFunds = await FetchFundsAsync(
                    baseUrl, _options.Token, "adresser", normalizedSearchText, cancellationToken);
                if (adresseFunds.Count > 0)
                {
                    allFunds = adresseFunds.Concat(husnummerFunds).ToList();
                }
            }

            var results = BuildResults(allFunds, searchUnit, unitHint);

            if (results.Count > 0)
            {
                SetCached(cacheKey, results);
            }

            return results;
        }

        private async Task<List<AdressevaelgerFund>> FetchFundsAsync(
            string baseUrl,
            string token,
            string endpoint,
            string searchText,
            CancellationToken cancellationToken)
        {
            var requestUri = BuildSearchUri(baseUrl, endpoint, searchText, token);
            using var response = await _httpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
            var payload = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return new List<AdressevaelgerFund>();
            }

            var searchResponse = JsonSerializer.Deserialize<AdressevaelgerSearchResponse>(payload, JsonOptions);
            if (!string.Equals(searchResponse?.Status, "ok", StringComparison.OrdinalIgnoreCase))
            {
                return new List<AdressevaelgerFund>();
            }

            return searchResponse?.Results ?? new List<AdressevaelgerFund>();
        }

        private static IReadOnlyList<DanishAddressAutocompleteResult> BuildResults(
            IEnumerable<AdressevaelgerFund> funds,
            bool searchUnit,
            string? unitHint)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var results = new List<DanishAddressAutocompleteResult>();

            foreach (var fund in funds
                         .Where(IsAutocompleteResult)
                         .OrderBy(f => GetResultSortOrder(f.Type, searchUnit))
                         .ThenBy(f => UnitMatchRank(f, unitHint)))
            {
                var mapped = MapFund(fund);
                if (mapped == null || !seen.Add(mapped.LocalId))
                {
                    continue;
                }

                results.Add(mapped);
                if (results.Count >= MaxResults)
                {
                    break;
                }
            }

            return results;
        }

        private static string BuildSearchUri(string baseUrl, string endpoint, string searchText, string token)
        {
            var trimmedBaseUrl = baseUrl.TrimEnd('/');
            var query = new Dictionary<string, string?>
            {
                ["tekst"] = searchText,
                ["token"] = token,
                ["maksimum"] = ApiFetchLimit.ToString(CultureInfo.InvariantCulture)
            };

            var queryString = string.Join(
                "&",
                query.Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value!)}"));

            return $"{trimmedBaseUrl}/{endpoint}/soeg?{queryString}";
        }

        private static bool ShouldSearchUnits(string searchText) => UnitSearchPattern.IsMatch(searchText);

        private static string? ExtractUnitHint(string searchText)
        {
            var match = Regex.Match(
                searchText,
                @"\b(\d+)\s*(?:\.?\s*)?(th|tv|mf|kl|sal)\b|\b(\d+)(th|sal|tv)\b",
                RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return null;
            }

            var floor = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[3].Value;
            var unit = (match.Groups[2].Success ? match.Groups[2].Value : match.Groups[4].Value).ToLowerInvariant();

            return unit switch
            {
                "th" => $"{floor}. th",
                "tv" => $"{floor}. tv",
                "mf" => $"{floor}. mf",
                "kl" => $"kl. {floor}",
                "sal" => $"{floor}. sal",
                _ => null
            };
        }

        private static int UnitMatchRank(AdressevaelgerFund fund, string? unitHint)
        {
            if (unitHint == null || string.IsNullOrWhiteSpace(fund.Title))
            {
                return 1;
            }

            return fund.Title.IndexOf(unitHint, StringComparison.OrdinalIgnoreCase) >= 0 ? 0 : 1;
        }

        private static bool IsAutocompleteResult(AdressevaelgerFund fund) =>
            string.Equals(fund.Type, "husnummer", StringComparison.OrdinalIgnoreCase)
            || string.Equals(fund.Type, "adresse", StringComparison.OrdinalIgnoreCase)
            || string.Equals(fund.Type, "navngivenvejpostnummer", StringComparison.OrdinalIgnoreCase)
            || string.Equals(fund.Type, "vejnavn", StringComparison.OrdinalIgnoreCase);

        private static int GetResultSortOrder(string? type, bool preferUnits) => (preferUnits, type?.ToLowerInvariant()) switch
        {
            (true, "adresse") => 0,
            (true, "husnummer") => 1,
            (false, "husnummer") => 0,
            (false, "adresse") => 1,
            (_, "navngivenvejpostnummer") => 2,
            (_, "vejnavn") => 3,
            _ => 4
        };

        private static bool IsCompleteAddress(AdressevaelgerFund fund) =>
            string.Equals(fund.Type, "husnummer", StringComparison.OrdinalIgnoreCase)
            || string.Equals(fund.Type, "adresse", StringComparison.OrdinalIgnoreCase);

        private static DanishAddressAutocompleteResult? MapFund(AdressevaelgerFund fund)
        {
            if (string.IsNullOrWhiteSpace(fund.Title))
            {
                return null;
            }

            var displayName = fund.Title.Trim();
            var localId = !string.IsNullOrWhiteSpace(fund.Id)
                ? fund.Id
                : $"vejnavn:{displayName.ToLower(DanishCulture)}";
            var isComplete = IsCompleteAddress(fund);
            var resultType = fund.Type?.Trim() ?? string.Empty;

            var unitMatch = AddressWithUnitPattern.Match(displayName);
            if (unitMatch.Success)
            {
                return new DanishAddressAutocompleteResult(
                    localId,
                    displayName,
                    unitMatch.Groups["street"].Value.Trim(),
                    unitMatch.Groups["postal"].Value.Trim(),
                    unitMatch.Groups["city"].Value.Trim(),
                    unitMatch.Groups["unit"].Value.Trim(),
                    isComplete,
                    resultType);
            }

            var match = AddressPattern.Match(displayName);
            if (match.Success)
            {
                return new DanishAddressAutocompleteResult(
                    localId,
                    displayName,
                    match.Groups["line1"].Value.Trim(),
                    match.Groups["postal"].Value.Trim(),
                    match.Groups["city"].Value.Trim(),
                    string.Empty,
                    isComplete,
                    resultType);
            }

            var addressLine1 = !string.IsNullOrWhiteSpace(fund.StreetName) && !string.IsNullOrWhiteSpace(fund.HouseNumber)
                ? $"{fund.StreetName.Trim()} {fund.HouseNumber.Trim()}"
                : fund.StreetName?.Trim() ?? displayName;

            return new DanishAddressAutocompleteResult(
                localId,
                displayName,
                addressLine1,
                fund.PostalCode?.Trim() ?? string.Empty,
                fund.PostalDistrict?.Trim() ?? string.Empty,
                string.Empty,
                isComplete,
                resultType);
        }

        private static bool TryGetCached(string cacheKey, out IReadOnlyList<DanishAddressAutocompleteResult> results)
        {
            if (SearchCache.TryGetValue(cacheKey, out var entry) && entry.ExpiresAtUtc > DateTime.UtcNow)
            {
                results = entry.Results;
                return true;
            }

            results = Array.Empty<DanishAddressAutocompleteResult>();
            return false;
        }

        private static void SetCached(string cacheKey, IReadOnlyList<DanishAddressAutocompleteResult> results)
        {
            SearchCache[cacheKey] = new CacheEntry(results, DateTime.UtcNow.Add(SearchCacheDuration));
        }

        private sealed class CacheEntry
        {
            public CacheEntry(IReadOnlyList<DanishAddressAutocompleteResult> results, DateTime expiresAtUtc)
            {
                Results = results;
                ExpiresAtUtc = expiresAtUtc;
            }

            public IReadOnlyList<DanishAddressAutocompleteResult> Results { get; }

            public DateTime ExpiresAtUtc { get; }
        }

        private sealed class AdressevaelgerSearchResponse
        {
            [JsonPropertyName("status")]
            public string? Status { get; set; }

            [JsonPropertyName("beskrivelse")]
            public string? Description { get; set; }

            [JsonPropertyName("fund")]
            public List<AdressevaelgerFund> Results { get; set; } = new();
        }

        private sealed class AdressevaelgerFund
        {
            [JsonPropertyName("type")]
            public string? Type { get; set; }

            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("titel")]
            public string? Title { get; set; }

            [JsonPropertyName("vejnavn")]
            public string? StreetName { get; set; }

            [JsonPropertyName("husnummer")]
            public string? HouseNumber { get; set; }

            [JsonPropertyName("postnr")]
            public string? PostalCode { get; set; }

            [JsonPropertyName("postdistrikt")]
            public string? PostalDistrict { get; set; }
        }
    }
}
