namespace VinhKhanhGuide.Application.Stalls;

public class StallDistanceCalculator : IStallDistanceCalculator
{
    private const double EarthRadiusMeters = 6_371_000;

    public double CalculateMeters(double latitude1, double longitude1, double latitude2, double longitude2)
    {
        var latitudeDeltaRadians = DegreesToRadians(latitude2 - latitude1);
        var longitudeDeltaRadians = DegreesToRadians(longitude2 - longitude1);
        var startLatitudeRadians = DegreesToRadians(latitude1);
        var endLatitudeRadians = DegreesToRadians(latitude2);

        var a = Math.Pow(Math.Sin(latitudeDeltaRadians / 2), 2) +
                Math.Cos(startLatitudeRadians) *
                Math.Cos(endLatitudeRadians) *
                Math.Pow(Math.Sin(longitudeDeltaRadians / 2), 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusMeters * c;
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180d;
    }
}
