using System;
using TheNerdCollective.Integrations.Dar.Models;
using Riok.Mapperly.Abstractions;

namespace TheNerdCollective.Integrations.Dar.Mapping
{
    [Mapper]
    internal static partial class AdresseopslagKvHxMapper
    {
        /// <summary>Mapper native DAR-data til legacy DAWA/KVHX-format.</summary>
        internal static KvHxInputDto Map(
            HusnummerDto husnummer,
            string adgangsadresse,
            string husnummerId,
            string postalCode,
            string? vejnavn,
            AdresseDto? adresse = null)
        {
            var husnummertekst = husnummer.Husnummertekst
                ?? throw new InvalidOperationException("Husnummer mangler husnummertekst.");

            var (kommunekode, vejkode) = VejmidteParser.Parse(husnummer.Vejmidte);
            var etage = adresse?.Etagebetegnelse;
            var door = adresse?.Doerbetegnelse;
            var hasUnit = !string.IsNullOrWhiteSpace(etage) || !string.IsNullOrWhiteSpace(door);
            var addressId = hasUnit && !string.IsNullOrWhiteSpace(adresse?.IdLokalId)
                ? adresse!.IdLokalId!
                : husnummerId;
            var adressebetegnelse = !string.IsNullOrWhiteSpace(adresse?.Adressebetegnelse)
                ? adresse!.Adressebetegnelse!
                : adgangsadresse;

            return ToKvHxInput(new KvHxInputMappingSource
            {
                Adgangsadresse = adressebetegnelse,
                HusnummerId = addressId,
                PostalCode = postalCode,
                Vejnavn = vejnavn ?? ParseVejnavnFallback(adgangsadresse, husnummertekst),
                Komunekode = kommunekode,
                Vejkode = vejkode,
                KvhxId = hasUnit
                    ? KvHxIdBuilder.BuildEnhedsadresse(kommunekode, vejkode, husnummertekst, etage, door)
                    : KvHxIdBuilder.BuildAdgangsadresse(kommunekode, vejkode, husnummertekst),
                Husnummer = husnummertekst,
                Esrejendomsnr = "0",
                Etage = etage ?? string.Empty,
                Door = door ?? string.Empty
            });
        }

        [MapProperty(nameof(KvHxInputMappingSource.Adgangsadresse), nameof(KvHxInputDto.Adressebetegnelse))]
        [MapProperty(nameof(KvHxInputMappingSource.HusnummerId), nameof(KvHxInputDto.Id))]
        [MapProperty(nameof(KvHxInputMappingSource.PostalCode), nameof(KvHxInputDto.Postnummer))]
        [MapProperty(nameof(KvHxInputMappingSource.Door), nameof(KvHxInputDto.Door))]
        private static partial KvHxInputDto ToKvHxInput(KvHxInputMappingSource source);

        private static string ParseVejnavnFallback(string adgangsadresse, string husnummer)
        {
            var commaIndex = adgangsadresse.IndexOf(',');
            var streetPart = commaIndex >= 0
                ? adgangsadresse.Substring(0, commaIndex)
                : adgangsadresse;

            if (streetPart.EndsWith(husnummer, StringComparison.OrdinalIgnoreCase))
            {
                return streetPart.Substring(0, streetPart.Length - husnummer.Length).TrimEnd();
            }

            var lastSpace = streetPart.LastIndexOf(' ');
            return lastSpace > 0 ? streetPart.Substring(0, lastSpace) : streetPart;
        }

        private sealed class KvHxInputMappingSource
        {
            public required string Adgangsadresse { get; init; }

            public required string HusnummerId { get; init; }

            public required string PostalCode { get; init; }

            public required string Vejnavn { get; init; }

            public required string Komunekode { get; init; }

            public required string Vejkode { get; init; }

            public required string KvhxId { get; init; }

            public required string Husnummer { get; init; }

            public required string Esrejendomsnr { get; init; }

            public required string Etage { get; init; }

            public required string Door { get; init; }
        }
    }
}
