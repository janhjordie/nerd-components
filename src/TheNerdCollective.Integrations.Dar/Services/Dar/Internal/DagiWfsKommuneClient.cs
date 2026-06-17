using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TheNerdCollective.Integrations.Dar.Configuration;
using TheNerdCollective.Integrations.Dar.Models;

namespace TheNerdCollective.Integrations.Dar.Services.Dar.Internal;

/// <summary>WFS-fallback til kommune-liste når GraphQL er tom (samme API-nøgle).</summary>
internal sealed class DagiWfsKommuneClient
{
    private const string FeatureType = "dagi_v001:kommuneinddeling_current";
    private const int PageSize = 200;

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly DarDagiOptions _options;

    public DagiWfsKommuneClient(HttpClient httpClient, string apiKey, DarDagiOptions options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    internal async Task<IReadOnlyList<KommuneDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = new List<KommuneDto>();
        var startIndex = 0;

        while (true)
        {
            var url =
                $"{(_options.WfsUrl ?? DarDagiOptions.DefaultWfsUrl).TrimEnd('?')}" +
                $"?service=WFS&version=2.0.0&request=GetFeature" +
                $"&typeNames={Uri.EscapeDataString(FeatureType)}" +
                $"&outputFormat=application/json" +
                $"&count={PageSize}" +
                $"&startIndex={startIndex}" +
                $"&apiKey={Uri.EscapeDataString(_apiKey)}";

            using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"DAGI WFS returnerede HTTP {(int)response.StatusCode}: {body}");
            }

            using var document = JsonDocument.Parse(body);
            var page = DagiKommuneJsonParser.ParseKommuneList(document.RootElement);
            if (page.Count == 0)
            {
                break;
            }

            items.AddRange(page);

            if (page.Count < PageSize)
            {
                break;
            }

            startIndex += PageSize;
        }

        return items
            .GroupBy(k => k.Kommunekode ?? k.IdLokalId ?? k.Navn ?? string.Empty, StringComparer.Ordinal)
            .Select(g => g.First())
            .OrderBy(k => k.Navn, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }
}
