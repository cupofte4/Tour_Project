namespace VinhKhanhGuide.Mobile.Models;

public sealed class AutoNarrationOptions
{
    public bool Enabled { get; set; } = true;

    public TimeSpan ReplayCooldown { get; set; } = TimeSpan.FromMinutes(3);
}
