using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Business
{
    /// <summary>
    /// Activity - Hoạt động CRM (cuộc gọi, email, cuộc họp...).
    /// </summary>
    public class Activity : BaseEntity
    {
        /// <summary>
        /// Tiêu đề hoạt động.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Loại hoạt động.
        /// </summary>
        public ActivityType Type { get; set; }

        /// <summary>
        /// Trạng thái.
        /// </summary>
        public ActivityStatus Status { get; set; } = ActivityStatus.Planned;

        /// <summary>
        /// Mô tả / Nội dung.
        /// </summary>
        [MaxLength(2000)]
        public string? Description { get; set; }

        /// <summary>
        /// Thời gian bắt đầu.
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// Thời gian kết thúc.
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Thời gian nhắc nhở.
        /// </summary>
        public DateTime? ReminderAt { get; set; }

        /// <summary>
        /// Người thực hiện.
        /// </summary>
        public Guid? AssignedToUserId { get; set; }

        /// <summary>
        /// ID khách hàng liên quan (tùy chọn).
        /// </summary>
        public Guid? CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }

        /// <summary>
        /// ID Lead liên quan (tùy chọn).
        /// </summary>
        public Guid? LeadId { get; set; }
        public virtual Lead? Lead { get; set; }

        /// <summary>
        /// ID Opportunity liên quan (tùy chọn).
        /// </summary>
        public Guid? OpportunityId { get; set; }
        public virtual Opportunity? Opportunity { get; set; }

        /// <summary>
        /// Kết quả / Ghi chú sau thực hiện.
        /// </summary>
        [MaxLength(2000)]
        public string? Outcome { get; set; }
    }
}
