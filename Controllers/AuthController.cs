using GarageMasterBE.Models;
using GarageMasterBE.Services;
using Microsoft.AspNetCore.Mvc;

namespace GarageMasterBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly EmailService _emailService;
        private readonly JwtService _jwtService;

        public AuthController(UserService userService, EmailService emailService, JwtService jwtService)
        {
            _userService = userService;
            _emailService = emailService;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _userService.GetByEmailAsync(request.Email);
            if (existingUser != null)
                return BadRequest("Email đã được sử dụng.");

            var user = await _userService.CreateUserAsync(request.Email, request.Password);
            if (user == null)
                return StatusCode(500, "Đăng ký không thành công.");

            // Kiểm tra email và mã xác nhận trước khi gửi mail
            if (string.IsNullOrWhiteSpace(user.Email))
                return StatusCode(500, "Email của người dùng không hợp lệ.");

            if (string.IsNullOrWhiteSpace(user.EmailConfirmationCode))
                return StatusCode(500, "Mã xác nhận email không hợp lệ.");

            try
            {
                await _emailService.SendConfirmationEmailAsync(user.Email, user.EmailConfirmationCode);
            }
            catch (Exception ex)
            {
                // Log lỗi (nên có logger ở đây)
                return StatusCode(500, $"Gửi email xác nhận thất bại: {ex.Message}");
            }

            return Ok(new { message = "Đăng ký thành công. Vui lòng kiểm tra email để xác nhận tài khoản." });
        }


        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
        {
            var success = await _userService.ConfirmEmailAsync(request.Email, request.Code);
            if (!success)
                return BadRequest(new { message = "Mã xác thực không đúng hoặc đã hết hạn." });

            var user = await _userService.GetByEmailAsync(request.Email);
            if (user == null)
                return BadRequest(new { message = "Không tìm thấy người dùng." });

            var token = _jwtService.GenerateToken(user.Id, user.Email, user.Role);

            return Ok(new { message = "Xác thực email thành công.", token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.ValidateUserAsync(request.Email, request.Password);
            if (user == null)
                return Unauthorized(new { message = "Email hoặc mật khẩu không đúng, hoặc chưa xác thực email." });

            var token = _jwtService.GenerateToken(user.Id, user.Email, user.Role);

            return Ok(new
            {
                message = "Đăng nhập thành công",
                token,
                user = new
                {
                    user.Id,
                    user.Email,
                    user.Role
                }
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
        {
            await _userService.ForgotPasswordAsync(req.Email, _emailService);
            return Ok(new { message = "Nếu email tồn tại, mật khẩu mới đã được gửi!" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
        {
            var ok = await _userService.ResetPasswordWithCurrentAsync(req.Email, req.CurrentPassword, req.NewPassword);
            if (!ok) return BadRequest(new { message = "Mật khẩu hiện tại không đúng hoặc email không tồn tại." });
            return Ok(new { message = "Đổi mật khẩu thành công!" });
        }
    }
}
