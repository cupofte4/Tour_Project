using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using VinhKhanhGuide.Application.Analytics;
using Tour_Project.Models;

namespace Tour_Project.Controllers
{
    [ApiController]
    [Route("api/analytics")]
    public sealed class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analytics;

        public AnalyticsController(IAnalyticsService analytics)
        {
            _analytics = analytics;
        }

        [HttpPost("heartbeat")]
        public async Task<IActionResult> Heartbeat([FromBody] AppUsageHeartbeatRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.DeviceId))
                return BadRequest(new ValidationProblemDetails { Detail = "DeviceId is required." });

            await _analytics.RecordHeartbeatAsync(request, cancellationToken);
            return Accepted();
        }

        [HttpPost("audio-plays")]
        public async Task<IActionResult> AudioPlays([FromBody] AudioPlayRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.DeviceId))
                return BadRequest(new ValidationProblemDetails { Detail = "DeviceId is required." });

            if (request.LocationId <= 0)
                return BadRequest(new ValidationProblemDetails { Detail = "LocationId is required." });

            await _analytics.RecordAudioPlayAsync(request, cancellationToken);
            return Accepted();
        }

        [HttpPost("favorite-clicks")]
        public async Task<IActionResult> FavoriteClicks([FromBody] FavoriteClickRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.DeviceId))
                return BadRequest(new ValidationProblemDetails { Detail = "DeviceId is required." });

            if (request.LocationId <= 0)
                return BadRequest(new ValidationProblemDetails { Detail = "LocationId is required." });

            await _analytics.RecordFavoriteClickAsync(request, cancellationToken);
            return Accepted();
        }
    }
}
