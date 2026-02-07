using NovaSaaS.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.PM
{
    /// <summary>
    /// ProjectPhase - Giai đoạn dự án.
    /// </summary>
    public class ProjectPhase : BaseEntity
    {
        /// <summary>
        /// ID dự án.
        /// </summary>
        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;

        /// <summary>
        /// Tên giai đoạn.
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
        /// Thứ tự giai đoạn.
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// Ngày bắt đầu.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Tiến độ (%).
        /// </summary>
        [Range(0, 100)]
        public decimal Progress { get; set; } = 0;

        // Navigation
        public virtual ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
    }
}
