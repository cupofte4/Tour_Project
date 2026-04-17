namespace VinhKhanhGuide.Domain.Entities;

public class Stall
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string DescriptionVi { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double TriggerRadiusMeters { get; set; }

    public int Priority { get; set; }

    public string Category { get; set; } = string.Empty;

    public string OpenHours { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public string ImageUrlsJson { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string ReviewsJson { get; set; } = string.Empty;

    public string MapLink { get; set; } = string.Empty;

    public string NarrationScriptVi { get; set; } = string.Empty;

    public string AudioUrl { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public decimal AverageRating { get; set; }
}
