namespace VinhKhanhGuide.Mobile.Services;

public interface IDeviceTextToSpeech
{
    Task SpeakAsync(string text, string? languageCode, string? localeCode, CancellationToken cancellationToken);
}

public sealed class TextToSpeechLocaleDiscoveryException(string message, Exception? innerException = null)
    : Exception(message, innerException);

public sealed class TextToSpeechLocaleUnavailableException(string message) : Exception(message);
