using Microsoft.AspNetCore.Mvc;
using Tour_Project.Data;
using Tour_Project.Models;
using Tour_Project.Services;

namespace Tour_Project.Controllers
{
    [Route("api/geofence")]
    [ApiController]
    public class GeofenceController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly LocationService _locationService;
        
        // Geofence radius in meters
        private const double GEOFENCE_RADIUS = 50;

        public GeofenceController(AppDbContext context, LocationService locationService)
        {
            _context = context;
            _locationService = locationService;
        }

        /// <summary>
        /// Check if user is within geofence of any location
        /// Returns nearest POI and whether user is inside geofence
        /// </summary>
        /// <param name="lat">User latitude</param>
        /// <param name="lng">User longitude</param>
        /// <returns>
        /// {
        ///   nearbyPOI: Location object or null,
        ///   distance: distance in meters,
        ///   inGeofence: true if distance < GEOFENCE_RADIUS,
        ///   geofenceRadius: GEOFENCE_RADIUS in meters
        /// }
        /// </returns>
        [HttpPost("check")]
        public IActionResult Check([FromQuery] double lat, [FromQuery] double lng)
        {
            if (lat == 0 || lng == 0)
            {
                return BadRequest(new { message = "Invalid coordinates" });
            }

            try
            {
                var locations = _context.Locations.ToList();
                
                if (locations.Count == 0)
                {
                    return Ok(new
                    {
                        nearbyPOI = (Location?)null,
                        distance = double.MaxValue,
                        inGeofence = false,
                        geofenceRadius = GEOFENCE_RADIUS
                    });
                }

                // Find nearest location
                Location? nearestLocation = null;
                double minDistance = double.MaxValue;

                foreach (var location in locations)
                {
                    double distance = _locationService.Distance(lat, lng, location.Latitude, location.Longitude);
                    
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestLocation = location;
                    }
                }

                // Check if within geofence
                bool inGeofence = minDistance < GEOFENCE_RADIUS;

                return Ok(new
                {
                    nearbyPOI = nearestLocation,
                    distance = Math.Round(minDistance, 2),
                    inGeofence = inGeofence,
                    geofenceRadius = GEOFENCE_RADIUS
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error checking geofence", error = ex.Message });
            }
        }

        /// <summary>
        /// Batch check multiple locations for geofence
        /// Returns all locations within geofence sorted by distance
        /// </summary>
        [HttpPost("check-all")]
        public IActionResult CheckAll([FromQuery] double lat, [FromQuery] double lng)
        {
            if (lat == 0 || lng == 0)
            {
                return BadRequest(new { message = "Invalid coordinates" });
            }

            try
            {
                var locations = _context.Locations.ToList();
                
                var results = locations
                    .Select(location => new
                    {
                        location = location,
                        distance = _locationService.Distance(lat, lng, location.Latitude, location.Longitude)
                    })
                    .Where(x => x.distance < GEOFENCE_RADIUS)
                    .OrderBy(x => x.distance)
                    .Select(x => new
                    {
                        id = x.location.Id,
                        name = x.location.Name,
                        distance = Math.Round(x.distance, 2),
                        latitude = x.location.Latitude,
                        longitude = x.location.Longitude
                    })
                    .ToList();

                return Ok(new
                {
                    nearbyLocations = results,
                    count = results.Count,
                    geofenceRadius = GEOFENCE_RADIUS
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error checking geofences", error = ex.Message });
            }
        }
    }
}
