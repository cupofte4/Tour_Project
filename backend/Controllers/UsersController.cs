using Microsoft.AspNetCore.Mvc;
using Tour_Project.Data;
using Tour_Project.Models;

namespace Tour_Project.Controllers
{
    [Route("api/users")]
    [ApiController]
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
            var users = _context.Users
                .OrderByDescending(u => u.Id)
                .Select(u => new
                {
                    id = u.Id,
                    fullName = u.FullName,
                    username = u.Username,
                    phone = u.Phone,
                    gender = u.Gender,
                    avatar = u.Avatar,
                    role = Roles.Normalize(u.Role),
                    isLocked = u.IsLocked
                })
                .ToList();

            return Ok(users);
        }

        [HttpPut("{id}/admin")]
        public IActionResult UpdateAdminSettings(int id, UserAdminUpdateRequest request)
        {
            var user = _context.Users.Find(id);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy người dùng" });

            var currentRole = Roles.Normalize(user.Role);

            // Cannot lock admin account
            if (currentRole == Roles.Admin && request.IsLocked)
                return BadRequest(new { message = "Không thể khóa tài khoản admin" });

            if (!string.IsNullOrWhiteSpace(request.Password))
                user.Password = request.Password.Trim();

            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                // Cannot change admin's own role
                if (currentRole == Roles.Admin)
                    return BadRequest(new { message = "Không thể đổi role của admin" });

                var newRole = Roles.Normalize(request.Role);
                // Only allow valid roles (admin / manager)
                if (!Roles.IsValid(newRole))
                    return BadRequest(new { message = "Role không hợp lệ. Chỉ chấp nhận: admin, manager" });

                user.Role = newRole;
            }

            user.IsLocked = request.IsLocked;
            _context.SaveChanges();

            return Ok(new
            {
                id = user.Id,
                fullName = user.FullName,
                username = user.Username,
                phone = user.Phone,
                gender = user.Gender,
                avatar = user.Avatar,
                role = Roles.Normalize(user.Role),
                isLocked = user.IsLocked
            });
        }
    }
}
