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

            return MapResult(result);
        }

        /// <summary>Finder husnummer via DAR husnummer-id (<c>id_lokalId</c>).</summary>
        public async Task<HusnummerLookupResult> FindByHusnummerIdAsync(
            string husnummerId,
            CancellationToken cancellationToken = default)
        {
            var result = await _adresseopslag.LookupByHusnummerIdAsync(husnummerId, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return MapResult(result);
        }

        private static HusnummerLookupResult MapResult(AdresseopslagResult result) =>
            new()
            {
                Dar = result.Dar,
                Husnummer = result.Husnummer,
                Adgangsadresse = result.Adgangsadresse,
                HusnummerId = result.HusnummerId,
                BygningId = result.BygningId
            };
    }
}
