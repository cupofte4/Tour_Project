using Microsoft.AspNetCore.Mvc;
using Tour_Project.Data;
using Tour_Project.Models;
using Tour_Project.Services;

namespace Tour_Project.Controllers
{
    [Route("api/location")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly LocationService _service;

        public LocationController(AppDbContext context)
        {
            _context = context;
            _service = new LocationService();
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_context.Locations.ToList());
        }

        [HttpPost]
        public IActionResult Create(Location location)
        {
            _context.Locations.Add(location);
            _context.SaveChanges();
            return Ok(location);
        }

        [HttpGet("near")]
        public IActionResult GetNear(double lat, double lng)
        {
            var locations = _context.Locations.ToList();

            Location nearest = null;
            double minDistance = 50;

            foreach (var loc in locations)
            {
                double distance = _service.Distance(lat, lng, loc.Latitude, loc.Longitude);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = loc;
                }
            }

            return Ok(nearest);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var location = _context.Locations.Find(id);
            if (location == null) return NotFound();

            _context.Locations.Remove(location);
            _context.SaveChanges();
            return Ok();
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, Location updated)
        {
            var location = _context.Locations.Find(id);
            if (location == null) return NotFound();

            location.Name = updated.Name;
            location.Description = updated.Description;
            location.Image = updated.Image;
            location.Images = updated.Images;
            location.Audio = updated.Audio;
            location.Latitude = updated.Latitude;
            location.Longitude = updated.Longitude;
            location.TextVi = updated.TextVi;
            location.TextEn = updated.TextEn;
            location.TextZh = updated.TextZh;
            location.TextDe = updated.TextDe;

            _context.SaveChanges();
            return Ok(location);
        }
    }
}
