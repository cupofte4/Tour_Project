namespace VinhKhanhGuide.Mobile.Models;

public class StallDetail
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string DescriptionVi { get; init; } = string.Empty;

    public double Latitude { get; init; }

    public double Longitude { get; init; }

    public double TriggerRadiusMeters { get; init; }

    public int Priority { get; init; }

    public string OpenHours { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public string ImageUrl { get; init; } = string.Empty;

    public string DisplayImageSource => StallImageSourceResolver.Resolve(ImageUrl);

    public string MapLink { get; init; } = string.Empty;

    public string NarrationScriptVi { get; init; } = string.Empty;

    public string AudioUrl { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public decimal AverageRating { get; init; }

    public IReadOnlyList<StallTranslation> Translations { get; init; } = [];

    public string AverageRatingText => $"Rating: {AverageRating:0.0}";
}
