using NovaSaaS.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Common
{
    /// <summary>
    /// Notification - Thông báo cho người dùng trong hệ thống.
    /// Hỗ trợ thông báo real-time qua SignalR.
    /// </summary>
    public class Notification : BaseEntity
    {
        /// <summary>
        /// User nhận thông báo.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Loại thông báo (Info, Success, Warning, Error).
        /// </summary>
        public NotificationType Type { get; set; } = NotificationType.Info;

        /// <summary>
        /// Tiêu đề thông báo.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Nội dung chi tiết.
        /// </summary>
        [MaxLength(2000)]
        public string? Message { get; set; }

        /// <summary>
        /// Category để phân loại (Order, Stock, AI, System, etc.)
        /// </summary>
        [MaxLength(50)]
        public string Category { get; set; } = "System";

        /// <summary>
        /// Entity liên quan (VD: OrderId, ProductId).
        /// </summary>
        public Guid? RelatedEntityId { get; set; }

        /// <summary>
        /// Loại entity liên quan.
        /// </summary>
        [MaxLength(50)]
        public string? RelatedEntityType { get; set; }

        /// <summary>
        /// Link để navigate khi click vào notification.
        /// </summary>
        [MaxLength(500)]
        public string? ActionUrl { get; set; }

        /// <summary>
        /// Đã đọc chưa.
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// Thời điểm đọc.
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// Metadata bổ sung (JSON).
        /// </summary>
        public string? Metadata { get; set; }
    }

    public enum NotificationType
    {
        Info = 0,
        Success = 1,
        Warning = 2,
        Error = 3
    }
}
