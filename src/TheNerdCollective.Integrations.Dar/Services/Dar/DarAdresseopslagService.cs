using System;
using System.Threading;
using System.Threading.Tasks;
using TheNerdCollective.Integrations.Dar.Json;
using TheNerdCollective.Integrations.Dar.Mapping;
using TheNerdCollective.Integrations.Dar.Models;
using TheNerdCollective.Integrations.Dar.Services.Dar.Internal;
using TheNerdCollective.Integrations.Dar.Services.Internal;

namespace TheNerdCollective.Integrations.Dar.Services.Dar
{
    /// <summary>DAR-adresseopslag via Datafordeler.</summary>
    public sealed class DarAdresseopslagService
    {
        private readonly GraphQlDataAccessor _accessor;

        public DarAdresseopslagService(GraphQlDataAccessor accessor)
        {
            _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        }

        /// <summary>Slår en adresse op ud fra vej/husnummer og postnummer.</summary>
        public async Task<AdresseopslagResult> LookupAsync(
            string streetAndNumber,
            string postalCode,
            string? city = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(streetAndNumber))
            {
                throw new ArgumentException("Vej og husnummer må ikke være tom.", nameof(streetAndNumber));
            }

            if (string.IsNullOrWhiteSpace(postalCode))
            {
                throw new ArgumentException("Postnummer må ikke være tom.", nameof(postalCode));
            }

            var husnummerNode = await DarAddressSearch.FindHusnummerNodeAsync(
                _accessor,
                streetAndNumber,
                postalCode,
                cancellationToken).ConfigureAwait(false);

            return await MapResultAsync(husnummerNode, streetAndNumber, postalCode, city, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>Slår en adresse op ud fra fuld adresse, fx "Århusvej 69a, 3000 Helsingør".</summary>
        public Task<AdresseopslagResult> LookupAsync(
            string fullAddress,
            CancellationToken cancellationToken = default)
        {
            var parsed = DarAddressParser.Parse(fullAddress);
            return LookupAsync(parsed.StreetAndNumber, parsed.PostalCode, parsed.City, cancellationToken);
        }

        private async Task<AdresseopslagResult> MapResultAsync(
            System.Text.Json.JsonElement husnummerNode,
            string streetAndNumber,
            string postalCode,
            string? city,
            CancellationToken cancellationToken)
        {
            var husnummerId = husnummerNode.GetProperty("id_lokalId").GetString()
                ?? throw new InvalidOperationException("Husnummer mangler id_lokalId.");

            var adgangsadresse = husnummerNode.GetProperty("adgangsadressebetegnelse").GetString()
                ?? $"{streetAndNumber}, {postalCode}";

            var bygningId = husnummerNode.TryGetProperty("adgangTilBygning", out var bygningRef)
                ? bygningRef.GetString()
                : null;

            var husnummer = DarJsonSerializer.DeserializeRequired<HusnummerDto>(husnummerNode);
            var vejnavn = await DarNavngivenVejLookup.TryGetVejnavnAsync(
                _accessor,
                husnummer.NavngivenVej,
                cancellationToken).ConfigureAwait(false);

            return new AdresseopslagResult
            {
                Husnummer = husnummer,
                Adgangsadresse = adgangsadresse,
                HusnummerId = husnummerId,
                BygningId = bygningId,
                StreetAndNumber = streetAndNumber,
                PostalCode = postalCode,
                City = city ?? (husnummerNode.TryGetProperty("supplerendeBynavn", out var bynavn)
                    ? bynavn.GetString()
                    : null),
                KvHxInput = AdresseopslagKvHxMapper.Map(husnummer, adgangsadresse, husnummerId, postalCode, vejnavn)
            };
        }
    }
}
