using System;

namespace TheNerdCollective.Integrations.Dar.Mapping
{
    internal static class VejmidteParser
    {
        internal static (string Kommunekode, string Vejkode) Parse(string? vejmidte)
        {
            if (string.IsNullOrWhiteSpace(vejmidte))
            {
                throw new InvalidOperationException("Husnummer mangler vejmidte (kommunekode-vejkode).");
            }

            var separatorIndex = vejmidte.IndexOf('-');
            if (separatorIndex <= 0 || separatorIndex >= vejmidte.Length - 1)
            {
                throw new InvalidOperationException($"Ugyldigt vejmidte-format: \"{vejmidte}\".");
            }

            return (
                NormalizeCode(vejmidte.Substring(0, separatorIndex), 4),
                NormalizeCode(vejmidte.Substring(separatorIndex + 1), 4));
        }

        private static string NormalizeCode(string value, int length)
        {
            var padded = value.PadLeft(length, '0');
            return padded.Substring(padded.Length - length);
        }
    }
}
