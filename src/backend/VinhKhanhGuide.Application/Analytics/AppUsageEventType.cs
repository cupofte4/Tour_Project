namespace VinhKhanhGuide.Application.Analytics;

public static class AppUsageEventType
{
    public const string AppOpen = "app_open";
    public const string Heartbeat = "heartbeat";
    public const string StallView = "stall_view";
    public const string SessionStopped = "session_stopped";

    public static bool IsValid(string? value)
    {
        return string.Equals(value, AppOpen, StringComparison.Ordinal) ||
               string.Equals(value, Heartbeat, StringComparison.Ordinal) ||
               string.Equals(value, StallView, StringComparison.Ordinal) ||
               string.Equals(value, SessionStopped, StringComparison.Ordinal);
    }
}
