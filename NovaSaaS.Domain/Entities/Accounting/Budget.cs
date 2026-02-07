using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Accounting
{
    /// <summary>
    /// Budget - Ngân sách.
    /// </summary>
    public class Budget : BaseEntity
    {
        /// <summary>
        /// Mã ngân sách.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên ngân sách.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// ID năm tài chính.
        /// </summary>
        public Guid FiscalYearId { get; set; }
        public virtual FiscalYear FiscalYear { get; set; } = null!;

        /// <summary>
        /// Ngày bắt đầu.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Tổng ngân sách dự kiến.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal TotalBudgetAmount { get; set; }

        /// <summary>
        /// Tổng đã chi thực tế.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal TotalActualAmount { get; set; } = 0;

        /// <summary>
        /// Trạng thái.
        /// </summary>
        public BudgetStatus Status { get; set; } = BudgetStatus.Draft;

        /// <summary>
        /// Phòng ban liên quan (nếu ngân sách theo phòng).
        /// </summary>
        public Guid? DepartmentId { get; set; }

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        // Navigation
        public virtual ICollection<BudgetLine> Lines { get; set; } = new List<BudgetLine>();
    }

    /// <summary>
    /// BudgetLine - Chi tiết ngân sách theo tài khoản.
    /// </summary>
    public class BudgetLine : BaseEntity
    {
        /// <summary>
        /// ID ngân sách.
        /// </summary>
        public Guid BudgetId { get; set; }
        public virtual Budget Budget { get; set; } = null!;

        /// <summary>
        /// ID tài khoản kế toán.
        /// </summary>
        public Guid AccountId { get; set; }
        public virtual ChartOfAccount Account { get; set; } = null!;

        /// <summary>
        /// Mô tả khoản mục.
        /// </summary>
        [MaxLength(200)]
        public string? Description { get; set; }

        /// <summary>
        /// Số tiền dự kiến.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal BudgetAmount { get; set; }

        /// <summary>
        /// Số tiền thực tế.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal ActualAmount { get; set; } = 0;

        /// <summary>
        /// Chênh lệch = Budget - Actual.
        /// </summary>
        public decimal Variance => BudgetAmount - ActualAmount;

        /// <summary>
        /// Tỷ lệ thực hiện (%).
        /// </summary>
        public decimal? PercentUsed => BudgetAmount > 0 ? (ActualAmount / BudgetAmount) * 100 : null;
    }
}
