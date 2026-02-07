using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.PM
{
    /// <summary>
    /// ProjectTask - Công việc trong dự án.
    /// </summary>
    public class ProjectTask : BaseEntity
    {
        /// <summary>
        /// ID dự án.
        /// </summary>
        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;

        /// <summary>
        /// ID giai đoạn (tùy chọn).
        /// </summary>
        public Guid? ProjectPhaseId { get; set; }
        public virtual ProjectPhase? ProjectPhase { get; set; }

        /// <summary>
        /// ID task cha (hỗ trợ sub-task).
        /// </summary>
        public Guid? ParentTaskId { get; set; }
        public virtual ProjectTask? ParentTask { get; set; }

        /// <summary>
        /// Tiêu đề công việc.
        /// </summary>
        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả chi tiết.
        /// </summary>
        [MaxLength(5000)]
        public string? Description { get; set; }

        /// <summary>
        /// Trạng thái.
        /// </summary>
        public ProjectTaskStatus Status { get; set; } = ProjectTaskStatus.Backlog;

        /// <summary>
        /// Độ ưu tiên.
        /// </summary>
        public ProjectTaskPriority Priority { get; set; } = ProjectTaskPriority.Medium;

        /// <summary>
        /// Người được giao.
        /// </summary>
        public Guid? AssigneeUserId { get; set; }

        /// <summary>
        /// Người tạo / phân công.
        /// </summary>
        public Guid? ReporterUserId { get; set; }

        /// <summary>
        /// Ngày bắt đầu.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Ngày đến hạn.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Ngày hoàn thành.
        /// </summary>
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// Ước tính effort (giờ).
        /// </summary>
        [Range(0, 10000)]
        public decimal EstimatedHours { get; set; } = 0;

        /// <summary>
        /// Thực tế effort (giờ).
        /// </summary>
        [Range(0, 10000)]
        public decimal ActualHours { get; set; } = 0;

        /// <summary>
        /// Tiến độ (%).
        /// </summary>
        [Range(0, 100)]
        public decimal Progress { get; set; } = 0;

        /// <summary>
        /// Thứ tự hiển thị.
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// Tag / Label.
        /// </summary>
        [MaxLength(200)]
        public string? Tags { get; set; }

        // Navigation
        public virtual ICollection<ProjectTask> SubTasks { get; set; } = new List<ProjectTask>();
        public virtual ICollection<TaskDependency> Dependencies { get; set; } = new List<TaskDependency>();
        public virtual ICollection<TaskDependency> Dependents { get; set; } = new List<TaskDependency>();
        public virtual ICollection<ProjectComment> Comments { get; set; } = new List<ProjectComment>();
        public virtual ICollection<TimesheetEntry> TimesheetEntries { get; set; } = new List<TimesheetEntry>();
    }
}
