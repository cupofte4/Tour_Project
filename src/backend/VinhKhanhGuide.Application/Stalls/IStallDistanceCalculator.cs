namespace VinhKhanhGuide.Application.Stalls;

public interface IStallDistanceCalculator
{
    double CalculateMeters(double latitude1, double longitude1, double latitude2, double longitude2);
}
