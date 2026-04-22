using Microsoft.AspNetCore.Mvc;
using Tour_Project.Data;

namespace Tour_Project.Controllers
{
    [Route("api/manager/stats")]
    [ApiController]
    public class ManagerStatsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ManagerStatsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("locations")]
        public IActionResult GetLocationTotals([FromQuery] int managerId)
        {
            var locationIds = _context.LocationManagerAssignments
                .Where(item => item.AdminUserId == managerId)
                .Select(item => item.LocationId)
                .ToList();

            var totals = _context.LocationStats
                .Where(stat => locationIds.Contains(stat.LocationId))
                .GroupBy(stat => stat.LocationId)
                .Select(group => new
                {
                    locationId = group.Key,
                    viewsCount = group.Sum(item => item.ViewsCount),
                    audioPlaysCount = group.Sum(item => item.AudioPlaysCount)
                })
                .ToList();

            var locations = _context.Locations
                .Where(location => locationIds.Contains(location.Id))
                .Select(location => new { id = location.Id, name = location.Name })
                .ToList();

            var result = locations.Select(location =>
            {
                var total = totals.FirstOrDefault(item => item.locationId == location.id);
                return new
                {
                    locationId = location.id,
                    locationName = location.name,
                    viewsCount = total?.viewsCount ?? 0,
                    audioPlaysCount = total?.audioPlaysCount ?? 0
                };
            });

            return Ok(result);
        }

        [HttpGet("timeseries")]
        public IActionResult GetTimeSeries([FromQuery] int managerId, [FromQuery] int days = 30)
        {
            if (days < 1) days = 1;
            if (days > 365) days = 365;

            var locationIds = _context.LocationManagerAssignments
                .Where(item => item.AdminUserId == managerId)
                .Select(item => item.LocationId)
                .ToList();

            var fromDate = DateTime.UtcNow.Date.AddDays(-days + 1);

            var data = _context.LocationStats
                .Where(stat => locationIds.Contains(stat.LocationId) && stat.StatDate >= fromDate)
                .GroupBy(stat => stat.StatDate.Date)
                .Select(group => new
                {
                    date = group.Key,
                    viewsCount = group.Sum(item => item.ViewsCount),
                    audioPlaysCount = group.Sum(item => item.AudioPlaysCount)
                })
                .OrderBy(item => item.date)
                .ToList()
                .Select(item => new
                {
                    date = item.date.ToString("yyyy-MM-dd"),
                    viewsCount = item.viewsCount,
                    audioPlaysCount = item.audioPlaysCount
                });

            return Ok(data);
        }
    }
}

