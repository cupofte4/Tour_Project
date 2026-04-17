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
        public IActionResult Login(User user)
        {
            var u = _context.Users
                .FirstOrDefault(x => x.Username == user.Username && x.Password == user.Password);

            if (u == null)
                return Unauthorized(new { message = "Username hoặc password không chính xác" });

            if (u.IsLocked)
                return Unauthorized(new { message = "Tài khoản của bạn đã bị khóa" });

            // Generate JWT token
            var token = _jwtService.GenerateToken(u.Id, u.Username, u.Role);

            return Ok(new
            {
                token = token,
                user = new
                {
                    id = u.Id,
                    fullName = u.FullName,
                    username = u.Username,
                    phone = u.Phone,
                    gender = u.Gender,
                    avatar = u.Avatar,
                    role = Roles.Normalize(u.Role),
                    isLocked = u.IsLocked
                }
            });
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Vui lÃ²ng nháº­p Ä‘áº§y Ä‘á»§ thÃ´ng tin" });
            }

            // Check if username already exists
            var existingUser = _context.Users.FirstOrDefault(x => x.Username == request.Username);
            if (existingUser != null)
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại" });

            var roleRaw = (request.Role ?? string.Empty).Trim();
            var roleNormalized = roleRaw.ToLowerInvariant();
            var selectedRole =
                roleNormalized == Roles.Manager ||
                roleRaw.Equals("Tôi muốn kinh doanh", StringComparison.OrdinalIgnoreCase) ||
                roleRaw.Equals("Toi muon kinh doanh", StringComparison.OrdinalIgnoreCase)
                    ? Roles.Manager
                    : Roles.User;

            var user = new User
            {
                FullName = request.FullName.Trim(),
                Username = request.Username.Trim(),
                Password = request.Password.Trim(),
                Role = selectedRole,
                IsLocked = false
            };
            
            _context.Users.Add(user);
            _context.SaveChanges();

            // Generate JWT token
            var token = _jwtService.GenerateToken(user.Id, user.Username, user.Role);

            return Ok(new
            {
                token = token,
                user = new
                {
                    id = user.Id,
                    fullName = user.FullName,
                    username = user.Username,
                    phone = user.Phone,
                    gender = user.Gender,
                    avatar = user.Avatar,
                    role = Roles.Normalize(user.Role),
                    isLocked = user.IsLocked
                }
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

    public class RegisterRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Role { get; set; }
    }
}
