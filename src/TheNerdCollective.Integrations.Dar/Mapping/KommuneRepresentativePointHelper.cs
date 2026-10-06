using System;
using System.Collections.Generic;
using System.Text.Json;
using TheNerdCollective.Integrations.Dar.Models;

namespace TheNerdCollective.Integrations.Dar.Mapping;

/// <summary>Haversine-afstand til kommunes repræsentativpunkt (WGS84).</summary>
internal static class KommuneRepresentativePointHelper
{
    private const double EarthRadiusMeters = 6_371_000;

    public static double HaversineDistanceMeters(
        double latitudeA,
        double longitudeA,
        double latitudeB,
        double longitudeB)
    {
        var lat1 = latitudeA * Math.PI / 180.0;
        var lat2 = latitudeB * Math.PI / 180.0;
        var deltaLat = (latitudeB - latitudeA) * Math.PI / 180.0;
        var deltaLon = (longitudeB - longitudeA) * Math.PI / 180.0;

        var sinHalfDeltaLat = Math.Sin(deltaLat / 2);
        var sinHalfDeltaLon = Math.Sin(deltaLon / 2);
        var a = sinHalfDeltaLat * sinHalfDeltaLat
            + Math.Cos(lat1) * Math.Cos(lat2) * sinHalfDeltaLon * sinHalfDeltaLon;
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusMeters * c;
    }

    public static double? TryGetDistanceToRepresentativeMeters(
        double queryLatitude,
        double queryLongitude,
        KommuneDto kommune)
    {
        if (kommune.RepræsentativPunktLatitude is not double latitude
            || kommune.RepræsentativPunktLongitude is not double longitude)
        {
            return null;
        }

        return HaversineDistanceMeters(queryLatitude, queryLongitude, latitude, longitude);
    }

    public static double? TryGetDistanceToRepresentativeMeters(
        double queryLatitude,
        double queryLongitude,
        string? geometryWkt)
    {
        var centroid = WktCentroidHelper.TryGetCentroidWgs84(geometryWkt);
        if (centroid is null)
        {
            return null;
        }

        return HaversineDistanceMeters(
            queryLatitude,
            queryLongitude,
            centroid.Value.Latitude,
            centroid.Value.Longitude);
    }

    public static KommuneDto? TryFindNearestByRepresentativePoint(
        double queryLatitude,
        double queryLongitude,
        IReadOnlyList<KommuneDto> kommuner,
        double maxDistanceMeters)
    {
        KommuneDto? nearest = null;
        var nearestDistance = double.MaxValue;

        foreach (var kommune in kommuner)
        {
            var distance = TryGetDistanceToRepresentativeMeters(queryLatitude, queryLongitude, kommune);
            if (distance is null || distance.Value > maxDistanceMeters)
            {
                continue;
            }

            if (distance.Value < nearestDistance)
            {
                nearestDistance = distance.Value;
                nearest = kommune;
            }
        }

        return nearest;
    }

    public static double? TryGetDistanceToGeoJsonFeatureRepresentativeMeters(
        double queryLatitude,
        double queryLongitude,
        JsonElement feature)
    {
        if (!feature.TryGetProperty("geometry", out var geometry)
            || geometry.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var centroidEtrs = TryGetGeoJsonCentroidEtrs89(geometry);
        if (centroidEtrs is null)
        {
            return null;
        }

        var (latitude, longitude) = Etrs89Utm32NConverter.ToWgs84(
            centroidEtrs.Value.Easting,
            centroidEtrs.Value.Northing);
        return HaversineDistanceMeters(queryLatitude, queryLongitude, latitude, longitude);
    }

    private static (double Easting, double Northing)? TryGetGeoJsonCentroidEtrs89(JsonElement geometry)
    {
        if (!geometry.TryGetProperty("type", out var typeElement)
            || !geometry.TryGetProperty("coordinates", out var coordinates))
        {
            return null;
        }

        var type = typeElement.GetString();
        if (string.Equals(type, "Polygon", StringComparison.OrdinalIgnoreCase))
        {
            return CentroidFromGeoJsonPolygon(coordinates);
        }

        if (string.Equals(type, "MultiPolygon", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var polygon in coordinates.EnumerateArray())
            {
                var centroid = CentroidFromGeoJsonPolygon(polygon);
                if (centroid is not null)
                {
                    return centroid;
                }
            }
        }

        return null;
    }

    private static (double Easting, double Northing)? CentroidFromGeoJsonPolygon(JsonElement polygonCoordinates)
    {
        if (polygonCoordinates.ValueKind != JsonValueKind.Array || polygonCoordinates.GetArrayLength() == 0)
        {
            return null;
        }

        var ring = polygonCoordinates[0];
        if (ring.ValueKind != JsonValueKind.Array || ring.GetArrayLength() == 0)
        {
            return null;
        }

        double sumX = 0;
        double sumY = 0;
        var count = 0;
        foreach (var point in ring.EnumerateArray())
        {
            if (point.ValueKind != JsonValueKind.Array || point.GetArrayLength() < 2)
            {
                continue;
            }

            sumX += point[0].GetDouble();
            sumY += point[1].GetDouble();
            count++;
        }

        if (count == 0)
        {
            return null;
        }

        return (sumX / count, sumY / count);
    }
}
