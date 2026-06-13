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
    /// <summary>BBR-opgange.</summary>
    public sealed class BbrOpgangService
    {
        private readonly GraphQlDataAccessor _accessor;

        public BbrOpgangService(GraphQlDataAccessor accessor)
        {
            _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        }

        /// <summary>Henter opgange for en bygning.</summary>
        public async Task<IReadOnlyList<OpgangDto>> GetByBygningIdAsync(
            string bygningId,
            CancellationToken cancellationToken = default)
        {
            var temporal = GraphQlDataAccessor.CreateTemporalVariables();
            var nodes = await _accessor.FetchBbrNodesAsync(
                GraphQlQueries.GetOpgangeByBygning,
                new { bygningId, temporal.Virkningstid, temporal.Registreringstid },
                "BBR_Opgang",
                cancellationToken).ConfigureAwait(false);

            return DarJsonSerializer.DeserializeList<OpgangDto>(nodes);
        }
    }
}
