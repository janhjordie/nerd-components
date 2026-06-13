using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TheNerdCollective.Integrations.Dar.GraphQL;
using TheNerdCollective.Integrations.Dar.Json;
using TheNerdCollective.Integrations.Dar.Models;
using TheNerdCollective.Integrations.Dar.Services.Internal;

namespace TheNerdCollective.Integrations.Dar.Services.Bbr
{
    /// <summary>BBR-bygning.</summary>
    public sealed class BbrBygningService
    {
        private readonly GraphQlDataAccessor _accessor;

        public BbrBygningService(GraphQlDataAccessor accessor)
        {
            _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        }

        /// <summary>Henter bygning ud fra BBR lokal ID.</summary>
        public async Task<BygningDto> GetByIdAsync(
            string bygningId,
            CancellationToken cancellationToken = default)
        {
            var temporal = GraphQlDataAccessor.CreateTemporalVariables();
            var node = await _accessor.FetchBbrSingleNodeAsync(
                GraphQlQueries.GetBygningById,
                new { bygningId, temporal.Virkningstid, temporal.Registreringstid },
                "BBR_Bygning",
                cancellationToken).ConfigureAwait(false);

            return DarJsonSerializer.DeserializeRequired<BygningDto>(node);
        }

        /// <summary>Henter første bygning knyttet til et DAR husnummer.</summary>
        public async Task<BygningDto> GetByHusnummerIdAsync(
            string husnummerId,
            CancellationToken cancellationToken = default)
        {
            var temporal = GraphQlDataAccessor.CreateTemporalVariables();
            var nodes = await _accessor.FetchBbrNodesAsync(
                GraphQlQueries.GetBygningByHusnummer,
                new { husnummerId, temporal.Virkningstid, temporal.Registreringstid },
                "BBR_Bygning",
                cancellationToken).ConfigureAwait(false);

            if (nodes.GetArrayLength() == 0)
            {
                throw new InvalidOperationException($"Husnummer \"{husnummerId}\" har intet tilknyttet BBR-bygning.");
            }

            return DarJsonSerializer.DeserializeRequired<BygningDto>(nodes[0]);
        }
    }
}
