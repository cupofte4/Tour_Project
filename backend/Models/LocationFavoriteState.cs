using Microsoft.EntityFrameworkCore;

namespace Tour_Project.Models;

[Index(nameof(DeviceId), nameof(LocationId), IsUnique = true)]
[Index(nameof(IsFavorited))]
public class LocationFavoriteState
{
    public long Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public int LocationId { get; set; }
    public bool IsFavorited { get; set; }
    public DateTimeOffset? FavoritedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
