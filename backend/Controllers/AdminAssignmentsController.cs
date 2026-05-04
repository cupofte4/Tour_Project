using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tour_Project.Data;
using Tour_Project.Models;

namespace Tour_Project.Controllers
{
    [Route("api/location-manager-assignments")]
    [Route("api/admin")]
    [ApiController]
    public class AdminAssignmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminAssignmentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("managers")]
        public IActionResult GetManagers()
        {
            var managers = _context.AdminUsers
                .Where(user => user.Role == "manager")
                .OrderByDescending(user => user.Id)
                .Select(user => new
                {
                    id = user.Id,
                    fullName = user.FullName,
                    username = user.Username,
                    role = user.Role,
                    isLocked = user.IsLocked
                })
                .ToList();

            return Ok(managers);
        }

        [HttpGet("")]
        [HttpGet("assignments")]
        public async Task<IActionResult> GetAssignments()
        {
            Console.WriteLine("Assignments called");

            try
            {
                var assignments = await _context.LocationManagerAssignments
                    .AsNoTracking()
                    .OrderByDescending(item => item.Id)
                    .ToListAsync();

                var managerIds = assignments
                    .Select(item => item.ManagerId)
                    .Distinct()
                    .ToList();

                var locationIds = assignments
                    .Select(item => item.LocationId)
                    .Distinct()
                    .ToList();

                var managers = await _context.AdminUsers
                    .AsNoTracking()
                    .Where(item => managerIds.Contains(item.Id))
                    .ToDictionaryAsync(item => item.Id);

                var locations = await _context.Locations
                    .AsNoTracking()
                    .Where(item => locationIds.Contains(item.Id))
                    .ToDictionaryAsync(item => item.Id);

                var result = assignments.Select(item => new
                {
                    id = item.Id,
                    managerId = item.ManagerId,
                    managerName = managers.TryGetValue(item.ManagerId, out var manager) ? manager.FullName : string.Empty,
                    username = managers.TryGetValue(item.ManagerId, out manager) ? manager.Username : string.Empty,
                    role = managers.TryGetValue(item.ManagerId, out manager) ? manager.Role : string.Empty,
                    locationId = item.LocationId,
                    locationName = locations.TryGetValue(item.LocationId, out var location) ? location.Name : string.Empty
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("managers/{managerId}/locations")]
        public async Task<IActionResult> GetManagerLocations(int managerId)
        {
            try
            {
                var assignments = await _context.LocationManagerAssignments
                    .AsNoTracking()
                    .Where(item => item.ManagerId == managerId)
                    .OrderBy(item => item.LocationId)
                    .ToListAsync();

                var locationIds = assignments
                    .Select(item => item.LocationId)
                    .Distinct()
                    .ToList();

                var locations = await _context.Locations
                    .AsNoTracking()
                    .Where(item => locationIds.Contains(item.Id))
                    .ToDictionaryAsync(item => item.Id);

                var result = assignments.Select(item => new
                {
                    id = item.Id,
                    managerId = item.ManagerId,
                    locationId = item.LocationId,
                    location = locations.TryGetValue(item.LocationId, out var location) ? location : null
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("")]
        [HttpPost("assignments")]
        public IActionResult CreateAssignment(CreateAssignmentRequest request)
        {
            var manager = _context.AdminUsers.Find(request.ManagerId);
            if (manager == null)
                return NotFound(new { message = "Manager not found" });

            var managerRole = manager.Role;
            if (managerRole != Roles.Manager)
                return BadRequest(new { message = "User is not a manager" });

            var location = _context.Locations.Find(request.LocationId);
            if (location == null)
                return NotFound(new { message = "Location not found" });

            var exists = _context.LocationManagerAssignments.Any(item =>
                item.ManagerId == request.ManagerId && item.LocationId == request.LocationId);

            if (exists)
                return Ok(new { message = "Already assigned" });

            var assignment = new LocationManagerAssignment
            {
                ManagerId = request.ManagerId,
                LocationId = request.LocationId
            };

            _context.LocationManagerAssignments.Add(assignment);
            _context.SaveChanges();

            return Ok(new
            {
                id = assignment.Id,
                managerId = assignment.ManagerId,
                locationId = assignment.LocationId
            });
        }

        [HttpDelete("")]
        [HttpDelete("assignments")]
        public IActionResult DeleteAssignment([FromQuery] int managerId, [FromQuery] int locationId)
        {
            var assignment = _context.LocationManagerAssignments.FirstOrDefault(item =>
                item.ManagerId == managerId && item.LocationId == locationId);

            if (assignment == null)
                return NotFound();

            _context.LocationManagerAssignments.Remove(assignment);
            _context.SaveChanges();

            return Ok();
        }
    }
}
