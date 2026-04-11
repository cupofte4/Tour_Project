namespace VinhKhanhGuide.Application.RemoteLocations;

public sealed class RemoteLocationRecord
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string Image { get; init; } = string.Empty;

    public string Images { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;

    public string Phone { get; init; } = string.Empty;

    public string ReviewsJson { get; init; } = string.Empty;

    public double Latitude { get; init; }

    public double Longitude { get; init; }

    public string TextVi { get; init; } = string.Empty;

    public string TextEn { get; init; } = string.Empty;

    public string TextZh { get; init; } = string.Empty;

    public string TextDe { get; init; } = string.Empty;
}
