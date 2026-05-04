using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tour_Project.Data;
using Tour_Project.Models;

namespace Tour_Project.Controllers
{
    [Route("api/admin-users")]
    [Route("api/users")]
    [ApiController]
    [Authorize(Roles = Roles.Admin)]
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
            try
            {
                var users = _context.AdminUsers
                    .OrderByDescending(u => u.Id)
                    .Select(u => new
                    {
                        id = u.Id,
                        fullName = u.FullName,
                        username = u.Username,
                        role = u.Role,
                        isLocked = u.IsLocked
                    })
                    .ToList();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public IActionResult CreateAdminUser([FromBody] CreateAdminUserRequest request)
        {
            try
            {
                var username = (request.Username ?? string.Empty).Trim();
                var password = request.Password ?? string.Empty;
                var role = request.Role;

                if (string.IsNullOrWhiteSpace(username))
                    return BadRequest(new { message = "Username is required" });

                if (string.IsNullOrWhiteSpace(password))
                    return BadRequest(new { message = "Password is required" });

                if (!Roles.IsValid(role))
                    return BadRequest(new { message = "Role is invalid" });

                var exists = _context.AdminUsers.Any(user => user.Username == username);
                if (exists)
                    return Conflict(new { message = "Username already exists" });

                var user = new AdminUser
                {
                    Username = username,
                    PasswordHash = password,
                    FullName = string.IsNullOrWhiteSpace(request.FullName) ? username : request.FullName.Trim(),
                    Role = role,
                    IsLocked = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.AdminUsers.Add(user);
                _context.SaveChanges();

                return Ok(new
                {
                    id = user.Id,
                    fullName = user.FullName,
                    username = user.Username,
                    role = user.Role,
                    isLocked = user.IsLocked
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
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
                role = user.Role,
                isLocked = user.IsLocked
            });
        }

        [HttpPatch("{id}/role")]
        public IActionResult UpdateRole(int id, [FromBody] UpdateAdminUserRoleRequest request)
        {
            try
            {
                var user = _context.AdminUsers.Find(id);
                if (user == null) return NotFound(new { message = "User not found" });

                var role = request.Role;
                if (!Roles.IsValid(role))
                    return BadRequest(new { message = "Role is invalid" });

                user.Role = role;
                _context.SaveChanges();

                return Ok(new
                {
                    id = user.Id,
                    fullName = user.FullName,
                    username = user.Username,
                    role = user.Role,
                    isLocked = user.IsLocked
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }

    public class CreateAdminUserRequest
    {
        public string? FullName { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
    }

    public class AdminUserUpdateRequest
    {
        public string? Role { get; set; }
        public bool? IsLocked { get; set; }
        public string? Password { get; set; }
    }

    public class UpdateAdminUserRoleRequest
    {
        public string? Role { get; set; }
    }
}
