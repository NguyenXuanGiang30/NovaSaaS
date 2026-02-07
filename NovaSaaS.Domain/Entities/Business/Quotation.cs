using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Business
{
    /// <summary>
    /// Quotation - Báo giá cho khách hàng.
    /// </summary>
    public class Quotation : BaseEntity
    {
        /// <summary>
        /// Mã báo giá (tự động tạo).
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string QuotationNumber { get; set; } = string.Empty;

        /// <summary>
        /// ID khách hàng.
        /// </summary>
        public Guid CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = null!;

        /// <summary>
        /// ID cơ hội liên quan (tùy chọn).
        /// </summary>
        public Guid? OpportunityId { get; set; }
        public virtual Opportunity? Opportunity { get; set; }

        /// <summary>
        /// Trạng thái báo giá.
        /// </summary>
        public QuotationStatus Status { get; set; } = QuotationStatus.Draft;

        /// <summary>
        /// Tổng tiền hàng chưa thuế.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal SubTotal { get; set; }

        /// <summary>
        /// Thuế.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Giảm giá.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Tổng thanh toán.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Ngày hết hạn báo giá.
        /// </summary>
        public DateTime? ValidUntil { get; set; }

        /// <summary>
        /// Ghi chú / Điều khoản.
        /// </summary>
        [MaxLength(2000)]
        public string? Notes { get; set; }

        /// <summary>
        /// ID đơn hàng nếu đã chuyển đổi.
        /// </summary>
        public Guid? ConvertedOrderId { get; set; }
        public virtual Order? ConvertedOrder { get; set; }

        // Navigation
        public virtual ICollection<QuotationItem> Items { get; set; } = new List<QuotationItem>();
    }

    /// <summary>
    /// QuotationItem - Chi tiết mục trong báo giá.
    /// </summary>
    public class QuotationItem : BaseEntity
    {
        /// <summary>
        /// ID báo giá.
        /// </summary>
        public Guid QuotationId { get; set; }
        public virtual Quotation Quotation { get; set; } = null!;

        /// <summary>
        /// ID sản phẩm.
        /// </summary>
        public Guid ProductId { get; set; }
        public virtual Inventory.Product Product { get; set; } = null!;

        /// <summary>
        /// Số lượng.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        /// <summary>
        /// Đơn giá.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Giảm giá (%).
        /// </summary>
        [Range(0, 100)]
        public decimal DiscountPercent { get; set; } = 0;

        /// <summary>
        /// Ghi chú mục.
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
