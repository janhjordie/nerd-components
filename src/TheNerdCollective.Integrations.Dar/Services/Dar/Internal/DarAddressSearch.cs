using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TheNerdCollective.Integrations.Dar.Compatibility;
using TheNerdCollective.Integrations.Dar.GraphQL;
using TheNerdCollective.Integrations.Dar.Services.Internal;

namespace TheNerdCollective.Integrations.Dar.Services.Dar.Internal
{
    internal static class DarAddressSearch
    {
        private static readonly Regex HouseNumberSuffixRegex = new Regex(
            @"(?<=\d)[a-z]$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        internal static async Task<JsonElement> FindHusnummerNodeAsync(
            GraphQlDataAccessor accessor,
            string streetAndNumber,
            string postalCode,
            CancellationToken cancellationToken)
        {
            var temporal = GraphQlDataAccessor.CreateTemporalVariables();

            foreach (var searchPrefix in BuildAddressSearchPrefixes(streetAndNumber, postalCode))
            {
                var nodes = await accessor.FetchDarNodesAsync(
                    GraphQlQueries.FindHusnummerByAddress,
                    new
                    {
                        search = searchPrefix,
                        temporal.Virkningstid,
                        temporal.Registreringstid
                    },
                    "DAR_Husnummer",
                    cancellationToken).ConfigureAwait(false);

                if (nodes.GetArrayLength() == 0)
                {
                    continue;
                }

                var match = SelectBestHusnummer(nodes, streetAndNumber, postalCode);
                if (match.HasValue)
                {
                    return match.Value.Clone();
                }
            }

            throw new InvalidOperationException(
                $"Ingen adresse fundet for \"{streetAndNumber}, {postalCode}\". " +
                "Prøv med stort bogstav i husnummer (fx 69A) eller fuld adresse.");
        }

        internal static async Task<JsonElement> FindHusnummerNodeByIdAsync(
            GraphQlDataAccessor accessor,
            string husnummerId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(husnummerId))
            {
                throw new ArgumentException("Husnummer-id må ikke være tomt.", nameof(husnummerId));
            }

            var temporal = GraphQlDataAccessor.CreateTemporalVariables();
            var nodes = await accessor.FetchDarNodesAsync(
                GraphQlQueries.GetHusnummerById,
                new
                {
                    husnummerId,
                    temporal.Virkningstid,
                    temporal.Registreringstid
                },
                "DAR_Husnummer",
                cancellationToken).ConfigureAwait(false);

            if (nodes.GetArrayLength() == 0)
            {
                throw new InvalidOperationException($"Ingen husnummer fundet for id \"{husnummerId}\".");
            }

            return nodes[0].Clone();
        }

        private static IEnumerable<string> BuildAddressSearchPrefixes(string streetAndNumber, string postalCode)
        {
            var normalizedStreet = NormalizeHouseLetter(streetAndNumber);

            return new[]
            {
                $"{normalizedStreet}, {postalCode}",
                $"{streetAndNumber}, {postalCode}",
                normalizedStreet,
                streetAndNumber
            };
        }

        private static string NormalizeHouseLetter(string streetAndNumber) =>
            HouseNumberSuffixRegex.Replace(streetAndNumber, match => match.Value.ToUpperInvariant());

        private static JsonElement? SelectBestHusnummer(JsonElement nodes, string streetAndNumber, string postalCode)
        {
            var candidates = nodes.EnumerateArray()
                .Where(node =>
                {
                    var address = node.GetProperty("adgangsadressebetegnelse").GetString();
                    return address != null
                        && StringCompat.ContainsOrdinal(address, postalCode)
                        && AddressMatchesStreet(address, streetAndNumber);
                })
                .ToList();

            if (candidates.Count == 0)
            {
                return nodes.EnumerateArray().FirstOrDefault();
            }

            return candidates
                .OrderBy(node => node.GetProperty("adgangsadressebetegnelse").GetString(), StringComparer.OrdinalIgnoreCase)
                .First();
        }

        private static bool AddressMatchesStreet(string fullAddress, string streetAndNumber)
        {
            var commaIndex = fullAddress.IndexOf(',');
            var streetPart = commaIndex >= 0 ? fullAddress.Substring(0, commaIndex) : fullAddress;
            return streetPart.Equals(streetAndNumber, StringComparison.OrdinalIgnoreCase)
                || streetPart.Equals(NormalizeHouseLetter(streetAndNumber), StringComparison.OrdinalIgnoreCase);
        }
    }
}
