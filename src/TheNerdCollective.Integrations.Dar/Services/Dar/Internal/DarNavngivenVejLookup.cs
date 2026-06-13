using System;
using System.Threading;
using System.Threading.Tasks;
using TheNerdCollective.Integrations.Dar.GraphQL;
using TheNerdCollective.Integrations.Dar.Services.Internal;

namespace TheNerdCollective.Integrations.Dar.Services.Dar.Internal
{
    internal static class DarNavngivenVejLookup
    {
        internal static async Task<string?> TryGetVejnavnAsync(
            GraphQlDataAccessor accessor,
            string? navngivenVejId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(navngivenVejId))
            {
                return null;
            }

            var temporal = GraphQlDataAccessor.CreateTemporalVariables();
            var nodes = await accessor.FetchDarNodesAsync(
                GraphQlQueries.GetNavngivenVejById,
                new
                {
                    navngivenVejId,
                    temporal.Virkningstid,
                    temporal.Registreringstid
                },
                "DAR_NavngivenVej",
                cancellationToken).ConfigureAwait(false);

            if (nodes.GetArrayLength() == 0)
            {
                return null;
            }

            return nodes[0].TryGetProperty("vejnavn", out var vejnavn)
                ? vejnavn.GetString()
                : null;
        }
    }
}
