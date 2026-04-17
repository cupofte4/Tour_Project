using Microsoft.AspNetCore.Mvc;
using VinhKhanhGuide.Application.RemoteLocations;
using VinhKhanhGuide.Application.Stalls;
using VinhKhanhGuide.Application.Translations;

namespace VinhKhanhGuide.Api.Controllers;

[ApiController]
[Route("api/stalls")]
public class StallsController(
    IStallReadService stallReadService,
    IStallTranslationService stallTranslationService,
    IRemoteLocationSyncService remoteLocationSyncService) : ControllerBase
{
    private static readonly HashSet<string> SupportedTranslationLanguages = ["vi", "en", "zh", "de"];

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StallDto>>> GetAll(CancellationToken cancellationToken)
    {
        var stalls = await stallReadService.GetAllAsync(cancellationToken);

        return Ok(stalls);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StallDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var stall = await stallReadService.GetByIdAsync(id, cancellationToken);

        return stall is null ? NotFound() : Ok(stall);
    }

    [HttpGet("nearby")]
    public async Task<ActionResult<IReadOnlyList<NearbyStallDto>>> GetNearby(
        [FromQuery] double lat,
        [FromQuery] double lng,
        [FromQuery] double radius,
        CancellationToken cancellationToken)
    {
        if (lat is < -90 or > 90)
        {
            return BadRequest("lat must be between -90 and 90.");
        }

        if (lng is < -180 or > 180)
        {
            return BadRequest("lng must be between -180 and 180.");
        }

        if (radius <= 0)
        {
            return BadRequest("radius must be greater than 0.");
        }

        var stalls = await stallReadService.GetNearbyAsync(new NearbyStallQueryDto
        {
            Latitude = lat,
            Longitude = lng,
            RadiusMeters = radius
        }, cancellationToken);

        return Ok(stalls);
    }

    [HttpGet("{id:int}/translation")]
    public async Task<ActionResult<StallTranslationDto>> GetTranslation(int id, [FromQuery] string lang, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(lang))
        {
            return BadRequest("lang is required.");
        }

        var normalizedLanguage = lang.Trim().ToLowerInvariant();

        if (normalizedLanguage.Contains('-'))
        {
            normalizedLanguage = normalizedLanguage.Split('-', 2)[0];
        }

        if (!SupportedTranslationLanguages.Contains(normalizedLanguage))
        {
            return BadRequest("lang must be one of: vi, en, zh, de.");
        }

        var translation = await stallTranslationService.GetTranslationAsync(id, normalizedLanguage, cancellationToken);

        return translation is null ? NotFound() : Ok(translation);
    }

    [HttpPost("sync/locations")]
    public async Task<ActionResult<RemoteLocationSyncResult>> SyncLocations(CancellationToken cancellationToken)
    {
        var result = await remoteLocationSyncService.SyncAsync(cancellationToken);
        return Ok(result);
    }
}
