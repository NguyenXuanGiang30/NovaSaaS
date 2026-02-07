using NovaSaaS.Domain.Entities.Business;
using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Entities.SCM;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Accounting
{
    /// <summary>
    /// BankAccount - Tài khoản ngân hàng của doanh nghiệp.
    /// </summary>
    public class BankAccount : BaseEntity
    {
        /// <summary>
        /// Tên tài khoản (hiển thị).
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string AccountName { get; set; } = string.Empty;

        /// <summary>
        /// Số tài khoản ngân hàng.
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// Tên ngân hàng.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string BankName { get; set; } = string.Empty;

        /// <summary>
        /// Chi nhánh ngân hàng.
        /// </summary>
        [MaxLength(100)]
        public string? BranchName { get; set; }

        /// <summary>
        /// Loại tiền tệ.
        /// </summary>
        [MaxLength(10)]
        public string Currency { get; set; } = "VND";

        /// <summary>
        /// Số dư hiện tại.
        /// </summary>
        public decimal CurrentBalance { get; set; } = 0;

        /// <summary>
        /// ID tài khoản kế toán liên kết.
        /// </summary>
        public Guid? ChartOfAccountId { get; set; }
        public virtual ChartOfAccount? ChartOfAccount { get; set; }

        /// <summary>
        /// Tài khoản mặc định cho thu chi.
        /// </summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// Trạng thái hoạt động.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation
        public virtual ICollection<BankTransaction> Transactions { get; set; } = new List<BankTransaction>();
        public virtual ICollection<BankReconciliation> Reconciliations { get; set; } = new List<BankReconciliation>();

        /// <summary>
        /// Thanh toán NCC qua tài khoản này (liên kết SCM module).
        /// </summary>
        public virtual ICollection<VendorPayment> VendorPayments { get; set; } = new List<VendorPayment>();

        /// <summary>
        /// Giao dịch thu tiền khách hàng qua tài khoản này (liên kết CRM module).
        /// </summary>
        public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
    }
}
