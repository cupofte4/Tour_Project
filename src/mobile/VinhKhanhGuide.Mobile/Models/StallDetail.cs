namespace VinhKhanhGuide.Mobile.Models;

public class StallDetail
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string DescriptionVi { get; init; } = string.Empty;

    public string OpenHours { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public string ImageUrl { get; init; } = string.Empty;

    public decimal AverageRating { get; init; }

    public string AverageRatingText => $"Rating: {AverageRating:0.0}";
}
