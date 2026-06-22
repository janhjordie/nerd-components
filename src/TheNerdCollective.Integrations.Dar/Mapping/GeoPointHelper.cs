using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TheNerdCollective.Integrations.Dar.Mapping;

/// <summary>Geometri-hjælpere til afstand og punkt-i-cirkel (EPSG:25832).</summary>
internal static class GeoPointHelper
{
    private static readonly Regex PointPattern = new(
        @"POINT\s*\(\s*(?<x>-?\d+(?:\.\d+)?)\s+(?<y>-?\d+(?:\.\d+)?)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    public static bool TryParseEtrs89Point(string? wkt, out double easting, out double northing)
    {
        easting = 0;
        northing = 0;

        if (string.IsNullOrWhiteSpace(wkt))
        {
            return false;
        }

        var match = PointPattern.Match(wkt);
        if (!match.Success)
        {
            return false;
        }

        easting = double.Parse(match.Groups["x"].Value, CultureInfo.InvariantCulture);
        northing = double.Parse(match.Groups["y"].Value, CultureInfo.InvariantCulture);
        return true;
    }

    public static double SquaredDistanceEtrs89(
        double eastingA,
        double northingA,
        double eastingB,
        double northingB)
    {
        var deltaX = eastingA - eastingB;
        var deltaY = northingA - northingB;
        return deltaX * deltaX + deltaY * deltaY;
    }

    public static bool IsWithinCircleEtrs89(
        double easting,
        double northing,
        double centerEasting,
        double centerNorthing,
        int radiusMeters)
    {
        var radiusSquared = (double)radiusMeters * radiusMeters;
        return SquaredDistanceEtrs89(easting, northing, centerEasting, centerNorthing) <= radiusSquared;
    }
}
