using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Accounting
{
    /// <summary>
    /// TaxRate - Thuế suất.
    /// </summary>
    public class TaxRate : BaseEntity
    {
        /// <summary>
        /// Mã thuế (VD: VAT10, VAT5, TNCN).
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên thuế suất.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Loại thuế.
        /// </summary>
        public TaxType TaxType { get; set; } = TaxType.VAT;

        /// <summary>
        /// Tỷ lệ thuế (%).
        /// </summary>
        [Range(0, 100)]
        public decimal Rate { get; set; }

        /// <summary>
        /// Mô tả.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Tài khoản kế toán thuế đầu ra.
        /// </summary>
        public Guid? OutputAccountId { get; set; }
        public virtual ChartOfAccount? OutputAccount { get; set; }

        /// <summary>
        /// Tài khoản kế toán thuế đầu vào.
        /// </summary>
        public Guid? InputAccountId { get; set; }
        public virtual ChartOfAccount? InputAccount { get; set; }

        /// <summary>
        /// Trạng thái hoạt động.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual ICollection<TaxTransaction> TaxTransactions { get; set; } = new List<TaxTransaction>();
    }

    /// <summary>
    /// TaxTransaction - Giao dịch thuế.
    /// </summary>
    public class TaxTransaction : BaseEntity
    {
        /// <summary>
        /// ID thuế suất.
        /// </summary>
        public Guid TaxRateId { get; set; }
        public virtual TaxRate TaxRate { get; set; } = null!;

        /// <summary>
        /// Ngày giao dịch.
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Số tiền trước thuế.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal BaseAmount { get; set; }

        /// <summary>
        /// Số tiền thuế.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Thuế đầu ra (true) hay đầu vào (false).
        /// </summary>
        public bool IsOutput { get; set; }

        /// <summary>
        /// Mã tham chiếu (Số hóa đơn).
        /// </summary>
        [MaxLength(50)]
        public string? ReferenceNumber { get; set; }

        /// <summary>
        /// ID tham chiếu.
        /// </summary>
        public Guid? ReferenceId { get; set; }

        /// <summary>
        /// Loại tham chiếu.
        /// </summary>
        [MaxLength(50)]
        public string? ReferenceType { get; set; }

        /// <summary>
        /// ID bút toán liên kết.
        /// </summary>
        public Guid? JournalEntryId { get; set; }

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
