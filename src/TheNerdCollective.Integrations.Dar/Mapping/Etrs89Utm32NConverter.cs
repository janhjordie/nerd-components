using System;

namespace TheNerdCollective.Integrations.Dar.Mapping
{
    /// <summary>Konverterer ETRS89 UTM zone 32N (EPSG:25832) til WGS84 bredde-/længdegrad.</summary>
    internal static class Etrs89Utm32NConverter
    {
        private const double SemiMajorAxis = 6378137.0;
        private const double Flattening = 1 / 298.257223563;
        private const double ScaleFactor = 0.9996;
        private const double FalseEasting = 500000.0;
        private const int ZoneNumber = 32;

        public static (double Latitude, double Longitude) ToWgs84(double easting, double northing)
        {
            var eccentricitySquared = 2 * Flattening - Flattening * Flattening;
            var eccentricityPrimeSquared = eccentricitySquared / (1 - eccentricitySquared);
            var eccentricity1 = (1 - Math.Sqrt(1 - eccentricitySquared)) / (1 + Math.Sqrt(1 - eccentricitySquared));

            var centralMeridianRadians = ((ZoneNumber - 1) * 6 - 180 + 3) * Math.PI / 180.0;
            var x = easting - FalseEasting;
            var y = northing;

            var meridionalArc = y / ScaleFactor;
            var footpointLatitude = meridionalArc / (SemiMajorAxis * (1 - eccentricitySquared / 4 - 3 * eccentricitySquared * eccentricitySquared / 64
                - 5 * eccentricitySquared * eccentricitySquared * eccentricitySquared / 256));

            footpointLatitude += (3 * eccentricity1 / 2 - 27 * eccentricity1 * eccentricity1 * eccentricity1 / 32) * Math.Sin(2 * footpointLatitude)
                + (21 * eccentricity1 * eccentricity1 / 16 - 55 * eccentricity1 * eccentricity1 * eccentricity1 * eccentricity1 / 32) * Math.Sin(4 * footpointLatitude)
                + (151 * eccentricity1 * eccentricity1 * eccentricity1 / 96) * Math.Sin(6 * footpointLatitude)
                + (1097 * eccentricity1 * eccentricity1 * eccentricity1 * eccentricity1 / 512) * Math.Sin(8 * footpointLatitude);

            var sinLatitude = Math.Sin(footpointLatitude);
            var cosLatitude = Math.Cos(footpointLatitude);
            var tanLatitude = Math.Tan(footpointLatitude);

            var radiusOfCurvature = SemiMajorAxis / Math.Sqrt(1 - eccentricitySquared * sinLatitude * sinLatitude);
            var tanLatitudeSquared = tanLatitude * tanLatitude;
            var eccentricityPrimeComponent = eccentricityPrimeSquared * cosLatitude * cosLatitude;
            var radiusOfCurvaturePrime = SemiMajorAxis * (1 - eccentricitySquared)
                / Math.Pow(1 - eccentricitySquared * sinLatitude * sinLatitude, 1.5);
            var eastingOverRadius = x / (radiusOfCurvature * ScaleFactor);

            var latitude = footpointLatitude
                - (radiusOfCurvature * tanLatitude / radiusOfCurvaturePrime)
                * (eastingOverRadius * eastingOverRadius / 2
                    - (5 + 3 * tanLatitudeSquared + 10 * eccentricityPrimeComponent - 4 * eccentricityPrimeComponent * eccentricityPrimeComponent
                        - 9 * eccentricityPrimeSquared) * Math.Pow(eastingOverRadius, 4) / 24
                    + (61 + 90 * tanLatitudeSquared + 298 * eccentricityPrimeComponent + 45 * tanLatitudeSquared * tanLatitudeSquared
                        - 252 * eccentricityPrimeSquared - 3 * eccentricityPrimeComponent * eccentricityPrimeComponent)
                    * Math.Pow(eastingOverRadius, 6) / 720);

            var longitude = centralMeridianRadians
                + (eastingOverRadius
                    - (1 + 2 * tanLatitudeSquared + eccentricityPrimeComponent) * Math.Pow(eastingOverRadius, 3) / 6
                    + (5 - 2 * eccentricityPrimeComponent + 28 * tanLatitudeSquared - 3 * eccentricityPrimeComponent * eccentricityPrimeComponent
                        + 8 * eccentricityPrimeSquared + 24 * tanLatitudeSquared * tanLatitudeSquared)
                    * Math.Pow(eastingOverRadius, 5) / 120)
                / cosLatitude;

            return (latitude * 180.0 / Math.PI, longitude * 180.0 / Math.PI);
        }
    }
}
