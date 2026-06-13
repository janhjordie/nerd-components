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
    /// <summary>BBR-tekniske anlæg.</summary>
    public sealed class BbrTekniskAnlaegService
    {
        private readonly GraphQlDataAccessor _accessor;

        public BbrTekniskAnlaegService(GraphQlDataAccessor accessor)
        {
            _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        }

        /// <summary>Henter tekniske anlæg for en bygning.</summary>
        public async Task<IReadOnlyList<TekniskAnlaegDto>> GetByBygningIdAsync(
            string bygningId,
            CancellationToken cancellationToken = default)
        {
            var temporal = GraphQlDataAccessor.CreateTemporalVariables();
            var nodes = await _accessor.FetchBbrNodesAsync(
                GraphQlQueries.GetTekniskeAnlaegByBygning,
                new { bygningId, temporal.Virkningstid, temporal.Registreringstid },
                "BBR_TekniskAnlaeg",
                cancellationToken).ConfigureAwait(false);

            return DarJsonSerializer.DeserializeList<TekniskAnlaegDto>(nodes);
        }
    }
}
