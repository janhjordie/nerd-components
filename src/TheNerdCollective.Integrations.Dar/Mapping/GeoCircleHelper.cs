using System;
using System.Collections.Generic;
using System.Globalization;

namespace TheNerdCollective.Integrations.Dar.Mapping;

/// <summary>Approximerer en cirkel som polygon i ETRS89 UTM zone 32N (EPSG:25832).</summary>
internal static class GeoCircleHelper
{
    public static string CreateCirclePolygonWkt(
        double longitude,
        double latitude,
        int radiusMeters,
        int segments = 32)
    {
        if (radiusMeters <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(radiusMeters), radiusMeters, "Radius skal være positiv.");
        }

        if (segments < 8)
        {
            throw new ArgumentOutOfRangeException(nameof(segments), segments, "Mindst 8 segmenter kræves.");
        }

        var (centerEasting, centerNorthing) = Etrs89Utm32NConverter.FromWgs84(latitude, longitude);
        var ring = new List<string>(segments + 1);

        for (var index = 0; index <= segments; index++)
        {
            var angle = 2 * Math.PI * index / segments;
            var easting = centerEasting + radiusMeters * Math.Sin(angle);
            var northing = centerNorthing + radiusMeters * Math.Cos(angle);
            ring.Add(FormatCoordinate(easting, northing));
        }

        return $"POLYGON(({string.Join(", ", ring)}))";
    }

    private static string FormatCoordinate(double easting, double northing) =>
        $"{easting.ToString("0.########", CultureInfo.InvariantCulture)} {northing.ToString("0.########", CultureInfo.InvariantCulture)}";
}
