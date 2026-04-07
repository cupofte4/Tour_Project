namespace VinhKhanhGuide.Application.Stalls;

public class StallDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string DescriptionVi { get; init; } = string.Empty;

    public double Latitude { get; init; }

    public double Longitude { get; init; }

    public double TriggerRadiusMeters { get; init; }

    public string Category { get; init; } = string.Empty;

    public string OpenHours { get; init; } = string.Empty;

    public string ImageUrl { get; init; } = string.Empty;

    public decimal AverageRating { get; init; }
}
