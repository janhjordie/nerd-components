using System;
using System.Threading;
using System.Threading.Tasks;
using TheNerdCollective.Integrations.Dar.Models;
using TheNerdCollective.Integrations.Dar.Services.Dar.Internal;
using TheNerdCollective.Integrations.Dar.Services.Internal;

namespace TheNerdCollective.Integrations.Dar.Services.Dar
{
    /// <summary>DAR husnummer (entity-niveau).</summary>
    public sealed class DarHusnummerService
    {
        private readonly DarAdresseopslagService _adresseopslag;

        public DarHusnummerService(GraphQlDataAccessor accessor)
        {
            _adresseopslag = new DarAdresseopslagService(accessor);
        }

        /// <summary>Finder husnummer ud fra vej og postnummer.</summary>
        public async Task<HusnummerLookupResult> FindByAddressAsync(
            string streetAndNumber,
            string postalCode,
            CancellationToken cancellationToken = default)
        {
            var result = await _adresseopslag.LookupAsync(streetAndNumber, postalCode, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return new HusnummerLookupResult
            {
                Dar = result.Dar,
                Husnummer = result.Husnummer,
                Adgangsadresse = result.Adgangsadresse,
                HusnummerId = result.HusnummerId,
                BygningId = result.BygningId
            };
        }
    }
}
