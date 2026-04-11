using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tour_Project.Data;
using Tour_Project.Models;

namespace Tour_Project.Controllers
{
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
            var managers = _context.Users
                .Where(user => Roles.Normalize(user.Role) == Roles.Manager)
                .OrderByDescending(user => user.Id)
                .Select(user => new
                {
                    id = user.Id,
                    fullName = user.FullName,
                    username = user.Username,
                    role = Roles.Normalize(user.Role),
                    isLocked = user.IsLocked
                })
                .ToList();

            return Ok(managers);
        }

        [HttpGet("assignments")]
        public IActionResult GetAssignments()
        {
            var assignments = _context.LocationManagerAssignments
                .Include(item => item.Manager)
                .Include(item => item.Location)
                .OrderByDescending(item => item.Id)
                .Select(item => new
                {
                    id = item.Id,
                    managerId = item.ManagerId,
                    managerName = item.Manager != null ? item.Manager.FullName : string.Empty,
                    locationId = item.LocationId,
                    locationName = item.Location != null ? item.Location.Name : string.Empty
                })
                .ToList();

            return Ok(assignments);
        }

        [HttpGet("available-locations")]
        public IActionResult GetAvailableLocations()
        {
            // Get all locations that are not assigned to any manager
            var assignedLocationIds = _context.LocationManagerAssignments
                .Select(item => item.LocationId)
                .ToList();

            var availableLocations = _context.Locations
                .Where(location => !assignedLocationIds.Contains(location.Id))
                .OrderByDescending(location => location.Id)
                .Select(location => new
                {
                    id = location.Id,
                    name = location.Name,
                    description = location.Description,
                    latitude = location.Latitude,
                    longitude = location.Longitude,
                    address = location.Address
                })
                .ToList();

            return Ok(availableLocations);
        }

        [HttpGet("locations/{locationId}/availability")]
        public IActionResult CheckLocationAvailability(int locationId)
        {
            var location = _context.Locations.Find(locationId);
            if (location == null)
                return NotFound(new { message = "Location not found" });

            var assignment = _context.LocationManagerAssignments
                .Include(item => item.Manager)
                .FirstOrDefault(item => item.LocationId == locationId);

            if (assignment == null)
            {
                return Ok(new
                {
                    locationId = locationId,
                    isAvailable = true,
                    assignedManager = (string?)null
                });
            }

            return Ok(new
            {
                locationId = locationId,
                isAvailable = false,
                assignedManager = assignment.Manager?.FullName
            });
        }

        [HttpGet("managers/{managerId}/locations")]
        public IActionResult GetManagerLocations(int managerId)
        {
            var assignments = _context.LocationManagerAssignments
                .Where(item => item.ManagerId == managerId)
                .Include(item => item.Location)
                .OrderBy(item => item.LocationId)
                .Select(item => new
                {
                    id = item.Id,
                    managerId = item.ManagerId,
                    locationId = item.LocationId,
                    location = item.Location
                })
                .ToList();

            return Ok(assignments);
        }

        [HttpPost("assignments")]
        public IActionResult CreateAssignment(CreateAssignmentRequest request)
        {
            var manager = _context.Users.Find(request.ManagerId);
            if (manager == null)
                return NotFound(new { message = "Manager not found" });

            var managerRole = Roles.Normalize(manager.Role);
            if (managerRole != Roles.Manager)
                return BadRequest(new { message = "User is not a manager" });

            var location = _context.Locations.Find(request.LocationId);
            if (location == null)
                return NotFound(new { message = "Location not found" });

            // Check if location is already assigned to ANY manager
            var isLocationAlreadyAssigned = _context.LocationManagerAssignments.Any(item =>
                item.LocationId == request.LocationId);

            if (isLocationAlreadyAssigned)
            {
                var currentAssignment = _context.LocationManagerAssignments
                    .Include(item => item.Manager)
                    .FirstOrDefault(item => item.LocationId == request.LocationId);
                
                if (currentAssignment?.ManagerId == request.ManagerId)
                    return Ok(new { message = "Already assigned" });

                return BadRequest(new 
                { 
                    message = $"Location is already assigned to manager: {currentAssignment?.Manager?.FullName}. A location can only be managed by one manager." 
                });
            }

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

