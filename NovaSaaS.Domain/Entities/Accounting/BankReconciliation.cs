using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Accounting
{
    /// <summary>
    /// BankReconciliation - Đối chiếu ngân hàng.
    /// </summary>
    public class BankReconciliation : BaseEntity
    {
        /// <summary>
        /// ID tài khoản ngân hàng.
        /// </summary>
        public Guid BankAccountId { get; set; }
        public virtual BankAccount BankAccount { get; set; } = null!;

        /// <summary>
        /// Ngày bắt đầu kỳ đối chiếu.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc kỳ đối chiếu.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Số dư đầu kỳ (theo sổ sách).
        /// </summary>
        public decimal OpeningBalanceBook { get; set; }

        /// <summary>
        /// Số dư đầu kỳ (theo ngân hàng).
        /// </summary>
        public decimal OpeningBalanceBank { get; set; }

        /// <summary>
        /// Số dư cuối kỳ (theo sổ sách).
        /// </summary>
        public decimal ClosingBalanceBook { get; set; }

        /// <summary>
        /// Số dư cuối kỳ (theo ngân hàng).
        /// </summary>
        public decimal ClosingBalanceBank { get; set; }

        /// <summary>
        /// Chênh lệch.
        /// </summary>
        public decimal Difference => ClosingBalanceBank - ClosingBalanceBook;

        /// <summary>
        /// Trạng thái.
        /// </summary>
        public ReconciliationStatus Status { get; set; } = ReconciliationStatus.Pending;

        /// <summary>
        /// Người thực hiện.
        /// </summary>
        public Guid? ReconciledByUserId { get; set; }

        /// <summary>
        /// Ngày hoàn thành.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
