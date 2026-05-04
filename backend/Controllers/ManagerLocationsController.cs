using Microsoft.AspNetCore.Mvc;
using Tour_Project.Data;
using Tour_Project.Models;

namespace Tour_Project.Controllers
{
    [Route("api/manager")]
    [ApiController]
    public class ManagerLocationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ManagerLocationsController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsLocationOwned(int managerId, int locationId)
        {
            return _context.LocationManagerAssignments.Any(item =>
                item.ManagerId == managerId && item.LocationId == locationId);
        }

        [HttpGet("locations")]
        public IActionResult GetMyLocations([FromQuery] int managerId)
        {
            var locations = _context.LocationManagerAssignments
                .Where(item => item.ManagerId == managerId)
                .Select(item => item.LocationId)
                .ToList();

            var result = _context.Locations
                .Where(location => locations.Contains(location.Id))
                .OrderByDescending(location => location.Id)
                .ToList();

            return Ok(result);
        }

        [HttpPost("locations")]
        public IActionResult CreateMyLocation([FromQuery] int managerId, Location location)
        {
            var manager = _context.AdminUsers.Find(managerId);
            if (manager == null)
            {
                return NotFound(new { message = "Manager not found" });
            }

            if (Roles.Normalize(manager.Role) != Roles.Manager)
            {
                return BadRequest(new { message = "User is not a manager" });
            }

            if (location == null)
            {
                return BadRequest(new { message = "Invalid location payload" });
            }

            if (!LocationPriority.IsValid(location.Prio))
            {
                return BadRequest(new { message = "Prio must be one of: Premium, Gold, Silver." });
            }

            location.ReviewsJson = string.IsNullOrWhiteSpace(location.ReviewsJson)
                ? "[]"
                : location.ReviewsJson;
            location.Prio = LocationPriority.NormalizeOrDefault(location.Prio);

            _context.Locations.Add(location);
            _context.SaveChanges();

            var assignment = new LocationManagerAssignment
            {
                ManagerId = managerId,
                LocationId = location.Id
            };

            _context.LocationManagerAssignments.Add(assignment);
            _context.SaveChanges();

            return Ok(location);
        }

        [HttpPut("locations/{id}")]
        public IActionResult UpdateMyLocation(int id, [FromQuery] int managerId, Location updated)
        {
            if (!IsLocationOwned(managerId, id))
                return Forbid();

            var location = _context.Locations.Find(id);
            if (location == null) return NotFound();

            if (!LocationPriority.IsValid(updated.Prio))
            {
                return BadRequest(new { message = "Prio must be one of: Premium, Gold, Silver." });
            }

            location.Name = updated.Name;
            location.Description = updated.Description;
            location.Image = updated.Image;
            location.Images = updated.Images;
            location.Address = updated.Address;
            location.Phone = updated.Phone;
            location.ReviewsJson = string.IsNullOrWhiteSpace(updated.ReviewsJson) ? "[]" : updated.ReviewsJson;
            location.Latitude = updated.Latitude;
            location.Longitude = updated.Longitude;
            location.TextVi = updated.TextVi;
            location.TextEn = updated.TextEn;
            location.TextZh = updated.TextZh;
            location.TextDe = updated.TextDe;
            location.Prio = updated.Prio;

            _context.SaveChanges();
            return Ok(location);
        }

        [HttpDelete("locations/{id}")]
        public IActionResult DeleteMyLocation(int id, [FromQuery] int managerId)
        {
            if (!IsLocationOwned(managerId, id))
                return Forbid();

            var location = _context.Locations.Find(id);
            if (location == null) return NotFound();

            _context.Locations.Remove(location);
            _context.SaveChanges();

            return Ok();
        }
    }
}
