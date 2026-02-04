using NovaSaaS.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Identity
{
    /// <summary>
    /// Token mời nhân viên tham gia hệ thống.
    /// Được tạo khi Admin mời người dùng mới qua email.
    /// </summary>
    public class InvitationToken : BaseEntity
    {
        /// <summary>
        /// Token duy nhất (UUID) để xác thực lời mời.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Token { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Email của người được mời.
        /// </summary>
        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Tên đầy đủ của người được mời (optional).
        /// </summary>
        [MaxLength(200)]
        public string? FullName { get; set; }

        /// <summary>
        /// Role sẽ được gán khi người dùng accept invitation.
        /// </summary>
        public Guid? RoleId { get; set; }
        public virtual Role? Role { get; set; }

        /// <summary>
        /// Người gửi lời mời.
        /// </summary>
        public Guid InvitedByUserId { get; set; }
        public virtual User InvitedByUser { get; set; } = null!;

        /// <summary>
        /// Thời điểm hết hạn token (mặc định 7 ngày).
        /// </summary>
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);

        /// <summary>
        /// Thời điểm người dùng accept invitation (null = chưa accept).
        /// </summary>
        public DateTime? AcceptedAt { get; set; }

        /// <summary>
        /// User được tạo khi accept invitation.
        /// </summary>
        public Guid? AcceptedUserId { get; set; }
        public virtual User? AcceptedUser { get; set; }

        /// <summary>
        /// Trạng thái invitation.
        /// </summary>
        public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

        // ========================================
        // Computed Properties
        // ========================================

        /// <summary>
        /// Kiểm tra token còn hiệu lực không.
        /// </summary>
        public bool IsValid => Status == InvitationStatus.Pending && DateTime.UtcNow < ExpiresAt;

        /// <summary>
        /// Kiểm tra token đã hết hạn chưa.
        /// </summary>
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    }

    public enum InvitationStatus
    {
        Pending = 0,
        Accepted = 1,
        Expired = 2,
        Cancelled = 3
    }
}
