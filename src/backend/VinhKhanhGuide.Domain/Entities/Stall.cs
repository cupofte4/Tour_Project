namespace VinhKhanhGuide.Domain.Entities;

public class Stall
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string DescriptionVi { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double TriggerRadiusMeters { get; set; }

    public string Category { get; set; } = string.Empty;

    public string OpenHours { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public decimal AverageRating { get; set; }
}
