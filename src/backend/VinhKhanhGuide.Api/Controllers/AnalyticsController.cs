using Microsoft.AspNetCore.Mvc;
using VinhKhanhGuide.Application.Analytics;

namespace VinhKhanhGuide.Api.Controllers;

[ApiController]
[Route("api/analytics")]
public sealed class AnalyticsController(IAppUsageAnalyticsService analyticsService) : ControllerBase
{
    [HttpPost("events")]
    public async Task<IActionResult> RecordEvent(
        [FromBody] AppUsageEventIngestRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SessionId))
        {
            return BadRequest(new ValidationProblemDetails
            {
                Detail = "SessionId is required."
            });
        }

        if (!AppUsageEventType.IsValid(request.EventType))
        {
            return BadRequest(new ValidationProblemDetails
            {
                Detail = "EventType must be app_open, heartbeat, stall_view, or session_stopped."
            });
        }

        await analyticsService.RecordEventAsync(request, cancellationToken);
        return Accepted();
    }

    [HttpPost("heartbeat")]
    public async Task<IActionResult> RecordHeartbeat(
        [FromBody] AppUsageHeartbeatRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SessionId))
        {
            return BadRequest(new ValidationProblemDetails
            {
                Detail = "SessionId is required."
            });
        }

        await analyticsService.RecordHeartbeatAsync(request, cancellationToken);
        return Accepted();
    }
}
