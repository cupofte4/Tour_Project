using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Tour_Project.Data;
using Tour_Project.Models;
using Tour_Project.Services;

namespace Tour_Project.Controllers
{
    [Route("api/location")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private static readonly JsonSerializerOptions ReviewJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly AppDbContext _context;
        private readonly LocationService _service;

        public LocationController(AppDbContext context, LocationService service)
        {
            _context = context;
            _service = service;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_context.Locations.ToList());
        }

        [HttpPost]
        public IActionResult Create(Location location)
        {
            location.ReviewsJson ??= "[]";
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
            location.Address = updated.Address;
            location.Phone = updated.Phone;
            location.ReviewsJson = string.IsNullOrWhiteSpace(updated.ReviewsJson) ? "[]" : updated.ReviewsJson;
            location.Latitude = updated.Latitude;
            location.Longitude = updated.Longitude;
            location.TextVi = updated.TextVi;
            location.TextEn = updated.TextEn;
            location.TextZh = updated.TextZh;
            location.TextDe = updated.TextDe;

            _context.SaveChanges();
            return Ok(location);
        }

        [HttpPost("{id}/reviews")]
        public IActionResult AddReview(int id, CreateLocationReviewRequest request)
        {
            var location = _context.Locations.Find(id);
            if (location == null) return NotFound();

            var comment = request.Comment?.Trim() ?? "";
            if (request.Rating < 1 || request.Rating > 5)
            {
                return BadRequest("Rating must be between 1 and 5.");
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                return BadRequest("Comment is required.");
            }

            var reviews = DeserializeReviews(location.ReviewsJson);

            reviews.Insert(0, new Dictionary<string, object>
            {
                ["author"] = string.IsNullOrWhiteSpace(request.Author) ? "Guest" : request.Author.Trim(),
                ["rating"] = request.Rating,
                ["comment"] = comment,
                ["createdAt"] = DateTime.UtcNow
            });

            location.ReviewsJson = JsonSerializer.Serialize(reviews, ReviewJsonOptions);
            _context.SaveChanges();

            return Ok(location);
        }

        private static List<Dictionary<string, object>> DeserializeReviews(string? rawJson)
        {
            if (string.IsNullOrWhiteSpace(rawJson))
            {
                return new List<Dictionary<string, object>>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<Dictionary<string, object>>>(rawJson, ReviewJsonOptions)
                       ?? new List<Dictionary<string, object>>();
            }
            catch
            {
                return new List<Dictionary<string, object>>();
            }
        }
    }
}
