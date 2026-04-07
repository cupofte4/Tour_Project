using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public class ProximityDistanceCalculator : IProximityDistanceCalculator
{
    private const double EarthRadiusMeters = 6_371_000;

    public double CalculateMeters(GeoPoint from, GeoPoint to)
    {
        var latitudeDeltaRadians = DegreesToRadians(to.Latitude - from.Latitude);
        var longitudeDeltaRadians = DegreesToRadians(to.Longitude - from.Longitude);
        var fromLatitudeRadians = DegreesToRadians(from.Latitude);
        var toLatitudeRadians = DegreesToRadians(to.Latitude);

        var a = Math.Pow(Math.Sin(latitudeDeltaRadians / 2), 2) +
                Math.Cos(fromLatitudeRadians) *
                Math.Cos(toLatitudeRadians) *
                Math.Pow(Math.Sin(longitudeDeltaRadians / 2), 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusMeters * c;
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180d;
    }
}
