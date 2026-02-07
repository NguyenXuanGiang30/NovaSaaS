using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Accounting
{
    /// <summary>
    /// ChartOfAccount - Hệ thống tài khoản kế toán (Plan comptable).
    /// Hỗ trợ cấu trúc cây phân cấp tài khoản.
    /// </summary>
    public class ChartOfAccount : BaseEntity
    {
        /// <summary>
        /// Số hiệu tài khoản (VD: 111, 112, 131, 511...).
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// Tên tài khoản.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string AccountName { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Loại tài khoản (Tài sản, Nợ phải trả, Vốn, Doanh thu, Chi phí).
        /// </summary>
        public AccountCategory Category { get; set; }

        /// <summary>
        /// ID tài khoản cha (hỗ trợ cây).
        /// </summary>
        public Guid? ParentAccountId { get; set; }
        public virtual ChartOfAccount? ParentAccount { get; set; }

        /// <summary>
        /// Cấp tài khoản (1 = cấp 1, 2 = cấp 2...).
        /// </summary>
        public int Level { get; set; } = 1;

        /// <summary>
        /// Là tài khoản chi tiết (có thể ghi sổ) hay tài khoản tổng hợp.
        /// </summary>
        public bool IsDetailAccount { get; set; } = true;

        /// <summary>
        /// Bên ghi Nợ bình thường (true) hay bên Có (false).
        /// </summary>
        public bool IsDebitNormal { get; set; } = true;

        /// <summary>
        /// Số dư hiện tại (cache).
        /// </summary>
        public decimal CurrentBalance { get; set; } = 0;

        /// <summary>
        /// Trạng thái hoạt động.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual ICollection<ChartOfAccount> ChildAccounts { get; set; } = new List<ChartOfAccount>();
        public virtual ICollection<JournalEntryLine> JournalEntryLines { get; set; } = new List<JournalEntryLine>();
    }
}
