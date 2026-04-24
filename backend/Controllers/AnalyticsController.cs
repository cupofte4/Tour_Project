using Microsoft.AspNetCore.Mvc;
using VinhKhanhGuide.Application.Analytics;

namespace Tour_Project.Controllers
{
    [ApiController]
    [Route("api/analytics")]
    public sealed class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analytics;
        private readonly IAudioPlayEventQueue _audioPlayEventQueue;

        public AnalyticsController(IAnalyticsService analytics, IAudioPlayEventQueue audioPlayEventQueue)
        {
            _analytics = analytics;
            _audioPlayEventQueue = audioPlayEventQueue;
        }

        [HttpPost("heartbeat")]
        public async Task<IActionResult> Heartbeat([FromBody] AppUsageHeartbeatRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.DeviceId))
                return BadRequest(new ValidationProblemDetails { Detail = "DeviceId is required." });

            var enrichedRequest = new AppUsageHeartbeatRequest
            {
                SessionId = request.SessionId,
                DeviceId = request.DeviceId,
                OccurredAtUtc = request.OccurredAtUtc,
                Platform = request.Platform,
                AppVersion = request.AppVersion,
                Path = request.Path,
                EventType = request.EventType,
                UserAgent = Request.Headers["User-Agent"].ToString()
            };

            await _analytics.RecordHeartbeatAsync(enrichedRequest, cancellationToken);
            return Accepted(new { accepted = true });
        }

        [HttpPost("event")]
        public async Task<IActionResult> Event([FromBody] AnalyticsEventRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.DeviceId))
                return BadRequest(new ValidationProblemDetails { Detail = "DeviceId is required." });

            if (string.IsNullOrWhiteSpace(request.EventType))
                return BadRequest(new ValidationProblemDetails { Detail = "EventType is required." });

            await _analytics.RecordEventAsync(request, cancellationToken);
            return Accepted(new { accepted = true });
        }

        [HttpPost("audio-plays")]
        public async Task<IActionResult> AudioPlays([FromBody] AudioPlayRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.DeviceId))
                return BadRequest(new ValidationProblemDetails { Detail = "DeviceId is required." });

            if (request.LocationId <= 0)
                return BadRequest(new ValidationProblemDetails { Detail = "LocationId is required." });

            await _audioPlayEventQueue.EnqueueAsync(new AudioPlayEvent
            {
                DeviceId = request.DeviceId,
                LocationId = request.LocationId,
                AudioId = request.AudioId,
                OccurredAtUtc = request.OccurredAtUtc
            }, cancellationToken);

            return Accepted(new { accepted = true });
        }

        [HttpPost("favorite-clicks")]
        public async Task<IActionResult> FavoriteClicks([FromBody] FavoriteClickRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.DeviceId))
                return BadRequest(new ValidationProblemDetails { Detail = "DeviceId is required." });

            if (request.LocationId <= 0)
                return BadRequest(new ValidationProblemDetails { Detail = "LocationId is required." });

            await _analytics.RecordFavoriteClickAsync(request, cancellationToken);
            return Accepted(new { accepted = true });
        }
    }
}
