namespace VinhKhanhGuide.Mobile.Services;

public static class NarrationAudioSourceResolver
{
    public static string ResolvePreferredAudioSource(string? localAudioPath, string? remoteAudioUrl)
    {
        if (TryGetValidLocalAudioPath(localAudioPath, out var validLocalAudioPath))
        {
            return validLocalAudioPath;
        }

        return remoteAudioUrl?.Trim() ?? string.Empty;
    }

    public static bool TryGetValidLocalAudioPath(string? localAudioPath, out string validLocalAudioPath)
    {
        if (!string.IsNullOrWhiteSpace(localAudioPath))
        {
            var trimmedPath = localAudioPath.Trim();
            if (File.Exists(trimmedPath))
            {
                validLocalAudioPath = trimmedPath;
                return true;
            }
        }

        validLocalAudioPath = string.Empty;
        return false;
    }
}
