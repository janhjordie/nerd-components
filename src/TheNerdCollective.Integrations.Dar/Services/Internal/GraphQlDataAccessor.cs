using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TheNerdCollective.Integrations.Dar.GraphQL;

namespace TheNerdCollective.Integrations.Dar.Services.Internal
{
    /// <summary>Intern GraphQL-adgang — brug <see cref="DarClientFactory"/> i stedet.</summary>
    public sealed class GraphQlDataAccessor
    {
        private readonly DatafordelerGraphQlClient _client;
        private readonly string _bbrEndpoint;
        private readonly string _darEndpoint;
        private readonly string _dagiEndpoint;

        public GraphQlDataAccessor(
            DatafordelerGraphQlClient client,
            string bbrEndpoint,
            string darEndpoint,
            string dagiEndpoint)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _bbrEndpoint = bbrEndpoint ?? throw new ArgumentNullException(nameof(bbrEndpoint));
            _darEndpoint = darEndpoint ?? throw new ArgumentNullException(nameof(darEndpoint));
            _dagiEndpoint = dagiEndpoint ?? throw new ArgumentNullException(nameof(dagiEndpoint));
        }

        internal string BbrEndpoint => _bbrEndpoint;

        internal string DarEndpoint => _darEndpoint;

        internal string DagiEndpoint => _dagiEndpoint;

        internal Task<JsonElement> FetchBbrNodesAsync(
            string query,
            object variables,
            string rootField,
            CancellationToken cancellationToken = default) =>
            FetchNodesAsync(_bbrEndpoint, query, variables, rootField, cancellationToken);

        internal Task<JsonElement> FetchDarNodesAsync(
            string query,
            object variables,
            string rootField,
            CancellationToken cancellationToken = default) =>
            FetchNodesAsync(_darEndpoint, query, variables, rootField, cancellationToken);

        internal Task<JsonElement> FetchDagiNodesAsync(
            string query,
            object variables,
            string rootField,
            CancellationToken cancellationToken = default) =>
            FetchNodesAsync(_dagiEndpoint, query, variables, rootField, cancellationToken);

        internal async Task<JsonElement> FetchAllDagiNodesAsync(
            string query,
            Func<string?, object> createVariables,
            string rootField,
            CancellationToken cancellationToken = default)
        {
            var items = new List<JsonElement>();
            string? after = null;

            while (true)
            {
                using var response = await _client.ExecuteAsync(
                    _dagiEndpoint,
                    query,
                    createVariables(after),
                    cancellationToken).ConfigureAwait(false);

                var connection = response.RootElement
                    .GetProperty("data")
                    .GetProperty(rootField);

                foreach (var node in connection.GetProperty("nodes").EnumerateArray())
                {
                    items.Add(node.Clone());
                }

                if (!connection.TryGetProperty("pageInfo", out var pageInfo)
                    || !pageInfo.TryGetProperty("hasNextPage", out var hasNextPage)
                    || !hasNextPage.GetBoolean())
                {
                    break;
                }

                after = pageInfo.TryGetProperty("endCursor", out var cursor)
                    ? cursor.GetString()
                    : null;

                if (string.IsNullOrEmpty(after))
                {
                    break;
                }
            }

            return ToJsonArray(items);
        }

        internal async Task<JsonElement> FetchDagiSingleNodeAsync(
            string query,
            object variables,
            string rootField,
            CancellationToken cancellationToken = default)
        {
            var nodes = await FetchDagiNodesAsync(query, variables, rootField, cancellationToken).ConfigureAwait(false);
            if (nodes.GetArrayLength() == 0)
            {
                throw new InvalidOperationException($"Ingen data returneret for {rootField}.");
            }

            return nodes[0].Clone();
        }

        internal async Task<JsonElement> FetchBbrSingleNodeAsync(
            string query,
            object variables,
            string rootField,
            CancellationToken cancellationToken = default)
        {
            var nodes = await FetchBbrNodesAsync(query, variables, rootField, cancellationToken).ConfigureAwait(false);
            if (nodes.GetArrayLength() == 0)
            {
                throw new InvalidOperationException($"Ingen data returneret for {rootField}.");
            }

            return nodes[0].Clone();
        }

        internal static TemporalVariables CreateTemporalVariables()
        {
            var now = DateTime.UtcNow.ToString(
                "yyyy-MM-ddTHH:mm:ss.000000'Z'",
                System.Globalization.CultureInfo.InvariantCulture);
            return new TemporalVariables(now, now);
        }

        internal static JsonElement EmptyArray() => ToJsonArray(Array.Empty<JsonElement>());

        internal static JsonElement ToJsonArray(IReadOnlyList<JsonElement> items)
        {
            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream))
            {
                writer.WriteStartArray();
                foreach (var item in items)
                {
                    item.WriteTo(writer);
                }

                writer.WriteEndArray();
            }

            return JsonDocument.Parse(stream.ToArray()).RootElement.Clone();
        }

        private async Task<JsonElement> FetchNodesAsync(
            string endpoint,
            string query,
            object variables,
            string rootField,
            CancellationToken cancellationToken)
        {
            using var response = await _client.ExecuteAsync(endpoint, query, variables, cancellationToken).ConfigureAwait(false);
            return response.RootElement
                .GetProperty("data")
                .GetProperty(rootField)
                .GetProperty("nodes")
                .Clone();
        }

        internal sealed class TemporalVariables
        {
            public TemporalVariables(string virkningstid, string registreringstid)
            {
                Virkningstid = virkningstid;
                Registreringstid = registreringstid;
            }

            public string Virkningstid { get; }

            public string Registreringstid { get; }
        }
    }
}
