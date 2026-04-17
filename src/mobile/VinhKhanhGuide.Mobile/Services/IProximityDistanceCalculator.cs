using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public interface IProximityDistanceCalculator
{
    double CalculateMeters(GeoPoint from, GeoPoint to);
}
