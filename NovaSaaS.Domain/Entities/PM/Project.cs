using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.PM
{
    /// <summary>
    /// Project - Dự án.
    /// </summary>
    public class Project : BaseEntity
    {
        /// <summary>
        /// Mã dự án.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên dự án.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả dự án.
        /// </summary>
        [MaxLength(5000)]
        public string? Description { get; set; }

        /// <summary>
        /// Trạng thái.
        /// </summary>
        public ProjectStatus Status { get; set; } = ProjectStatus.Planning;

        /// <summary>
        /// Độ ưu tiên.
        /// </summary>
        public ProjectPriority Priority { get; set; } = ProjectPriority.Medium;

        /// <summary>
        /// Loại billing.
        /// </summary>
        public ProjectBillingType BillingType { get; set; } = ProjectBillingType.FixedPrice;

        /// <summary>
        /// Ngày bắt đầu dự kiến.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc dự kiến.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Ngày kết thúc thực tế.
        /// </summary>
        public DateTime? ActualEndDate { get; set; }

        /// <summary>
        /// Ngân sách dự kiến.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal Budget { get; set; } = 0;

        /// <summary>
        /// Chi phí thực tế.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal ActualCost { get; set; } = 0;

        /// <summary>
        /// Tiến độ tổng thể (%).
        /// </summary>
        [Range(0, 100)]
        public decimal Progress { get; set; } = 0;

        /// <summary>
        /// ID khách hàng (nếu dự án cho khách).
        /// </summary>
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// ID quản lý dự án.
        /// </summary>
        public Guid? ProjectManagerUserId { get; set; }

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(2000)]
        public string? Notes { get; set; }

        // Navigation
        public virtual ICollection<ProjectPhase> Phases { get; set; } = new List<ProjectPhase>();
        public virtual ICollection<ProjectMilestone> Milestones { get; set; } = new List<ProjectMilestone>();
        public virtual ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
        public virtual ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
        public virtual ICollection<ProjectExpense> Expenses { get; set; } = new List<ProjectExpense>();
    }
}
