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
    /// <summary>BBR-grund og grund-jordstykker.</summary>
    public sealed class BbrGrundService
    {
        private readonly GraphQlDataAccessor _accessor;

        public BbrGrundService(GraphQlDataAccessor accessor)
        {
            _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        }

        /// <summary>Henter grund ud fra BBR lokal ID.</summary>
        public async Task<GrundDto> GetByIdAsync(
            string grundId,
            CancellationToken cancellationToken = default)
        {
            var temporal = GraphQlDataAccessor.CreateTemporalVariables();
            var node = await _accessor.FetchBbrSingleNodeAsync(
                GraphQlQueries.GetGrundById,
                new { grundId, temporal.Virkningstid, temporal.Registreringstid },
                "BBR_Grund",
                cancellationToken).ConfigureAwait(false);

            return DarJsonSerializer.DeserializeRequired<GrundDto>(node);
        }

        /// <summary>Henter grund-jordstykker for en grund.</summary>
        public async Task<IReadOnlyList<GrundJordstykkeDto>> GetJordstykkerByGrundIdAsync(
            string grundId,
            CancellationToken cancellationToken = default)
        {
            var temporal = GraphQlDataAccessor.CreateTemporalVariables();
            var nodes = await _accessor.FetchBbrNodesAsync(
                GraphQlQueries.GetGrundJordstykkerByGrund,
                new { grundId, temporal.Virkningstid, temporal.Registreringstid },
                "BBR_GrundJordstykke",
                cancellationToken).ConfigureAwait(false);

            return DarJsonSerializer.DeserializeList<GrundJordstykkeDto>(nodes);
        }
    }
}
