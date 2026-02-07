using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.PM
{
    /// <summary>
    /// ProjectMilestone - Mốc quan trọng của dự án.
    /// </summary>
    public class ProjectMilestone : BaseEntity
    {
        /// <summary>
        /// ID dự án.
        /// </summary>
        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;

        /// <summary>
        /// Tên milestone.
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
        /// Ngày đến hạn.
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Ngày hoàn thành thực tế.
        /// </summary>
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// Trạng thái.
        /// </summary>
        public MilestoneStatus Status { get; set; } = MilestoneStatus.Pending;

        /// <summary>
        /// Người phụ trách.
        /// </summary>
        public Guid? OwnerUserId { get; set; }
    }
}
