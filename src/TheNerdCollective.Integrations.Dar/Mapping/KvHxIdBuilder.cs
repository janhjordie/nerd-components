using System;

namespace TheNerdCollective.Integrations.Dar.Mapping
{
    internal static class KvHxIdBuilder
    {
        private const int SuffixLength = 9;

        /// <summary>Bygger DAWA-format KVHX for adgangsadresse (19 tegn).</summary>
        internal static string BuildAdgangsadresse(string kommunekode, string vejkode, string husnummer)
        {
            if (string.IsNullOrWhiteSpace(husnummer))
            {
                throw new ArgumentException("Husnummer må ikke være tomt.", nameof(husnummer));
            }

            if (husnummer.Length > SuffixLength)
            {
                throw new ArgumentException(
                    $"Husnummer \"{husnummer}\" er for langt til KVHX (max {SuffixLength} tegn).",
                    nameof(husnummer));
            }

            var suffix = husnummer + new string('_', SuffixLength - husnummer.Length);
            return $"{kommunekode}{vejkode}__{suffix}";
        }
    }
}
