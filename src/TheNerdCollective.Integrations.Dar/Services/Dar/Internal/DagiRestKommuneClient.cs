using System;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TheNerdCollective.Integrations.Dar.Configuration;
using TheNerdCollective.Integrations.Dar.Models;

namespace TheNerdCollective.Integrations.Dar.Services.Dar.Internal;

/// <summary>
/// REST DAGI multigeometri-punkt — fallback når GraphQL er tom.
/// <see href="https://confluence.sdfi.dk/pages/viewpage.action?pageId=13666129"/>
/// </summary>
internal sealed class DagiRestKommuneClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly DarDagiOptions _options;

    public DagiRestKommuneClient(HttpClient httpClient, string apiKey, DarDagiOptions options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    internal async Task<KommuneDto?> FindByPointAsync(
        double easting,
        double northing,
        CancellationToken cancellationToken = default)
    {
        var x = easting.ToString("0.########", CultureInfo.InvariantCulture);
        var y = northing.ToString("0.########", CultureInfo.InvariantCulture);
        var url = BuildPointUrl(x, y);

        using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden
                && string.IsNullOrWhiteSpace(_options.RestUsername))
            {
                return null;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            throw new InvalidOperationException(
                $"REST DAGI returnerede HTTP {(int)response.StatusCode}: {body}");
        }

        using var document = JsonDocument.Parse(body);
        return DagiKommuneJsonParser.ParseKommuneinddeling(document.RootElement);
    }

    private string BuildPointUrl(string x, string y)
    {
        var baseUrl = (_options.RestUrl ?? DarDagiOptions.DefaultRestUrl).TrimEnd('?');
        var query = $"Format=JSON&x={Uri.EscapeDataString(x)}&y={Uri.EscapeDataString(y)}";

        if (!string.IsNullOrWhiteSpace(_options.RestUsername) && !string.IsNullOrWhiteSpace(_options.RestPassword))
        {
            query += $"&username={Uri.EscapeDataString(_options.RestUsername)}&password={Uri.EscapeDataString(_options.RestPassword)}";
        }
        else
        {
            query += $"&apiKey={Uri.EscapeDataString(_apiKey)}";
        }

        return baseUrl.IndexOf('?') >= 0
            ? $"{baseUrl}&{query}"
            : $"{baseUrl}?{query}";
    }
}
