using NovaSaaS.Domain.Entities.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Identity
{
    /// <summary>
    /// Refresh Token - Lưu trữ token làm mới để gia hạn phiên đăng nhập.
    /// Mỗi User có thể có nhiều RefreshToken (đăng nhập từ nhiều thiết bị).
    /// </summary>
    public class RefreshToken : BaseEntity
    {
        /// <summary>
        /// Token string (thường là GUID hoặc chuỗi random).
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Thời điểm token hết hạn.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Đã bị thu hồi (revoked) chưa.
        /// </summary>
        public bool IsRevoked { get; set; } = false;

        /// <summary>
        /// Thời điểm bị thu hồi.
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// Token thay thế (nếu đã refresh sang token mới).
        /// </summary>
        [MaxLength(500)]
        public string? ReplacedByToken { get; set; }

        /// <summary>
        /// Địa chỉ IP khi tạo token.
        /// </summary>
        [MaxLength(50)]
        public string? CreatedByIp { get; set; }

        /// <summary>
        /// Địa chỉ IP khi thu hồi token.
        /// </summary>
        [MaxLength(50)]
        public string? RevokedByIp { get; set; }

        /// <summary>
        /// User ID sở hữu token này.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Navigation property đến User.
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Kiểm tra token còn hoạt động không.
        /// </summary>
        public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;
    }
}
