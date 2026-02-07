using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.PM
{
    /// <summary>
    /// ProjectExpense - Chi phí dự án.
    /// </summary>
    public class ProjectExpense : BaseEntity
    {
        /// <summary>
        /// ID dự án.
        /// </summary>
        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;

        /// <summary>
        /// Tên khoản chi phí.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả.
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Số tiền.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        /// <summary>
        /// Ngày phát sinh.
        /// </summary>
        public DateTime ExpenseDate { get; set; }

        /// <summary>
        /// Trạng thái.
        /// </summary>
        public ProjectExpenseStatus Status { get; set; } = ProjectExpenseStatus.Pending;

        /// <summary>
        /// Người yêu cầu.
        /// </summary>
        public Guid? RequestedByUserId { get; set; }

        /// <summary>
        /// Người duyệt.
        /// </summary>
        public Guid? ApprovedByUserId { get; set; }

        /// <summary>
        /// File đính kèm (hóa đơn).
        /// </summary>
        [MaxLength(500)]
        public string? AttachmentUrl { get; set; }

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
