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
    /// <summary>BBR-enheder.</summary>
    public sealed class BbrEnhedService
    {
        private readonly GraphQlDataAccessor _accessor;

        public BbrEnhedService(GraphQlDataAccessor accessor)
        {
            _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        }

        /// <summary>Henter enheder for en bygning.</summary>
        public async Task<IReadOnlyList<EnhedDto>> GetByBygningIdAsync(
            string bygningId,
            CancellationToken cancellationToken = default)
        {
            var temporal = GraphQlDataAccessor.CreateTemporalVariables();
            var nodes = await _accessor.FetchBbrNodesAsync(
                GraphQlQueries.GetEnhederByBygning,
                new { bygningId, temporal.Virkningstid, temporal.Registreringstid },
                "BBR_Enhed",
                cancellationToken).ConfigureAwait(false);

            return DarJsonSerializer.DeserializeList<EnhedDto>(nodes);
        }
    }
}
