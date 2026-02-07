using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.PM
{
    /// <summary>
    /// Timesheet - Bảng chấm công dự án (theo tuần/tháng).
    /// </summary>
    public class Timesheet : BaseEntity
    {
        /// <summary>
        /// ID User.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// ID dự án.
        /// </summary>
        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;

        /// <summary>
        /// Ngày bắt đầu kỳ (tuần/tháng).
        /// </summary>
        public DateTime PeriodStart { get; set; }

        /// <summary>
        /// Ngày kết thúc kỳ.
        /// </summary>
        public DateTime PeriodEnd { get; set; }

        /// <summary>
        /// Tổng giờ trong kỳ.
        /// </summary>
        [Range(0, 1000)]
        public decimal TotalHours { get; set; } = 0;

        /// <summary>
        /// Trạng thái.
        /// </summary>
        public TimesheetStatus Status { get; set; } = TimesheetStatus.Draft;

        /// <summary>
        /// Người duyệt.
        /// </summary>
        public Guid? ApprovedByUserId { get; set; }

        /// <summary>
        /// Ngày duyệt.
        /// </summary>
        public DateTime? ApprovedAt { get; set; }

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation
        public virtual ICollection<TimesheetEntry> Entries { get; set; } = new List<TimesheetEntry>();
    }

    /// <summary>
    /// TimesheetEntry - Chi tiết chấm công dự án theo ngày/task.
    /// </summary>
    public class TimesheetEntry : BaseEntity
    {
        /// <summary>
        /// ID timesheet.
        /// </summary>
        public Guid TimesheetId { get; set; }
        public virtual Timesheet Timesheet { get; set; } = null!;

        /// <summary>
        /// ID task.
        /// </summary>
        public Guid? ProjectTaskId { get; set; }
        public virtual ProjectTask? ProjectTask { get; set; }

        /// <summary>
        /// Ngày làm việc.
        /// </summary>
        public DateTime WorkDate { get; set; }

        /// <summary>
        /// Số giờ làm.
        /// </summary>
        [Range(0, 24)]
        public decimal Hours { get; set; }

        /// <summary>
        /// Mô tả công việc đã làm.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Có billable hay không.
        /// </summary>
        public bool IsBillable { get; set; } = true;
    }
}
