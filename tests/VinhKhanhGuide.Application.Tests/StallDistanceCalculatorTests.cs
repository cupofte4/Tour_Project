using VinhKhanhGuide.Application.Stalls;

namespace VinhKhanhGuide.Application.Tests;

public class StallDistanceCalculatorTests
{
    private readonly StallDistanceCalculator _calculator = new();

    [Fact]
    public void CalculateMeters_ReturnsZero_ForSameCoordinate()
    {
        var distance = _calculator.CalculateMeters(10.76042, 106.69321, 10.76042, 106.69321);

        Assert.Equal(0d, distance, precision: 6);
    }

    [Fact]
    public void CalculateMeters_IsSymmetric()
    {
        var forward = _calculator.CalculateMeters(10.76042, 106.69321, 10.76088, 106.69402);
        var backward = _calculator.CalculateMeters(10.76088, 106.69402, 10.76042, 106.69321);

        Assert.Equal(forward, backward, precision: 6);
    }

    [Fact]
    public void CalculateMeters_ReturnsExpectedApproximateDistance()
    {
        var distance = _calculator.CalculateMeters(10.76042, 106.69321, 10.76088, 106.69402);

        Assert.InRange(distance, 100, 110);
    }
}
