using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Accounting
{
    /// <summary>
    /// AccountPayable - Phải trả (công nợ nhà cung cấp).
    /// </summary>
    public class AccountPayable : BaseEntity
    {
        /// <summary>
        /// Mã phải trả.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string PayableNumber { get; set; } = string.Empty;

        /// <summary>
        /// ID nhà cung cấp (từ SCM module).
        /// </summary>
        public Guid VendorId { get; set; }

        /// <summary>
        /// ID hóa đơn mua hàng liên quan.
        /// </summary>
        public Guid? PurchaseInvoiceId { get; set; }

        /// <summary>
        /// Mã tham chiếu.
        /// </summary>
        [MaxLength(50)]
        public string? ReferenceNumber { get; set; }

        /// <summary>
        /// Ngày phát sinh.
        /// </summary>
        public DateTime IssueDate { get; set; }

        /// <summary>
        /// Ngày đáo hạn.
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Tổng số tiền phải trả.
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
        /// Trạng thái.
        /// </summary>
        public PayableStatus Status { get; set; } = PayableStatus.Open;

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
    }
}
