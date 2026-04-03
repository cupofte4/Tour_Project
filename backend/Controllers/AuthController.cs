using Microsoft.AspNetCore.Mvc;
using Tour_Project.Data;
using Tour_Project.Models;

namespace Tour_Project.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public IActionResult Login(User user)
        {
            var u = _context.Users
                .FirstOrDefault(x => x.Username == user.Username && x.Password == user.Password);

            if (u == null)
                return Unauthorized(new { message = "Username hoặc password không chính xác" });

            if (u.IsLocked)
                return Unauthorized(new { message = "Tài khoản của bạn đã bị khóa" });

            return Ok(new
            {
                id = u.Id,
                fullName = u.FullName,
                username = u.Username,
                phone = u.Phone,
                gender = u.Gender,
                avatar = u.Avatar,
                role = Roles.Normalize(u.Role),
                isLocked = u.IsLocked
            });
        }

        [HttpPost("register")]
        public IActionResult Register(User user)
        {
            // Check if username already exists
            var existingUser = _context.Users.FirstOrDefault(x => x.Username == user.Username);
            if (existingUser != null)
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại" });

            // Set default role as 'user'
            user.Role = Roles.User;
            
            _context.Users.Add(user);
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

        [HttpPost("change-password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.CurrentPassword) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { message = "Vui lòng nhập đầy đủ thông tin đổi mật khẩu" });
            }

            if (request.NewPassword.Length < 6)
            {
                return BadRequest(new { message = "Mật khẩu mới phải có ít nhất 6 ký tự" });
            }

            var user = _context.Users.FirstOrDefault(x => x.Username == request.Username);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng" });
            }

            if (user.Password != request.CurrentPassword)
            {
                return BadRequest(new { message = "Mật khẩu hiện tại không chính xác" });
            }

            user.Password = request.NewPassword;
            _context.SaveChanges();

            return Ok(new { message = "Đổi mật khẩu thành công" });
        }
    }

    public class ChangePasswordRequest
    {
        public string Username { get; set; } = string.Empty;
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
