using Microsoft.EntityFrameworkCore;

namespace Tour_Project.Models;

[Index(nameof(DeviceId), nameof(OccurredAtUtc))]
public class AppUsageHeartbeat
{
    public long Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string AppVersion { get; set; } = string.Empty;
}
