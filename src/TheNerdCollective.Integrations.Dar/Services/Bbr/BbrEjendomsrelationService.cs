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
    /// <summary>BBR-ejendomsrelationer og BFE.</summary>
    public sealed class BbrEjendomsrelationService
    {
        private readonly GraphQlDataAccessor _accessor;

        public BbrEjendomsrelationService(GraphQlDataAccessor accessor)
        {
            _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        }

        /// <summary>Henter bygning-ejendomsrelationer for en bygning.</summary>
        public async Task<IReadOnlyList<BygningEjendomsrelationDto>> GetByBygningIdAsync(
            string bygningId,
            CancellationToken cancellationToken = default)
        {
            var temporal = GraphQlDataAccessor.CreateTemporalVariables();
            var nodes = await _accessor.FetchBbrNodesAsync(
                GraphQlQueries.GetBygningEjendomsrelationer,
                new { bygningId, temporal.Virkningstid, temporal.Registreringstid },
                "BBR_BygningEjendomsrelation",
                cancellationToken).ConfigureAwait(false);

            return DarJsonSerializer.DeserializeList<BygningEjendomsrelationDto>(nodes);
        }

        /// <summary>
        /// Henter ejendomsrelationer (BFE m.m.) ud fra bygning-relationer og evt. grund.
        /// </summary>
        public async Task<IReadOnlyList<EjendomsrelationDto>> ResolveAsync(
            IReadOnlyList<BygningEjendomsrelationDto> bygningEjendomsrelationer,
            GrundDto? grund = null,
            CancellationToken cancellationToken = default)
        {
            var relationIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (var relation in bygningEjendomsrelationer)
            {
                if (!string.IsNullOrWhiteSpace(relation.BygningPaaFremmedGrund))
                {
                    relationIds.Add(relation.BygningPaaFremmedGrund);
                }
            }

            if (!string.IsNullOrWhiteSpace(grund?.BestemtFastEjendom))
            {
                relationIds.Add(grund.BestemtFastEjendom);
            }

            if (relationIds.Count == 0)
            {
                return Array.Empty<EjendomsrelationDto>();
            }

            var temporal = GraphQlDataAccessor.CreateTemporalVariables();
            var results = new List<EjendomsrelationDto>();

            foreach (var relationId in relationIds)
            {
                try
                {
                    var node = await _accessor.FetchBbrSingleNodeAsync(
                        GraphQlQueries.GetEjendomsrelationById,
                        new { ejendomsrelationId = relationId, temporal.Virkningstid, temporal.Registreringstid },
                        "BBR_Ejendomsrelation",
                        cancellationToken).ConfigureAwait(false);

                    results.Add(DarJsonSerializer.DeserializeRequired<EjendomsrelationDto>(node));
                }
                catch (DatafordelerApiException)
                {
                    // Relation-ID kan pege på en anden entitetstype — spring over.
                }
            }

            return results;
        }
    }
}
