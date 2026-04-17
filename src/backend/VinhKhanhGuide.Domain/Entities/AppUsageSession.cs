namespace VinhKhanhGuide.Domain.Entities;

public class AppUsageSession
{
    public string SessionId { get; set; } = string.Empty;

    public string AnonymousClientId { get; set; } = string.Empty;

    public string LastEventType { get; set; } = string.Empty;

    public DateTimeOffset FirstSeenAtUtc { get; set; }

    public DateTimeOffset LastSeenAtUtc { get; set; }

    public string AppVersion { get; set; } = string.Empty;

    public string Platform { get; set; } = string.Empty;
}
