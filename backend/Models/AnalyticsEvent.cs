using Microsoft.EntityFrameworkCore;

namespace Tour_Project.Models;

[Index(nameof(DeviceId), nameof(CreatedAtUtc))]
[Index(nameof(EventType), nameof(CreatedAtUtc))]
public class AnalyticsEvent
{
    public long Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public int? LocationId { get; set; }
    public string Path { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
}
