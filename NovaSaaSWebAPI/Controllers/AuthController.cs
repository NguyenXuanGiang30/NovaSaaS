using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Application.DTOs;
using NovaSaaS.Application.Interfaces;

namespace NovaSaaSWebAPI.Controllers
{
    /// <summary>
    /// Controller xác thực - Quản lý đăng nhập, đăng xuất và làm mới token.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Đăng nhập với email, password và subdomain.
        /// POST /api/auth/login
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(LoginResponse.Fail(
                    "Dữ liệu không hợp lệ",
                    "VALIDATION_FAILED"
                ));
            }

            var ipAddress = GetIpAddress();
            var result = await _authService.LoginAsync(request, ipAddress);

            if (result.Success)
            {
                // Set refresh token vào HttpOnly cookie
                SetRefreshTokenCookie(result.Tokens!.RefreshToken, result.Tokens.RefreshTokenExpiresAt);
                return Ok(result);
            }

            return Unauthorized(result);
        }

        /// <summary>
        /// Làm mới Access Token bằng Refresh Token.
        /// POST /api/auth/refresh
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest? request = null)
        {
            // Ưu tiên lấy từ cookie, nếu không có thì lấy từ body
            var refreshToken = request?.RefreshToken ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(RefreshTokenResponse.Fail(
                    "Refresh token không được cung cấp",
                    "MISSING_TOKEN"
                ));
            }

            var ipAddress = GetIpAddress();
            var result = await _authService.RefreshTokenAsync(refreshToken, ipAddress);

            if (result.Success)
            {
                // Cập nhật cookie với token mới
                SetRefreshTokenCookie(result.Tokens!.RefreshToken, result.Tokens.RefreshTokenExpiresAt);
                return Ok(result);
            }

            return Unauthorized(result);
        }

        /// <summary>
        /// Đăng xuất - Thu hồi Refresh Token.
        /// POST /api/auth/logout
        /// </summary>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest? request = null)
        {
            var refreshToken = request?.RefreshToken ?? Request.Cookies["refreshToken"];

            if (!string.IsNullOrEmpty(refreshToken))
            {
                var ipAddress = GetIpAddress();
                await _authService.RevokeTokenAsync(refreshToken, ipAddress);
            }

            // Xóa cookie
            Response.Cookies.Delete("refreshToken");

            return Ok(new { message = "Đăng xuất thành công" });
        }

        /// <summary>
        /// Lấy thông tin User hiện tại từ JWT.
        /// GET /api/auth/me
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserInfo), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetCurrentUser()
        {
            var user = User;

            if (user.Identity?.IsAuthenticated != true)
            {
                return Unauthorized();
            }

            var userInfo = new UserInfo
            {
                Id = Guid.Parse(user.FindFirst("sub")?.Value ?? Guid.Empty.ToString()),
                Email = user.FindFirst("email")?.Value ?? "",
                FullName = user.FindFirst("name")?.Value ?? "",
                TenantId = Guid.Parse(user.FindFirst("tenant_id")?.Value ?? Guid.Empty.ToString()),
                TenantName = user.FindFirst("tenant_name")?.Value ?? "",
                SchemaName = user.FindFirst("schema_name")?.Value ?? "",
                Roles = user.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList(),
                Permissions = user.FindAll("permission").Select(c => c.Value).ToList()
            };

            return Ok(userInfo);
        }

        #region Helpers

        /// <summary>
        /// Lấy địa chỉ IP của client.
        /// </summary>
        private string GetIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                return Request.Headers["X-Forwarded-For"].ToString();
            }
            return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "unknown";
        }

        /// <summary>
        /// Đặt Refresh Token vào HttpOnly cookie.
        /// </summary>
        private void SetRefreshTokenCookie(string token, DateTime expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expires,
                SameSite = SameSiteMode.Strict,
                Secure = true // Chỉ gửi qua HTTPS
            };
            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        #endregion
    }
}
