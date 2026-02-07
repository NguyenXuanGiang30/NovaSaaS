using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Business
{
    /// <summary>
    /// Coupon - Mã khuyến mãi / giảm giá.
    /// </summary>
    public class Coupon : BaseEntity
    {
        /// <summary>
        /// Mã coupon (duy nhất).
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên coupon.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả chi tiết.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Loại giảm giá: Percentage hoặc FixedAmount.
        /// </summary>
        public DiscountType DiscountType { get; set; } = DiscountType.Percentage;

        /// <summary>
        /// Giá trị giảm (% hoặc số tiền tùy DiscountType).
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal DiscountValue { get; set; }

        /// <summary>
        /// Giá trị giảm tối đa (chỉ áp dụng cho Percentage).
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal? MaxDiscountAmount { get; set; }

        /// <summary>
        /// Giá trị đơn hàng tối thiểu để áp dụng.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal MinOrderAmount { get; set; } = 0;

        /// <summary>
        /// Số lần sử dụng tối đa (null = không giới hạn).
        /// </summary>
        public int? MaxUsageCount { get; set; }

        /// <summary>
        /// Số lần sử dụng tối đa mỗi khách hàng.
        /// </summary>
        public int? MaxUsagePerCustomer { get; set; }

        /// <summary>
        /// Số lần đã sử dụng.
        /// </summary>
        public int UsedCount { get; set; } = 0;

        /// <summary>
        /// Ngày bắt đầu hiệu lực.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ngày hết hạn.
        /// </summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Trạng thái hoạt động.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Áp dụng cho nhóm khách hàng cụ thể (null = tất cả).
        /// </summary>
        public Guid? CustomerGroupId { get; set; }
        public virtual CustomerGroup? CustomerGroup { get; set; }

        /// <summary>
        /// Coupon còn hiệu lực hay không.
        /// </summary>
        public bool IsValid => IsActive
            && DateTime.UtcNow >= StartDate
            && DateTime.UtcNow <= ExpiryDate
            && (!MaxUsageCount.HasValue || UsedCount < MaxUsageCount.Value);
    }

    /// <summary>
    /// Loại giảm giá.
    /// </summary>
    public enum DiscountType
    {
        Percentage = 0,
        FixedAmount = 1
    }
}
