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

            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                if (normalizedRole == Roles.Admin)
                    return BadRequest(new { message = "KhÃ´ng thá»ƒ Ä‘á»•i role cá»§a admin" });

                user.Role = Roles.Normalize(request.Role);
                normalizedRole = Roles.Normalize(user.Role);
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
                role = normalizedRole,
                isLocked = user.IsLocked
            });
        }

        [HttpPost("admin/create")]
        public IActionResult CreateUserAsAdmin([FromBody] CreateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Vui lòng nhập đầy đủ thông tin (Họ tên, Username, Mật khẩu)" });
            }

            // Check if username already exists
            var existingUser = _context.Users.FirstOrDefault(x => x.Username == request.Username);
            if (existingUser != null)
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại" });

            // Validate role
            var roleNormalized = Roles.Normalize(request.Role ?? Roles.User);
            if (roleNormalized != Roles.User && roleNormalized != Roles.Manager)
            {
                return BadRequest(new { message = "Role không hợp lệ (user hoặc manager)" });
            }

            var newUser = new User
            {
                FullName = request.FullName.Trim(),
                Username = request.Username.Trim(),
                Password = request.Password.Trim(),
                Phone = request.Phone?.Trim(),
                Gender = request.Gender?.Trim(),
                Avatar = request.Avatar?.Trim(),
                Role = roleNormalized,
                IsLocked = false
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok(new
            {
                id = newUser.Id,
                fullName = newUser.FullName,
                username = newUser.Username,
                phone = newUser.Phone,
                gender = newUser.Gender,
                avatar = newUser.Avatar,
                role = Roles.Normalize(newUser.Role),
                isLocked = newUser.IsLocked
            });
        }
    }
}
