using NovaSaaS.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Interfaces
{
    /// <summary>
    /// Service xác thực người dùng và quản lý JWT tokens.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Đăng nhập với email, password và subdomain.
        /// </summary>
        /// <param name="request">Thông tin đăng nhập</param>
        /// <param name="ipAddress">Địa chỉ IP của client</param>
        /// <returns>Kết quả đăng nhập với JWT tokens</returns>
        Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress = null);

        /// <summary>
        /// Làm mới Access Token bằng Refresh Token.
        /// </summary>
        /// <param name="refreshToken">Refresh token hiện tại</param>
        /// <param name="ipAddress">Địa chỉ IP của client</param>
        /// <returns>Tokens mới</returns>
        Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken, string? ipAddress = null);

        /// <summary>
        /// Thu hồi một Refresh Token cụ thể.
        /// </summary>
        /// <param name="refreshToken">Token cần thu hồi</param>
        /// <param name="ipAddress">Địa chỉ IP của client</param>
        Task<bool> RevokeTokenAsync(string refreshToken, string? ipAddress = null);

        /// <summary>
        /// Thu hồi tất cả Refresh Tokens của một User.
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <param name="ipAddress">Địa chỉ IP của client</param>
        Task RevokeAllTokensAsync(Guid userId, string? ipAddress = null);
    }
}
