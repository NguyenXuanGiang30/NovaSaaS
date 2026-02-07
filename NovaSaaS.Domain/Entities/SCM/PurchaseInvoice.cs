using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.SCM
{
    /// <summary>
    /// PurchaseInvoice - Hóa đơn mua hàng (từ NCC).
    /// </summary>
    public class PurchaseInvoice : BaseEntity
    {
        /// <summary>
        /// Mã hóa đơn mua hàng.
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string InvoiceNumber { get; set; } = string.Empty;

        /// <summary>
        /// ID nhà cung cấp.
        /// </summary>
        public Guid VendorId { get; set; }
        public virtual Vendor Vendor { get; set; } = null!;

        /// <summary>
        /// ID đơn đặt hàng liên quan.
        /// </summary>
        public Guid? PurchaseOrderId { get; set; }
        public virtual PurchaseOrder? PurchaseOrder { get; set; }

        /// <summary>
        /// Ngày hóa đơn.
        /// </summary>
        public DateTime InvoiceDate { get; set; }

        /// <summary>
        /// Ngày đáo hạn thanh toán.
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Trạng thái.
        /// </summary>
        public PurchaseInvoiceStatus Status { get; set; } = PurchaseInvoiceStatus.Draft;

        /// <summary>
        /// Tổng tiền hàng.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal SubTotal { get; set; }

        /// <summary>
        /// Thuế.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Tổng thanh toán.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Số tiền đã trả.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal PaidAmount { get; set; } = 0;

        /// <summary>
        /// Số tiền còn lại.
        /// </summary>
        public decimal RemainingAmount => TotalAmount - PaidAmount;

        /// <summary>
        /// Loại tiền tệ.
        /// </summary>
        [MaxLength(10)]
        public string Currency { get; set; } = "VND";

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        // Navigation
        public virtual ICollection<VendorPayment> Payments { get; set; } = new List<VendorPayment>();
    }
}
