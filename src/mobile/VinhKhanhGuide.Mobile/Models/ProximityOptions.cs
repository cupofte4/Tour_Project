namespace VinhKhanhGuide.Mobile.Models;

public class ProximityOptions
{
    public TimeSpan TriggerDebounce { get; set; } = TimeSpan.FromSeconds(5);

    public TimeSpan TriggerCooldown { get; set; } = TimeSpan.FromMinutes(3);
}
