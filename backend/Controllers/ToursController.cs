using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tour_Project.Data;
using Tour_Project.Models;

namespace Tour_Project.Controllers
{
    [Route("api/tours")]
    [ApiController]
    public class ToursController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ToursController(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/tours
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var tours = await _context.Tours
                .Where(t => t.IsActive)
                .Select(t => new
                {
                    t.Id, t.Title, t.Description, t.CoverImage,
                    t.EstimatedDurationMinutes, t.CreatedAt,
                    locationCount = t.TourLocations.Count
                })
                .ToListAsync();
            return Ok(tours);
        }

        // GET /api/tours/admin
        [HttpGet("admin")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetAllAdmin()
        {
            var tours = await _context.Tours
                .OrderByDescending(t => t.Id)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.CoverImage,
                    t.EstimatedDurationMinutes,
                    t.IsActive,
                    t.CreatedAt,
                    locationCount = t.TourLocations.Count
                })
                .ToListAsync();

            return Ok(tours);
        }

        // GET /api/tours/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var tour = await _context.Tours
                .Where(t => t.Id == id && t.IsActive)
                .Select(t => new
                {
                    t.Id, t.Title, t.Description, t.CoverImage,
                    t.EstimatedDurationMinutes, t.IsActive, t.CreatedAt,
                    locationCount = t.TourLocations.Count
                })
                .FirstOrDefaultAsync();

            return tour == null ? NotFound() : Ok(tour);
        }

        // GET /api/tours/{id}/locations
        [HttpGet("{id}/locations")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLocations(int id)
        {
            var exists = await _context.Tours.AnyAsync(t => t.Id == id && t.IsActive);
            if (!exists) return NotFound();

            var locations = await _context.TourLocations
                .Where(tl => tl.TourId == id)
                .OrderBy(tl => tl.OrderIndex)
                .Select(tl => new
                {
                    tl.Id,
                    tl.OrderIndex,
                    isOptional = false,
                    location = new
                    {
                        tl.Location!.Id,
                        tl.Location.Name,
                        tl.Location.Description,
                        tl.Location.Image,
                        tl.Location.Latitude,
                        tl.Location.Longitude,
                        tl.Location.Address,
                        tl.Location.TextVi,
                        tl.Location.TextEn,
                        tl.Location.TextZh,
                        tl.Location.TextDe
                    }
                })
                .ToListAsync();

            return Ok(locations);
        }

        // POST /api/tours
        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Create(CreateTourRequest request)
        {
            var tour = new Tour
            {
                Title = request.Title,
                Description = request.Description,
                CoverImage = request.CoverImage,
                EstimatedDurationMinutes = request.EstimatedDurationMinutes
            };
            _context.Tours.Add(tour);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = tour.Id }, tour);
        }

        // PUT /api/tours/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Update(int id, CreateTourRequest request)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour == null) return NotFound();

            tour.Title = request.Title;
            tour.Description = request.Description;
            tour.CoverImage = request.CoverImage;
            tour.EstimatedDurationMinutes = request.EstimatedDurationMinutes;

            await _context.SaveChangesAsync();
            return Ok(tour);
        }

        // PATCH /api/tours/{id}/status
        [HttpPatch("{id}/status")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> UpdateStatus(int id, UpdateTourStatusRequest request)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour == null) return NotFound();

            tour.IsActive = request.IsActive;
            await _context.SaveChangesAsync();
            return Ok(new { tour.Id, tour.IsActive });
        }

        // DELETE /api/tours/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour == null) return NotFound();

            tour.IsActive = false;
            await _context.SaveChangesAsync();
            return Ok();
        }

        // POST /api/tours/{id}/locations  — gán POI vào tour
        [HttpPost("{id}/locations")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> AssignLocation(int id, AssignLocationRequest request)
        {
            var tourExists = await _context.Tours.AnyAsync(t => t.Id == id);
            if (!tourExists) return NotFound(new { message = "Tour not found" });

            var locationExists = await _context.Locations.AnyAsync(l => l.Id == request.LocationId);
            if (!locationExists) return NotFound(new { message = "Location not found" });

            var duplicate = await _context.TourLocations
                .AnyAsync(tl => tl.TourId == id && tl.LocationId == request.LocationId);
            if (duplicate) return BadRequest(new { message = "Location already in tour" });

            var tl = new TourLocation
            {
                TourId = id,
                LocationId = request.LocationId,
                OrderIndex = request.OrderIndex,
                IsOptional = request.IsOptional
            };
            _context.TourLocations.Add(tl);
            await _context.SaveChangesAsync();
            return Ok(tl);
        }

        // DELETE /api/tours/{id}/locations/{locationId}  — gỡ POI khỏi tour
        [HttpDelete("{id}/locations/{locationId}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> RemoveLocation(int id, int locationId)
        {
            var tl = await _context.TourLocations
                .FirstOrDefaultAsync(x => x.TourId == id && x.LocationId == locationId);
            if (tl == null) return NotFound();

            _context.TourLocations.Remove(tl);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // PUT /api/tours/{id}/locations/reorder  — sắp xếp lại thứ tự POI
        [HttpPut("{id}/locations/reorder")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Reorder(int id, List<ReorderItem> items)
        {
            var tourLocations = await _context.TourLocations
                .Where(tl => tl.TourId == id)
                .ToListAsync();

            foreach (var item in items)
            {
                var tl = tourLocations.FirstOrDefault(x => x.Id == item.Id);
                if (tl != null) tl.OrderIndex = item.OrderIndex;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
    }

    public class CreateTourRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CoverImage { get; set; }
        public int EstimatedDurationMinutes { get; set; }
    }

    public class AssignLocationRequest
    {
        public int LocationId { get; set; }
        public int OrderIndex { get; set; }
        public bool IsOptional { get; set; } = false;
    }

    public class UpdateTourStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class ReorderItem
    {
        public int Id { get; set; }
        public int OrderIndex { get; set; }
    }
}
