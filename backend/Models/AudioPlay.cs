namespace Tour_Project.Models;

public class AudioPlay
{
    public long Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public int LocationId { get; set; }
    public int? AudioId { get; set; }
    public DateTimeOffset OccurredAtUtc { get; set; }
}