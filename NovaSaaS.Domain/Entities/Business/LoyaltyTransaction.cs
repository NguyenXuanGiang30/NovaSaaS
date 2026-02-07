using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Business
{
    /// <summary>
    /// LoyaltyTransaction - Giao dịch điểm thưởng.
    /// </summary>
    public class LoyaltyTransaction : BaseEntity
    {
        /// <summary>
        /// ID chương trình loyalty.
        /// </summary>
        public Guid LoyaltyProgramId { get; set; }
        public virtual LoyaltyProgram LoyaltyProgram { get; set; } = null!;

        /// <summary>
        /// ID khách hàng.
        /// </summary>
        public Guid CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = null!;

        /// <summary>
        /// Loại giao dịch.
        /// </summary>
        public LoyaltyTransactionType Type { get; set; }

        /// <summary>
        /// Số điểm (dương = tích, âm = tiêu/hết hạn).
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// Số dư điểm sau giao dịch.
        /// </summary>
        public int BalanceAfter { get; set; }

        /// <summary>
        /// Mã tham chiếu (OrderId, etc.).
        /// </summary>
        [MaxLength(100)]
        public string? ReferenceCode { get; set; }

        /// <summary>
        /// ID tham chiếu.
        /// </summary>
        public Guid? ReferenceId { get; set; }

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// Ngày hết hạn điểm.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }
}
