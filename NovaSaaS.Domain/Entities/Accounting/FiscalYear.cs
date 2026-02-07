using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Accounting
{
    /// <summary>
    /// FiscalYear - Năm tài chính.
    /// </summary>
    public class FiscalYear : BaseEntity
    {
        /// <summary>
        /// Tên năm tài chính (VD: "Năm tài chính 2026").
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Năm.
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Ngày bắt đầu.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Trạng thái: Open, Closed, Locked.
        /// </summary>
        public FiscalPeriodStatus Status { get; set; } = FiscalPeriodStatus.Open;

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation
        public virtual ICollection<FiscalPeriod> Periods { get; set; } = new List<FiscalPeriod>();
    }

    /// <summary>
    /// FiscalPeriod - Kỳ kế toán (tháng, quý).
    /// </summary>
    public class FiscalPeriod : BaseEntity
    {
        /// <summary>
        /// ID năm tài chính.
        /// </summary>
        public Guid FiscalYearId { get; set; }
        public virtual FiscalYear FiscalYear { get; set; } = null!;

        /// <summary>
        /// Tên kỳ (VD: "Tháng 01/2026", "Quý 1/2026").
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Số thứ tự kỳ trong năm.
        /// </summary>
        public int PeriodNumber { get; set; }

        /// <summary>
        /// Ngày bắt đầu kỳ.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc kỳ.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Trạng thái.
        /// </summary>
        public FiscalPeriodStatus Status { get; set; } = FiscalPeriodStatus.Open;

        // Navigation
        public virtual ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
    }
}
