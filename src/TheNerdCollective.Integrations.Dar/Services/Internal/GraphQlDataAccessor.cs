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

        public GraphQlDataAccessor(DatafordelerGraphQlClient client, string bbrEndpoint, string darEndpoint)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _bbrEndpoint = bbrEndpoint ?? throw new ArgumentNullException(nameof(bbrEndpoint));
            _darEndpoint = darEndpoint ?? throw new ArgumentNullException(nameof(darEndpoint));
        }

        internal string BbrEndpoint => _bbrEndpoint;

        internal string DarEndpoint => _darEndpoint;

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
