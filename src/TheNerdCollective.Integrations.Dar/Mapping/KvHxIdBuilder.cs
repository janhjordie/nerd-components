using System;

namespace TheNerdCollective.Integrations.Dar.Mapping
{
    internal static class KvHxIdBuilder
    {
        private const int SuffixLength = 9;

        /// <summary>Bygger DAWA-format KVHX for adgangsadresse (19 tegn).</summary>
        internal static string BuildAdgangsadresse(string kommunekode, string vejkode, string husnummer) =>
            Build(kommunekode, vejkode, husnummer, null, null);

        /// <summary>Bygger DAWA-format KVHX for enhedsadresse (19 tegn) med etage og dørbetegnelse.</summary>
        internal static string BuildEnhedsadresse(
            string kommunekode,
            string vejkode,
            string husnummer,
            string? etagebetegnelse,
            string? doerbetegnelse) =>
            Build(kommunekode, vejkode, husnummer, etagebetegnelse, doerbetegnelse);

        private static string Build(
            string kommunekode,
            string vejkode,
            string husnummer,
            string? etagebetegnelse,
            string? doerbetegnelse)
        {
            if (string.IsNullOrWhiteSpace(husnummer))
            {
                throw new ArgumentException("Husnummer må ikke være tomt.", nameof(husnummer));
            }

            if (husnummer.Length > 4)
            {
                throw new ArgumentException(
                    $"Husnummer \"{husnummer}\" er for langt til KVHX (max 4 tegn).",
                    nameof(husnummer));
            }

            var etage = NormalizeComponent(etagebetegnelse);
            var door = NormalizeComponent(doerbetegnelse);
            if (!string.IsNullOrEmpty(door) && door!.Length > 4)
            {
                throw new ArgumentException(
                    $"Dørbetegnelse \"{door}\" er for lang til KVHX (max 4 tegn).",
                    nameof(doerbetegnelse));
            }

            var suffix = BuildSuffix(husnummer, etage, door);
            return $"{kommunekode}{vejkode}__{suffix}";
        }

        private static string BuildSuffix(string husnummer, string? etage, string? door)
        {
            if (string.IsNullOrEmpty(etage) && string.IsNullOrEmpty(door))
            {
                return PadRight(husnummer, SuffixLength, '_');
            }

            if (!string.IsNullOrEmpty(etage) && !string.IsNullOrEmpty(door))
            {
                return PadRight($"{husnummer}__{etage}__{door}", SuffixLength, '_');
            }

            return PadRight($"{husnummer}__{etage!}", SuffixLength, '_');
        }

        private static string? NormalizeComponent(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value!.Trim();

        private static string PadRight(string value, int totalWidth, char paddingChar)
        {
            if (value.Length >= totalWidth)
            {
                return value.Substring(0, totalWidth);
            }

            return value + new string(paddingChar, totalWidth - value.Length);
        }
    }
}
