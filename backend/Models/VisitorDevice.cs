using Microsoft.EntityFrameworkCore;

namespace Tour_Project.Models;

[Index(nameof(DeviceId), IsUnique = true)]
[Index(nameof(LastSeenAtUtc))]
public class VisitorDevice
{
    public long Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public DateTimeOffset FirstSeenAtUtc { get; set; }
    public DateTimeOffset LastSeenAtUtc { get; set; }
    public string LastPath { get; set; } = string.Empty;
    public string LastUserAgent { get; set; } = string.Empty;
}
