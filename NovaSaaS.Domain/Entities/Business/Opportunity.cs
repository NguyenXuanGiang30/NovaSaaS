using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Business
{
    /// <summary>
    /// Opportunity - Cơ hội bán hàng trong pipeline.
    /// </summary>
    public class Opportunity : BaseEntity
    {
        /// <summary>
        /// Tên cơ hội.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// ID khách hàng liên quan.
        /// </summary>
        public Guid CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = null!;

        /// <summary>
        /// Giai đoạn trong pipeline.
        /// </summary>
        public OpportunityStage Stage { get; set; } = OpportunityStage.Qualification;

        /// <summary>
        /// Giá trị cơ hội (VND).
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal Value { get; set; } = 0;

        /// <summary>
        /// Xác suất thắng (%).
        /// </summary>
        [Range(0, 100)]
        public int Probability { get; set; } = 0;

        /// <summary>
        /// Ngày dự kiến chốt.
        /// </summary>
        public DateTime? ExpectedCloseDate { get; set; }

        /// <summary>
        /// Ngày chốt thực tế.
        /// </summary>
        public DateTime? ActualCloseDate { get; set; }

        /// <summary>
        /// Người phụ trách.
        /// </summary>
        public Guid? AssignedToUserId { get; set; }

        /// <summary>
        /// Lý do thua (nếu ClosedLost).
        /// </summary>
        [MaxLength(500)]
        public string? LostReason { get; set; }

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(2000)]
        public string? Notes { get; set; }

        /// <summary>
        /// ID đơn hàng nếu đã chuyển đổi.
        /// </summary>
        public Guid? ConvertedOrderId { get; set; }
        public virtual Order? ConvertedOrder { get; set; }

        // Navigation
        public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();
        public virtual ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();
    }
}
