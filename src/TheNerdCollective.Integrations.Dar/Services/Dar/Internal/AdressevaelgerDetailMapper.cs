using System;
using System.Text.Json;
using TheNerdCollective.Integrations.Dar.Mapping;
using TheNerdCollective.Integrations.Dar.Models;

namespace TheNerdCollective.Integrations.Dar.Services.Dar.Internal
{
    internal static class AdressevaelgerDetailMapper
    {
        internal static DanishAddressDetailResult FromHusnummer(JsonElement husnummer)
        {
            var adgangspunkt = husnummer.GetProperty("adgangspunkt");
            var (easting, northing, crs) = ReadCoordinates(adgangspunkt);
            var (latitude, longitude) = Etrs89Utm32NConverter.ToWgs84(easting, northing);
            var postnummer = TryGetProperty(husnummer, "postnummer");

            return new DanishAddressDetailResult
            {
                HusnummerId = husnummer.GetProperty("id_lokalid").GetString() ?? string.Empty,
                Betegnelse = husnummer.GetProperty("adgangsadressebetegnelse").GetString() ?? string.Empty,
                Vejnavn = husnummer.GetProperty("vejnavn").GetString() ?? string.Empty,
                Husnummer = husnummer.GetProperty("husnummertekst").GetString() ?? string.Empty,
                Postnummer = postnummer?.GetProperty("postnr").GetString() ?? string.Empty,
                Postdistrikt = postnummer?.GetProperty("navn").GetString() ?? string.Empty,
                Kommunekode = TryGetProperty(husnummer, "navngivenvejkommunedel")?.GetProperty("kommune").GetString(),
                Easting = easting,
                Northing = northing,
                Latitude = latitude,
                Longitude = longitude,
                CoordinateSystem = crs
            };
        }

        internal static DanishAddressDetailResult FromAdresse(JsonElement adresse)
        {
            var husnummer = adresse.GetProperty("husnummer");
            var result = FromHusnummer(husnummer);

            return result with
            {
                AdresseId = adresse.GetProperty("id_lokalid").GetString(),
                Betegnelse = adresse.GetProperty("adressebetegnelse").GetString() ?? result.Betegnelse,
                Etagebetegnelse = ReadOptionalString(adresse, "etagebetegnelse"),
                Doerbetegnelse = ReadOptionalString(adresse, "doerbetegnelse")
            };
        }

        private static (double Easting, double Northing, string Crs) ReadCoordinates(JsonElement adgangspunkt)
        {
            if (adgangspunkt.TryGetProperty("koordinater", out var koordinater)
                && koordinater.TryGetProperty("x", out var x)
                && koordinater.TryGetProperty("y", out var y))
            {
                return (x.GetDouble(), y.GetDouble(), ReadCrs(adgangspunkt));
            }

            if (adgangspunkt.TryGetProperty("geometri", out var geometri)
                && geometri.TryGetProperty("coordinates", out var coordinates)
                && coordinates.GetArrayLength() >= 2)
            {
                return (coordinates[0].GetDouble(), coordinates[1].GetDouble(), ReadCrs(adgangspunkt));
            }

            throw new InvalidOperationException("Adressevælger returnerede intet adgangspunkt med koordinater.");
        }

        private static string ReadCrs(JsonElement adgangspunkt)
        {
            if (adgangspunkt.TryGetProperty("geometri", out var geometri)
                && geometri.TryGetProperty("crs", out var crs)
                && crs.TryGetProperty("properties", out var properties)
                && properties.TryGetProperty("name", out var name))
            {
                return name.GetString() ?? "EPSG:25832";
            }

            return "EPSG:25832";
        }

        private static JsonElement? TryGetProperty(JsonElement element, string propertyName) =>
            element.TryGetProperty(propertyName, out var value) ? value : null;

        private static string? ReadOptionalString(JsonElement element, string propertyName) =>
            element.TryGetProperty(propertyName, out var value) && value.ValueKind != JsonValueKind.Null
                ? value.GetString()
                : null;
    }
}
