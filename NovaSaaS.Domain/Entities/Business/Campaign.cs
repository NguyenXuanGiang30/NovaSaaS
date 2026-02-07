using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Business
{
    /// <summary>
    /// Campaign - Chiến dịch marketing.
    /// </summary>
    public class Campaign : BaseEntity
    {
        /// <summary>
        /// Tên chiến dịch.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả chiến dịch.
        /// </summary>
        [MaxLength(2000)]
        public string? Description { get; set; }

        /// <summary>
        /// Loại chiến dịch.
        /// </summary>
        public CampaignType Type { get; set; } = CampaignType.Promotion;

        /// <summary>
        /// Trạng thái.
        /// </summary>
        public CampaignStatus Status { get; set; } = CampaignStatus.Draft;

        /// <summary>
        /// Ngày bắt đầu.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Ngân sách (VND).
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal Budget { get; set; } = 0;

        /// <summary>
        /// Chi phí thực tế (VND).
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal ActualCost { get; set; } = 0;

        /// <summary>
        /// Doanh thu tạo ra (VND).
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal Revenue { get; set; } = 0;

        /// <summary>
        /// Số lượng Lead tạo ra.
        /// </summary>
        public int LeadsGenerated { get; set; } = 0;

        /// <summary>
        /// Số lượng đơn hàng tạo ra.
        /// </summary>
        public int OrdersGenerated { get; set; } = 0;

        /// <summary>
        /// ID Coupon liên kết (tùy chọn).
        /// </summary>
        public Guid? CouponId { get; set; }
        public virtual Coupon? Coupon { get; set; }

        /// <summary>
        /// Nhóm khách hàng mục tiêu.
        /// </summary>
        public Guid? TargetCustomerGroupId { get; set; }
        public virtual CustomerGroup? TargetCustomerGroup { get; set; }

        /// <summary>
        /// Ghi chú nội bộ.
        /// </summary>
        [MaxLength(1000)]
        public string? InternalNotes { get; set; }
    }
}
