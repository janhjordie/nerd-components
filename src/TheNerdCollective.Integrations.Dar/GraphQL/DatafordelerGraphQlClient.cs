using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TheNerdCollective.Integrations.Dar.Compatibility;

namespace TheNerdCollective.Integrations.Dar.GraphQL
{
    public sealed class DatafordelerGraphQlClient
    {
        private const int MaxAuthRetries = 5;
        private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(500);

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public DatafordelerGraphQlClient(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        }

        public async Task<JsonDocument> ExecuteAsync(
            string endpoint,
            string query,
            object? variables = null,
            CancellationToken cancellationToken = default)
        {
            for (var attempt = 1; attempt <= MaxAuthRetries; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var requestUri = $"{endpoint.TrimEnd('/')}?apiKey={Uri.EscapeDataString(_apiKey)}";
                var payload = new GraphQlRequest(query, variables);

                using var response = await _httpClient.PostAsJsonAsync(requestUri, payload, JsonOptions, cancellationToken)
                    .ConfigureAwait(false);
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                    && StringCompat.ContainsOrdinal(body, "DAF-AUTH-0005")
                    && attempt < MaxAuthRetries)
                {
                    await Task.Delay(RetryDelay, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw new DatafordelerApiException(
                        $"Datafordeler returnerede HTTP {(int)response.StatusCode}: {body}",
                        (int)response.StatusCode,
                        body);
                }

                var document = JsonDocument.Parse(body);

                if (document.RootElement.TryGetProperty("errors", out var errors) && errors.GetArrayLength() > 0)
                {
                    throw new DatafordelerApiException(
                        $"GraphQL-fejl: {errors}",
                        (int)response.StatusCode,
                        body);
                }

                return document;
            }

            throw new DatafordelerApiException(
                "Datafordeler afviste adgang efter flere forsøg (IP-whitelist kan være under opdatering).",
                401,
                string.Empty);
        }

        private sealed class GraphQlRequest
        {
            public GraphQlRequest(string query, object? variables)
            {
                Query = query;
                Variables = variables;
            }

            public string Query { get; }

            public object? Variables { get; }
        }
    }

    public sealed class DatafordelerApiException : Exception
    {
        public DatafordelerApiException(string message, int statusCode, string responseBody)
            : base(message)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }

        public int StatusCode { get; }

        public string ResponseBody { get; }
    }
}
