namespace NovaSaaS.Infrastructure.Configurations
{
    /// <summary>
    /// Cấu hình JWT Authentication.
    /// Đọc từ appsettings.json section "JwtSettings".
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// Secret key để ký JWT (tối thiểu 32 ký tự).
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// Issuer của token (tên hệ thống phát hành).
        /// </summary>
        public string Issuer { get; set; } = "NovaSaaS";

        /// <summary>
        /// Audience của token (đối tượng sử dụng).
        /// </summary>
        public string Audience { get; set; } = "NovaSaaS.Clients";

        /// <summary>
        /// Thời gian sống của Access Token (phút).
        /// </summary>
        public int AccessTokenExpiryMinutes { get; set; } = 60;

        /// <summary>
        /// Thời gian sống của Refresh Token (ngày).
        /// </summary>
        public int RefreshTokenExpiryDays { get; set; } = 7;
    }
}
