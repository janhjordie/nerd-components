using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace TheNerdCollective.Integrations.Dar.Mapping;

/// <summary>Beregner repræsentativt punkt (centroid) fra WKT POLYGON/MULTIPOLYGON.</summary>
internal static class WktCentroidHelper
{
    private static readonly Regex CoordinatePairPattern = new(
        @"(?<x>-?\d+(?:\.\d+)?)\s+(?<y>-?\d+(?:\.\d+)?)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static (double Easting, double Northing)? TryGetCentroidEtrs89(string? wkt)
    {
        if (string.IsNullOrWhiteSpace(wkt))
        {
            return null;
        }

        var normalized = wkt!.Trim();
        if (normalized.StartsWith("POINT", StringComparison.OrdinalIgnoreCase))
        {
            var point = ParseFirstRing(normalized);
            return point.Count > 0 ? point[0] : null;
        }

        if (normalized.StartsWith("POLYGON", StringComparison.OrdinalIgnoreCase))
        {
            return CentroidFromRing(ParseFirstRing(normalized));
        }

        if (normalized.StartsWith("MULTIPOLYGON", StringComparison.OrdinalIgnoreCase))
        {
            var rings = ParseMultiPolygonRings(normalized);
            if (rings.Count == 0)
            {
                return null;
            }

            var largest = rings.OrderByDescending(r => r.Count).First();
            return CentroidFromRing(largest);
        }

        return null;
    }

    public static (double Latitude, double Longitude)? TryGetCentroidWgs84(string? wkt)
    {
        var centroid = TryGetCentroidEtrs89(wkt);
        return centroid is null
            ? null
            : Etrs89Utm32NConverter.ToWgs84(centroid.Value.Easting, centroid.Value.Northing);
    }

    private static (double Easting, double Northing)? CentroidFromRing(IReadOnlyList<(double Easting, double Northing)> ring)
    {
        if (ring.Count == 0)
        {
            return null;
        }

        if (ring.Count == 1)
        {
            return ring[0];
        }

        double area = 0;
        double centroidX = 0;
        double centroidY = 0;

        for (var index = 0; index < ring.Count - 1; index++)
        {
            var (x0, y0) = ring[index];
            var (x1, y1) = ring[index + 1];
            var cross = x0 * y1 - x1 * y0;
            area += cross;
            centroidX += (x0 + x1) * cross;
            centroidY += (y0 + y1) * cross;
        }

        if (Math.Abs(area) < double.Epsilon)
        {
            var averageX = ring.Average(p => p.Easting);
            var averageY = ring.Average(p => p.Northing);
            return (averageX, averageY);
        }

        area *= 0.5;
        return (centroidX / (6 * area), centroidY / (6 * area));
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
