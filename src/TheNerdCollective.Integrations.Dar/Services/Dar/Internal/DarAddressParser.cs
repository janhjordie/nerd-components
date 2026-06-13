using System;
using System.Linq;
using System.Text.RegularExpressions;
using TheNerdCollective.Integrations.Dar.Compatibility;

namespace TheNerdCollective.Integrations.Dar.Services.Dar.Internal
{
    internal static class DarAddressParser
    {
        private static readonly Regex PostalCityRegex = new Regex(
            @"^(?<postnr>\d{4})\s*(?<city>.*)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        internal static ParsedAddress Parse(string fullAddress)
        {
            if (string.IsNullOrWhiteSpace(fullAddress))
            {
                throw new ArgumentException("Adresse må ikke være tom.", nameof(fullAddress));
            }

            var segments = StringCompat.SplitTrimmed(fullAddress, ',');
            if (segments.Length < 2)
            {
                throw new ArgumentException(
                    $"Ugyldig adresse: \"{fullAddress}\". Forventet format: \"Vej 1, 1234 By\".",
                    nameof(fullAddress));
            }

            var streetAndNumber = segments[0];
            var postalSegment = segments[1];
            var city = segments.Length > 2
                ? string.Join(", ", segments.Skip(2))
                : null;

            var postalMatch = PostalCityRegex.Match(postalSegment);
            if (!postalMatch.Success)
            {
                throw new ArgumentException(
                    $"Ugyldigt postnummer i adresse: \"{fullAddress}\".",
                    nameof(fullAddress));
            }

            var parsedCity = postalMatch.Groups["city"].Value.Trim();
            if (string.IsNullOrWhiteSpace(parsedCity))
            {
                parsedCity = city ?? string.Empty;
            }

            return new ParsedAddress(
                streetAndNumber,
                postalMatch.Groups["postnr"].Value,
                string.IsNullOrWhiteSpace(parsedCity) ? null : parsedCity);
        }

        internal sealed class ParsedAddress
        {
            public ParsedAddress(string streetAndNumber, string postalCode, string? city)
            {
                StreetAndNumber = streetAndNumber;
                PostalCode = postalCode;
                City = city;
            }

            public string StreetAndNumber { get; }

            public string PostalCode { get; }

            public string? City { get; }
        }
    }
}
