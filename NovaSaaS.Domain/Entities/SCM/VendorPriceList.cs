using NovaSaaS.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.SCM
{
    /// <summary>
    /// VendorPriceList - Bảng giá của nhà cung cấp.
    /// Mỗi NCC có thể có nhiều bảng giá theo thời gian hoặc theo nhóm sản phẩm.
    /// </summary>
    public class VendorPriceList : BaseEntity
    {
        /// <summary>
        /// ID nhà cung cấp.
        /// </summary>
        public Guid VendorId { get; set; }
        public virtual Vendor Vendor { get; set; } = null!;

        /// <summary>
        /// Tên bảng giá (VD: "Bảng giá Q1/2026", "Giá sỉ").
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Ngày hiệu lực.
        /// </summary>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Ngày hết hạn (null = vô thời hạn).
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Loại tiền tệ.
        /// </summary>
        [MaxLength(10)]
        public string Currency { get; set; } = "VND";

        /// <summary>
        /// Trạng thái hoạt động.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        // Navigation
        public virtual ICollection<VendorPriceListItem> Items { get; set; } = new List<VendorPriceListItem>();
    }

    /// <summary>
    /// VendorPriceListItem - Chi tiết sản phẩm trong bảng giá NCC.
    /// </summary>
    public class VendorPriceListItem : BaseEntity
    {
        /// <summary>
        /// ID bảng giá.
        /// </summary>
        public Guid VendorPriceListId { get; set; }
        public virtual VendorPriceList VendorPriceList { get; set; } = null!;

        /// <summary>
        /// ID sản phẩm.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Đơn giá.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Số lượng tối thiểu để áp dụng giá này.
        /// </summary>
        public int MinQuantity { get; set; } = 1;

        /// <summary>
        /// Thời gian giao hàng (ngày).
        /// </summary>
        public int? LeadTimeDays { get; set; }

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
