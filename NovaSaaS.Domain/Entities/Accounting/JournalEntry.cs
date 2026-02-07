using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Accounting
{
    /// <summary>
    /// JournalEntry - Bút toán kế toán.
    /// </summary>
    public class JournalEntry : BaseEntity
    {
        /// <summary>
        /// Số bút toán (tự sinh).
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string EntryNumber { get; set; } = string.Empty;

        /// <summary>
        /// Ngày hạch toán.
        /// </summary>
        public DateTime EntryDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// ID kỳ kế toán.
        /// </summary>
        public Guid FiscalPeriodId { get; set; }
        public virtual FiscalPeriod FiscalPeriod { get; set; } = null!;

        /// <summary>
        /// Loại bút toán.
        /// </summary>
        public JournalEntryType EntryType { get; set; } = JournalEntryType.Manual;

        /// <summary>
        /// Trạng thái bút toán.
        /// </summary>
        public JournalEntryStatus Status { get; set; } = JournalEntryStatus.Draft;

        /// <summary>
        /// Diễn giải / Mô tả bút toán.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Tổng số tiền bên Nợ.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal TotalDebit { get; set; }

        /// <summary>
        /// Tổng số tiền bên Có.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal TotalCredit { get; set; }

        /// <summary>
        /// Mã tham chiếu (Số hóa đơn, Số PO...).
        /// </summary>
        [MaxLength(100)]
        public string? ReferenceNumber { get; set; }

        /// <summary>
        /// ID tham chiếu (OrderId, InvoiceId...).
        /// </summary>
        public Guid? ReferenceId { get; set; }

        /// <summary>
        /// Loại tham chiếu (Order, Invoice, Payroll...).
        /// </summary>
        [MaxLength(50)]
        public string? ReferenceType { get; set; }

        /// <summary>
        /// Người duyệt.
        /// </summary>
        public Guid? ApprovedByUserId { get; set; }

        /// <summary>
        /// Ngày duyệt.
        /// </summary>
        public DateTime? ApprovedAt { get; set; }

        /// <summary>
        /// ID bút toán gốc (nếu đây là bút toán đảo).
        /// </summary>
        public Guid? ReversedFromId { get; set; }
        public virtual JournalEntry? ReversedFrom { get; set; }

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        // Navigation
        public virtual ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
    }

    /// <summary>
    /// JournalEntryLine - Dòng chi tiết bút toán (Nợ / Có).
    /// </summary>
    public class JournalEntryLine : BaseEntity
    {
        /// <summary>
        /// ID bút toán.
        /// </summary>
        public Guid JournalEntryId { get; set; }
        public virtual JournalEntry JournalEntry { get; set; } = null!;

        /// <summary>
        /// ID tài khoản kế toán.
        /// </summary>
        public Guid AccountId { get; set; }
        public virtual ChartOfAccount Account { get; set; } = null!;

        /// <summary>
        /// Số tiền bên Nợ.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal DebitAmount { get; set; } = 0;

        /// <summary>
        /// Số tiền bên Có.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal CreditAmount { get; set; } = 0;

        /// <summary>
        /// Diễn giải dòng.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Đối tượng công nợ (Customer/Vendor).
        /// </summary>
        public Guid? PartnerEntityId { get; set; }

        /// <summary>
        /// Loại đối tượng (Customer, Vendor, Employee).
        /// </summary>
        [MaxLength(50)]
        public string? PartnerEntityType { get; set; }
    }
}
