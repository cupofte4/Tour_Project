using VinhKhanhGuide.Mobile.Models;

namespace VinhKhanhGuide.Mobile.Services;

public interface IExternalStallRouteParser
{
    ExternalStallRoute Parse(string? rawPayload, ExternalEntrySourceType sourceType);
}
