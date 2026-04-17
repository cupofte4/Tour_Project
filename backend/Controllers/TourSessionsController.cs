using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tour_Project.Data;
using Tour_Project.Models;

namespace Tour_Project.Controllers
{
    [Route("api/sessions")]
    [ApiController]
    public class TourSessionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TourSessionsController(AppDbContext context)
        {
            _context = context;
        }

        // POST /api/sessions  — bắt đầu tour
        [HttpPost]
        public async Task<IActionResult> Start(StartSessionRequest request)
        {
            var tourExists = await _context.Tours.AnyAsync(t => t.Id == request.TourId && t.IsActive);
            if (!tourExists) return NotFound(new { message = "Tour not found" });

            var session = new TourSession
            {
                UserId = request.UserId,
                TourId = request.TourId,
                LanguageCode = request.LanguageCode ?? "vi"
            };
            _context.TourSessions.Add(session);
            await _context.SaveChangesAsync();
            return Ok(new { session.Id, session.TourId, session.LanguageCode, session.StartedAt });
        }

        // GET /api/sessions/{id}  — lấy trạng thái phiên
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSession(int id)
        {
            var session = await _context.TourSessions
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id, s.TourId, s.UserId, s.LanguageCode,
                    s.StartedAt, s.CompletedAt,
                    visitedCount = s.Visits.Count,
                    visits = s.Visits.Select(v => new
                    {
                        v.LocationId, v.AudioPlayed, v.VisitedAt
                    })
                })
                .FirstOrDefaultAsync();

            return session == null ? NotFound() : Ok(session);
        }

        // POST /api/sessions/{id}/visits  — ghi nhận POI đã ghé
        [HttpPost("{id}/visits")]
        public async Task<IActionResult> RecordVisit(int id, RecordVisitRequest request)
        {
            var session = await _context.TourSessions.FindAsync(id);
            if (session == null) return NotFound();

            // Không ghi trùng
            var alreadyVisited = await _context.SessionVisits
                .AnyAsync(v => v.SessionId == id && v.LocationId == request.LocationId);
            if (alreadyVisited) return Ok(new { message = "Already visited" });

            var visit = new SessionVisit
            {
                SessionId = id,
                LocationId = request.LocationId,
                AudioPlayed = request.AudioPlayed
            };
            _context.SessionVisits.Add(visit);

            // Kiểm tra hoàn thành tour
            var totalPOIs = await _context.TourLocations
                .CountAsync(tl => tl.TourId == session.TourId && !tl.IsOptional);
            var visitedCount = await _context.SessionVisits
                .CountAsync(v => v.SessionId == id);

            if (visitedCount + 1 >= totalPOIs && session.CompletedAt == null)
            {
                session.CompletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok(visit);
        }

        // PATCH /api/sessions/{id}/complete  — đánh dấu hoàn thành thủ công
        [HttpPatch("{id}/complete")]
        public async Task<IActionResult> Complete(int id)
        {
            var session = await _context.TourSessions.FindAsync(id);
            if (session == null) return NotFound();

            session.CompletedAt ??= DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { session.Id, session.CompletedAt });
        }
    }

    public class StartSessionRequest
    {
        public int UserId { get; set; }
        public int TourId { get; set; }
        public string? LanguageCode { get; set; }
    }

    public class RecordVisitRequest
    {
        public int LocationId { get; set; }
        public bool AudioPlayed { get; set; } = false;
    }
}
