using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Entities.PM;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Business
{
    /// <summary>
    /// Invoice - Hóa đơn bán hàng.
    /// </summary>
    public class Invoice : BaseEntity
    {
        /// <summary>
        /// ID đơn hàng liên quan.
        /// </summary>
        public Guid OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;

        /// <summary>
        /// Mã hóa đơn.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string InvoiceNumber { get; set; } = string.Empty;

        /// <summary>
        /// Ngày xuất hóa đơn.
        /// </summary>
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

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
        /// Số tiền đã thanh toán.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal PaidAmount { get; set; } = 0;

        /// <summary>
        /// Số tiền còn lại.
        /// </summary>
        public decimal RemainingAmount => TotalAmount - PaidAmount;

        /// <summary>
        /// Trạng thái hóa đơn.
        /// </summary>
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

        /// <summary>
        /// Ngày đáo hạn.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Ngày thanh toán.
        /// </summary>
        public DateTime? PaidDate { get; set; }

        /// <summary>
        /// Phương thức thanh toán.
        /// </summary>
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.BankTransfer;

        /// <summary>
        /// ID dự án liên quan (liên kết PM module).
        /// </summary>
        public Guid? ProjectId { get; set; }
        public virtual Project? Project { get; set; }

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(2000)]
        public string? Notes { get; set; }

        // Navigation
        public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
    }
}
