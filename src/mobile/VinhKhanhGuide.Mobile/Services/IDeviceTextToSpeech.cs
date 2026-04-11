namespace VinhKhanhGuide.Mobile.Services;

public interface IDeviceTextToSpeech
{
    Task SpeakAsync(string text, string? languageCode, CancellationToken cancellationToken);
}
