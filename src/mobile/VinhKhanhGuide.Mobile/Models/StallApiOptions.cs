namespace VinhKhanhGuide.Mobile.Models;

public class StallApiOptions
{
    public const string SectionName = "StallApi";
    public const string DemoCustomUrlScheme = "vinhkhanhguide";
    public const string LegacyCustomUrlScheme = "vkguide";

    public string BaseUrl { get; set; } = string.Empty;

    public static bool IsSupportedCustomScheme(string? scheme)
    {
        return string.Equals(scheme, DemoCustomUrlScheme, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(scheme, LegacyCustomUrlScheme, StringComparison.OrdinalIgnoreCase);
    }

    public static bool TryNormalizeBaseUrl(string? value, out string normalizedBaseUrl)
    {
        normalizedBaseUrl = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        normalizedBaseUrl = uri.AbsoluteUri.EndsWith("/", StringComparison.Ordinal)
            ? uri.AbsoluteUri
            : $"{uri.AbsoluteUri}/";

        return true;
    }
}
