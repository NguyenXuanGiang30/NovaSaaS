using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Application.DTOs
{
    #region Login DTOs

    /// <summary>
    /// Request đăng nhập - Yêu cầu cả Subdomain để xác định Tenant.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Email đăng nhập.
        /// </summary>
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Mật khẩu.
        /// </summary>
        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Subdomain của Tenant (ví dụ: "demo", "vinamilk").
        /// </summary>
        [Required(ErrorMessage = "Subdomain là bắt buộc")]
        public string Subdomain { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response sau khi đăng nhập thành công.
    /// </summary>
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ErrorCode { get; set; }
        public AuthTokens? Tokens { get; set; }
        public UserInfo? User { get; set; }

        public static LoginResponse Ok(AuthTokens tokens, UserInfo user)
        {
            return new LoginResponse
            {
                Success = true,
                Message = "Đăng nhập thành công",
                Tokens = tokens,
                User = user
            };
        }

        public static LoginResponse Fail(string message, string errorCode)
        {
            return new LoginResponse
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }

    /// <summary>
    /// Thông tin tokens trả về.
    /// </summary>
    public class AuthTokens
    {
        /// <summary>
        /// JWT Access Token.
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Refresh Token để gia hạn phiên.
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Thời điểm AccessToken hết hạn.
        /// </summary>
        public DateTime AccessTokenExpiresAt { get; set; }

        /// <summary>
        /// Thời điểm RefreshToken hết hạn.
        /// </summary>
        public DateTime RefreshTokenExpiresAt { get; set; }

        /// <summary>
        /// Loại token (luôn là "Bearer").
        /// </summary>
        public string TokenType { get; set; } = "Bearer";
    }

    /// <summary>
    /// Thông tin User trả về sau đăng nhập.
    /// </summary>
    public class UserInfo
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public Guid TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string SchemaName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
    }

    #endregion

    #region Refresh Token DTOs

    /// <summary>
    /// Request làm mới token.
    /// </summary>
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "Refresh token là bắt buộc")]
        public string RefreshToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response khi làm mới token.
    /// </summary>
    public class RefreshTokenResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ErrorCode { get; set; }
        public AuthTokens? Tokens { get; set; }

        public static RefreshTokenResponse Ok(AuthTokens tokens)
        {
            return new RefreshTokenResponse
            {
                Success = true,
                Message = "Token đã được làm mới",
                Tokens = tokens
            };
        }

        public static RefreshTokenResponse Fail(string message, string errorCode)
        {
            return new RefreshTokenResponse
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }

    #endregion

    #region Logout DTOs

    /// <summary>
    /// Request đăng xuất (thu hồi refresh token).
    /// </summary>
    public class LogoutRequest
    {
        /// <summary>
        /// Refresh token cần thu hồi.
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Đăng xuất tất cả thiết bị.
        /// </summary>
        public bool RevokeAll { get; set; } = false;
    }

    #endregion
}
