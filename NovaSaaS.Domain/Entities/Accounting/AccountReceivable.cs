using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Accounting
{
    /// <summary>
    /// AccountReceivable - Phải thu (công nợ khách hàng).
    /// </summary>
    public class AccountReceivable : BaseEntity
    {
        /// <summary>
        /// Mã phải thu.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string ReceivableNumber { get; set; } = string.Empty;

        /// <summary>
        /// ID khách hàng (từ CRM module).
        /// </summary>
        public Guid CustomerId { get; set; }

        /// <summary>
        /// ID hóa đơn liên quan.
        /// </summary>
        public Guid? InvoiceId { get; set; }

        /// <summary>
        /// Mã tham chiếu (Số hóa đơn).
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
        /// Tổng số tiền phải thu.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Số tiền đã thu.
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
        public ReceivableStatus Status { get; set; } = ReceivableStatus.Open;

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

        /// <summary>
        /// Số ngày quá hạn.
        /// </summary>
        public int DaysOverdue => Status == ReceivableStatus.Overdue
            ? (int)(DateTime.UtcNow - DueDate).TotalDays
            : 0;
    }
}
