using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TheNerdCollective.Integrations.Dar.Mapping;

/// <summary>Point-in-polygon tests for DAGI geometries in EPSG:25832 (ETRS89 UTM zone 32N).</summary>
internal static class WktPointInPolygonHelper
{
    private static readonly Regex CoordinatePairPattern = new(
        @"(?<x>-?\d+(?:\.\d+)?)\s+(?<y>-?\d+(?:\.\d+)?)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static bool ContainsEtrs89(string? wkt, double easting, double northing)
    {
        if (string.IsNullOrWhiteSpace(wkt))
        {
            return false;
        }

        var normalized = wkt.Trim();
        if (normalized.StartsWith("POLYGON", StringComparison.OrdinalIgnoreCase))
        {
            return ContainsInPolygon(normalized, easting, northing);
        }

        if (normalized.StartsWith("MULTIPOLYGON", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var ring in ParseMultiPolygonRings(normalized))
            {
                if (ContainsInRing(ring, easting, northing))
                {
                    return true;
                }
            }

            return false;
        }

        return false;
    }

    public static bool GeoJsonFeatureContainsEtrs89(JsonElement feature, double easting, double northing)
    {
        if (!feature.TryGetProperty("geometry", out var geometry)
            || geometry.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (!geometry.TryGetProperty("type", out var typeElement))
        {
            return false;
        }

        var type = typeElement.GetString();
        if (!geometry.TryGetProperty("coordinates", out var coordinates))
        {
            return false;
        }

        if (string.Equals(type, "Polygon", StringComparison.OrdinalIgnoreCase))
        {
            return GeoJsonPolygonContains(coordinates, easting, northing);
        }

        if (string.Equals(type, "MultiPolygon", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var polygon in coordinates.EnumerateArray())
            {
                if (GeoJsonPolygonContains(polygon, easting, northing))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static double TryGetAbsoluteAreaEtrs89(string? wkt)
    {
        if (string.IsNullOrWhiteSpace(wkt))
        {
            return double.MaxValue;
        }

        var normalized = wkt.Trim();
        if (normalized.StartsWith("POLYGON", StringComparison.OrdinalIgnoreCase))
        {
            return Math.Abs(ShoelaceArea(ParseFirstRing(normalized)));
        }

        if (normalized.StartsWith("MULTIPOLYGON", StringComparison.OrdinalIgnoreCase))
        {
            return ParseMultiPolygonRings(normalized)
                .Select(ring => Math.Abs(ShoelaceArea(ring)))
                .DefaultIfEmpty(double.MaxValue)
                .Min();
        }

        return double.MaxValue;
    }

    private static bool GeoJsonPolygonContains(JsonElement polygonCoordinates, double easting, double northing)
    {
        if (polygonCoordinates.ValueKind != JsonValueKind.Array || polygonCoordinates.GetArrayLength() == 0)
        {
            return false;
        }

        var outerRing = polygonCoordinates[0];
        var ring = ParseGeoJsonRing(outerRing);
        return ContainsInRing(ring, easting, northing);
    }

    private static List<(double Easting, double Northing)> ParseGeoJsonRing(JsonElement ringCoordinates)
    {
        var points = new List<(double Easting, double Northing)>();
        if (ringCoordinates.ValueKind != JsonValueKind.Array)
        {
            return points;
        }

        foreach (var coordinate in ringCoordinates.EnumerateArray())
        {
            if (coordinate.ValueKind != JsonValueKind.Array || coordinate.GetArrayLength() < 2)
            {
                continue;
            }

            points.Add((
                coordinate[0].GetDouble(),
                coordinate[1].GetDouble()));
        }

        return points;
    }

    private static bool ContainsInPolygon(string polygonWkt, double easting, double northing)
    {
        return ContainsInRing(ParseFirstRing(polygonWkt), easting, northing);
    }

    private static bool ContainsInRing(IReadOnlyList<(double Easting, double Northing)> ring, double easting, double northing)
    {
        if (ring.Count < 3)
        {
            return false;
        }

        var inside = false;
        for (int i = 0, j = ring.Count - 1; i < ring.Count; j = i++)
        {
            var pointI = ring[i];
            var pointJ = ring[j];
            var xi = pointI.Easting;
            var yi = pointI.Northing;
            var xj = pointJ.Easting;
            var yj = pointJ.Northing;

            var intersects = yi > northing != yj > northing
                && easting < ((xj - xi) * (northing - yi) / ((yj - yi) + double.Epsilon)) + xi;

            if (intersects)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private static double ShoelaceArea(IReadOnlyList<(double Easting, double Northing)> ring)
    {
        if (ring.Count < 3)
        {
            return 0;
        }

        double area = 0;
        for (var index = 0; index < ring.Count - 1; index++)
        {
            var (x0, y0) = ring[index];
            var (x1, y1) = ring[index + 1];
            area += x0 * y1 - x1 * y0;
        }

        return area * 0.5;
    }

    private static List<(double Easting, double Northing)> ParseFirstRing(string wkt)
    {
        var openIndex = wkt.IndexOf("((", StringComparison.Ordinal);
        if (openIndex < 0)
        {
            return ParseCoordinatePairs(wkt);
        }

        var closeIndex = wkt.IndexOf("))", openIndex + 2, StringComparison.Ordinal);
        var body = closeIndex > openIndex
            ? wkt.Substring(openIndex + 2, closeIndex - openIndex - 2)
            : wkt.Substring(openIndex + 2);

        var firstRing = body.Split(new[] { "), (" }, StringSplitOptions.None)[0];
        return ParseCoordinatePairs(firstRing);
    }

    private static List<List<(double Easting, double Northing)>> ParseMultiPolygonRings(string wkt)
    {
        var rings = new List<List<(double Easting, double Northing)>>();
        var openIndex = wkt.IndexOf("(((", StringComparison.Ordinal);
        if (openIndex < 0)
        {
            rings.Add(ParseFirstRing(wkt));
            return rings;
        }

        var bodyStart = openIndex + 3;
        var bodyEnd = wkt.LastIndexOf(")))", StringComparison.Ordinal);
        if (bodyEnd <= bodyStart)
        {
            rings.Add(ParseFirstRing(wkt));
            return rings;
        }

        var body = wkt.Substring(bodyStart, bodyEnd - bodyStart);
        foreach (var polygon in body.Split(new[] { ")), ((" }, StringSplitOptions.None))
        {
            rings.Add(ParseCoordinatePairs(polygon));
        }

        return rings;
    }

    private static List<(double Easting, double Northing)> ParseCoordinatePairs(string text)
    {
        var points = new List<(double Easting, double Northing)>();
        foreach (Match match in CoordinatePairPattern.Matches(text))
        {
            points.Add((
                double.Parse(match.Groups["x"].Value, CultureInfo.InvariantCulture),
                double.Parse(match.Groups["y"].Value, CultureInfo.InvariantCulture)));
        }

        return points;
    }
}
