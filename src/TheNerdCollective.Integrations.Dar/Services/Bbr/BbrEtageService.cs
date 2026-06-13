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
    /// <summary>BBR-etager.</summary>
    public sealed class BbrEtageService
    {
        private readonly GraphQlDataAccessor _accessor;

        public BbrEtageService(GraphQlDataAccessor accessor)
        {
            _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        }

        /// <summary>Henter etager for en bygning.</summary>
        public async Task<IReadOnlyList<EtageDto>> GetByBygningIdAsync(
            string bygningId,
            CancellationToken cancellationToken = default)
        {
            var temporal = GraphQlDataAccessor.CreateTemporalVariables();
            var nodes = await _accessor.FetchBbrNodesAsync(
                GraphQlQueries.GetEtagerByBygning,
                new { bygningId, temporal.Virkningstid, temporal.Registreringstid },
                "BBR_Etage",
                cancellationToken).ConfigureAwait(false);

            return DarJsonSerializer.DeserializeList<EtageDto>(nodes);
        }
    }
}
