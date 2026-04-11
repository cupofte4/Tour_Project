namespace VinhKhanhGuide.Application.Stalls;

public class StallDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string DescriptionVi { get; init; } = string.Empty;

    public double Latitude { get; init; }

    public double Longitude { get; init; }

    public double TriggerRadiusMeters { get; init; }

    public int Priority { get; init; }

    public string Category { get; init; } = string.Empty;

    public string OpenHours { get; init; } = string.Empty;

    public string ImageUrl { get; init; } = string.Empty;

    public IReadOnlyList<string> ImageUrls { get; init; } = [];

    public string Address { get; init; } = string.Empty;

    public string Phone { get; init; } = string.Empty;

    public string ReviewsJson { get; init; } = string.Empty;

    public string MapLink { get; init; } = string.Empty;

    public string NarrationScriptVi { get; init; } = string.Empty;

    public string AudioUrl { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public decimal AverageRating { get; init; }
}
