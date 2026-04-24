using Microsoft.AspNetCore.Mvc;
using Tour_Project.Data;
using Tour_Project.Models;
using Tour_Project.Services;

namespace Tour_Project.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public AuthController(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] AdminUser credentials)
        {
            var u = _context.AdminUsers
                .FirstOrDefault(x => x.Username == credentials.Username && x.PasswordHash == credentials.PasswordHash);

            if (u == null)
                return Unauthorized(new { message = "Invalid username or password" });

            if (u.IsLocked)
                return Unauthorized(new { message = "Account is locked" });

            var token = _jwtService.GenerateToken(u.Id, u.Username, Roles.Normalize(u.Role));

            return Ok(new
            {
                token,
                user = new
                {
                    id = u.Id,
                    fullName = u.FullName,
                    username = u.Username,
                    phone = u.Phone,
                    role = Roles.Normalize(u.Role),
                    isLocked = u.IsLocked
                }
            });
        }

        // Registration endpoint removed: admin accounts are seeded or managed out-of-band.

        [HttpPost("change-password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.CurrentPassword) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { message = "Please provide username, current and new password" });
            }

            if (request.NewPassword.Length < 6)
            {
                return BadRequest(new { message = "New password must be at least 6 characters" });
            }

            var user = _context.AdminUsers.FirstOrDefault(x => x.Username == request.Username);
            if (user == null)
                return NotFound(new { message = "User not found" });

            if (user.PasswordHash != request.CurrentPassword)
                return BadRequest(new { message = "Current password is incorrect" });

            user.PasswordHash = request.NewPassword;
            _context.SaveChanges();

            return Ok(new { message = "Password changed" });
        }
    }

    public class ChangePasswordRequest
    {
        public string Username { get; set; } = string.Empty;
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
