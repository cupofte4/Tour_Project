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
                .OrderByDescending(user => user.Id)
                .Select(user => new
                {
                    id = user.Id,
                    fullName = user.FullName,
                    username = user.Username,
                    phone = user.Phone,
                    gender = user.Gender,
                    avatar = user.Avatar,
                    role = Roles.Normalize(user.Role),
                    isLocked = user.IsLocked
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

            var normalizedRole = Roles.Normalize(user.Role);
            if (normalizedRole == Roles.Admin && request.IsLocked)
                return BadRequest(new { message = "Không thể khóa tài khoản admin" });

            if (!string.IsNullOrWhiteSpace(request.Password))
                user.Password = request.Password.Trim();

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
                role = normalizedRole,
                isLocked = user.IsLocked
            });
        }
    }
}
