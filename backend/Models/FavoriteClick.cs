namespace Tour_Project.Models;

public class FavoriteClick
{
    public long Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public int LocationId { get; set; }
    public bool IsFavorite { get; set; }
    public DateTimeOffset OccurredAtUtc { get; set; }
}