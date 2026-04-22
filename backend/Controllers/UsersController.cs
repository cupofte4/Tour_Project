using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tour_Project.Data;
using Tour_Project.Models;

namespace Tour_Project.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _context.AdminUsers
                .OrderByDescending(u => u.Id)
                .Select(u => new
                {
                    id = u.Id,
                    fullName = u.FullName,
                    username = u.Username,
                    role = Roles.Normalize(u.Role),
                    isLocked = u.IsLocked
                })
                .ToList();

            return Ok(users);
        }

        [HttpPut("{id}/admin")]
        public IActionResult UpdateAdminUser(int id, [FromBody] AdminUserUpdateRequest request)
        {
            var user = _context.AdminUsers.Find(id);
            if (user == null) return NotFound(new { message = "User not found" });

            if (request.IsLocked.HasValue)
                user.IsLocked = request.IsLocked.Value;

            if (!string.IsNullOrWhiteSpace(request.Role))
                user.Role = request.Role;

            if (!string.IsNullOrWhiteSpace(request.Password))
                user.PasswordHash = request.Password; // NOTE: password hashing is out of scope here

            _context.SaveChanges();

            return Ok(new
            {
                id = user.Id,
                fullName = user.FullName,
                username = user.Username,
                role = Roles.Normalize(user.Role),
                isLocked = user.IsLocked
            });
        }
    }

    public class AdminUserUpdateRequest
    {
        public string? Role { get; set; }
        public bool? IsLocked { get; set; }
        public string? Password { get; set; }
    }
}
