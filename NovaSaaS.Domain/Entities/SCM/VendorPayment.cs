using NovaSaaS.Domain.Entities.Accounting;
using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.SCM
{
    /// <summary>
    /// VendorPayment - Thanh toán cho nhà cung cấp.
    /// </summary>
    public class VendorPayment : BaseEntity
    {
        /// <summary>
        /// Mã thanh toán.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string PaymentNumber { get; set; } = string.Empty;

        /// <summary>
        /// ID hóa đơn mua hàng.
        /// </summary>
        public Guid PurchaseInvoiceId { get; set; }
        public virtual PurchaseInvoice PurchaseInvoice { get; set; } = null!;

        /// <summary>
        /// ID nhà cung cấp.
        /// </summary>
        public Guid VendorId { get; set; }
        public virtual Vendor Vendor { get; set; } = null!;

        /// <summary>
        /// Ngày thanh toán.
        /// </summary>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Số tiền thanh toán.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        /// <summary>
        /// Phương thức thanh toán.
        /// </summary>
        public VendorPaymentMethod PaymentMethod { get; set; } = VendorPaymentMethod.BankTransfer;

        /// <summary>
        /// Trạng thái.
        /// </summary>
        public VendorPaymentStatus Status { get; set; } = VendorPaymentStatus.Pending;

        /// <summary>
        /// ID tài khoản ngân hàng thanh toán (liên kết ACC module).
        /// </summary>
        public Guid? BankAccountId { get; set; }
        public virtual BankAccount? BankAccount { get; set; }

        /// <summary>
        /// Mã giao dịch ngân hàng.
        /// </summary>
        [MaxLength(100)]
        public string? BankTransactionReference { get; set; }

        /// <summary>
        /// Loại tiền tệ.
        /// </summary>
        [MaxLength(10)]
        public string Currency { get; set; } = "VND";

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
