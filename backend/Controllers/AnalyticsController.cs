using Microsoft.AspNetCore.Mvc;
using Tour_Project.Data;
using Tour_Project.Models;

namespace Tour_Project.Controllers
{
    [Route("api/analytics")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AnalyticsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("event")]
        public IActionResult RecordEvent(RecordLocationStatRequest request)
        {
            if (request == null || request.LocationId <= 0)
                return BadRequest(new { message = "LocationId is required." });

            var location = _context.Locations.Find(request.LocationId);
            if (location == null)
                return NotFound(new { message = "Location not found." });

            var type = (request.EventType ?? string.Empty).Trim().ToLowerInvariant();
            if (type != "view" && type != "audio_play")
                return BadRequest(new { message = "Invalid event type." });

            var count = request.Count <= 0 ? 1 : request.Count;
            var statDate = DateTime.UtcNow.Date;

            var stat = _context.LocationStats.FirstOrDefault(s => s.LocationId == request.LocationId && s.StatDate == statDate);
            if (stat == null)
            {
                stat = new LocationStat
                {
                    LocationId = request.LocationId,
                    StatDate = statDate,
                    ViewsCount = 0,
                    AudioPlaysCount = 0,
                };
                _context.LocationStats.Add(stat);
            }

            if (type == "view")
            {
                stat.ViewsCount += count;
            }
            else
            {
                stat.AudioPlaysCount += count;
            }

            _context.SaveChanges();
            return Ok(new
            {
                locationId = stat.LocationId,
                statDate = stat.StatDate.ToString("yyyy-MM-dd"),
                viewsCount = stat.ViewsCount,
                audioPlaysCount = stat.AudioPlaysCount,
            });
        }
    }

    public class RecordLocationStatRequest
    {
        public int LocationId { get; set; }
        public string? EventType { get; set; }
        public int Count { get; set; } = 1;
    }
}
