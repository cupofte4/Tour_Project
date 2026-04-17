using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public sealed class ExternalStallRouteParser : IExternalStallRouteParser
{
    private static readonly HashSet<string> SupportedLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        "vi",
        "en",
        "zh",
        "de"
    };

    public ExternalStallRoute Parse(string? rawPayload, ExternalEntrySourceType sourceType)
    {
        if (string.IsNullOrWhiteSpace(rawPayload))
        {
            return ExternalStallRoute.Invalid(rawPayload ?? string.Empty, sourceType, "Please enter a stall QR or deep-link payload.");
        }

        var normalizedPayload = rawPayload.Trim();

        if (TryParseShortCode(normalizedPayload, sourceType, out var shortCodeRoute))
        {
            return shortCodeRoute;
        }

        if (normalizedPayload.Contains("://", StringComparison.Ordinal))
        {
            return ParseUriPayload(normalizedPayload, sourceType);
        }

        return ExternalStallRoute.Invalid(normalizedPayload, sourceType, "That payload format is not supported.");
    }

    private static bool TryParseShortCode(
        string rawPayload,
        ExternalEntrySourceType sourceType,
        out ExternalStallRoute route)
    {
        route = ExternalStallRoute.Invalid(rawPayload, sourceType, "That payload format is not supported.");

        var separatorIndex = rawPayload.IndexOf(':');
        if (separatorIndex <= 0)
        {
            return false;
        }

        var prefix = rawPayload[..separatorIndex];
        if (!prefix.Equals("stall", StringComparison.OrdinalIgnoreCase) &&
            !prefix.Equals("poi", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var identifier = rawPayload[(separatorIndex + 1)..].Trim();
        if (!TryParseStallId(identifier, out var stallId, out var errorMessage))
        {
            route = ExternalStallRoute.Invalid(rawPayload, sourceType, errorMessage);
            return true;
        }

        route = new ExternalStallRoute
        {
            RawPayload = rawPayload,
            StallId = stallId,
            AutoPlay = false,
            RequestedLanguageCode = null,
            SourceType = sourceType,
            IsValid = true
        };

        return true;
    }

    private static ExternalStallRoute ParseUriPayload(string rawPayload, ExternalEntrySourceType sourceType)
    {
        if (!Uri.TryCreate(rawPayload, UriKind.Absolute, out var uri))
        {
            return ExternalStallRoute.Invalid(rawPayload, sourceType, "That deep link is malformed.");
        }

        if (!StallApiOptions.IsSupportedCustomScheme(uri.Scheme))
        {
            return ExternalStallRoute.Invalid(rawPayload, sourceType, "That deep link scheme is not supported.");
        }

        var routeName = uri.Host.Trim('/');
        var queryParameters = ParseQueryString(uri.Query);
        var pathSegments = uri.AbsolutePath
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (string.IsNullOrWhiteSpace(routeName) && pathSegments.Length > 0)
        {
            routeName = pathSegments[0];
            pathSegments = pathSegments.Skip(1).ToArray();
        }

        if (routeName.Equals("open", StringComparison.OrdinalIgnoreCase))
        {
            queryParameters.TryGetValue("stallId", out var stallIdValue);
            if (!TryParseStallId(stallIdValue ?? string.Empty, out var openStallId, out var openErrorMessage))
            {
                return ExternalStallRoute.Invalid(rawPayload, sourceType, openErrorMessage);
            }

            queryParameters.TryGetValue("lang", out var openRequestedLanguageCode);
            queryParameters.TryGetValue("autoplay", out var openAutoPlayValue);

            return new ExternalStallRoute
            {
                RawPayload = rawPayload,
                StallId = openStallId,
                AutoPlay = IsAutoPlayEnabled(openAutoPlayValue),
                RequestedLanguageCode = NormalizeSupportedLanguage(openRequestedLanguageCode),
                SourceType = sourceType,
                IsValid = true
            };
        }

        if (!routeName.Equals("stall", StringComparison.OrdinalIgnoreCase) &&
            !routeName.Equals("poi", StringComparison.OrdinalIgnoreCase))
        {
            return ExternalStallRoute.Invalid(rawPayload, sourceType, "That deep link route is not supported.");
        }

        if (pathSegments.Length == 0)
        {
            return ExternalStallRoute.Invalid(rawPayload, sourceType, "The deep link is missing a stall id.");
        }

        if (!TryParseStallId(pathSegments[0], out var stallId, out var errorMessage))
        {
            return ExternalStallRoute.Invalid(rawPayload, sourceType, errorMessage);
        }

        queryParameters.TryGetValue("lang", out var requestedLanguageCode);
        queryParameters.TryGetValue("autoplay", out var autoPlayValue);

        return new ExternalStallRoute
        {
            RawPayload = rawPayload,
            StallId = stallId,
            AutoPlay = IsAutoPlayEnabled(autoPlayValue),
            RequestedLanguageCode = NormalizeSupportedLanguage(requestedLanguageCode),
            SourceType = sourceType,
            IsValid = true
        };
    }

    private static bool TryParseStallId(string rawIdentifier, out int stallId, out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(rawIdentifier))
        {
            stallId = 0;
            errorMessage = "The payload is missing a stall id.";
            return false;
        }

        if (!int.TryParse(rawIdentifier, out stallId) || stallId <= 0)
        {
            stallId = 0;
            errorMessage = "The stall id must be a positive number.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    private static Dictionary<string, string> ParseQueryString(string query)
    {
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(query))
        {
            return parameters;
        }

        var trimmed = query.TrimStart('?');
        foreach (var pair in trimmed.Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separatorIndex = pair.IndexOf('=');
            if (separatorIndex < 0)
            {
                parameters[Uri.UnescapeDataString(pair)] = string.Empty;
                continue;
            }

            var key = Uri.UnescapeDataString(pair[..separatorIndex]);
            var value = Uri.UnescapeDataString(pair[(separatorIndex + 1)..]);
            parameters[key] = value;
        }

        return parameters;
    }

    private static bool IsAutoPlayEnabled(string? value)
    {
        return value is not null &&
               (value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("true", StringComparison.OrdinalIgnoreCase));
    }

    private static string? NormalizeSupportedLanguage(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim().ToLowerInvariant();
        return SupportedLanguages.Contains(normalized) ? normalized : null;
    }
}
