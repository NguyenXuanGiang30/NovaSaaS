using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Accounting
{
    /// <summary>
    /// BankTransaction - Giao dịch ngân hàng.
    /// </summary>
    public class BankTransaction : BaseEntity
    {
        /// <summary>
        /// ID tài khoản ngân hàng.
        /// </summary>
        public Guid BankAccountId { get; set; }
        public virtual BankAccount BankAccount { get; set; } = null!;

        /// <summary>
        /// Ngày giao dịch.
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Loại giao dịch.
        /// </summary>
        public BankTransactionType TransactionType { get; set; }

        /// <summary>
        /// Số tiền.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Số dư sau giao dịch.
        /// </summary>
        public decimal BalanceAfter { get; set; }

        /// <summary>
        /// Mã tham chiếu từ ngân hàng.
        /// </summary>
        [MaxLength(100)]
        public string? BankReference { get; set; }

        /// <summary>
        /// Diễn giải.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Đối tác giao dịch (tên).
        /// </summary>
        [MaxLength(200)]
        public string? CounterpartyName { get; set; }

        /// <summary>
        /// Số tài khoản đối tác.
        /// </summary>
        [MaxLength(30)]
        public string? CounterpartyAccount { get; set; }

        /// <summary>
        /// Trạng thái đối chiếu.
        /// </summary>
        public ReconciliationStatus ReconciliationStatus { get; set; } = ReconciliationStatus.Pending;

        /// <summary>
        /// ID bút toán liên kết.
        /// </summary>
        public Guid? JournalEntryId { get; set; }

        /// <summary>
        /// Đã nhập từ sao kê ngân hàng (import).
        /// </summary>
        public bool IsImported { get; set; } = false;
    }
}
