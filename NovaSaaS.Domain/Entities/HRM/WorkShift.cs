using NovaSaaS.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.HRM
{
    /// <summary>
    /// WorkShift - Ca làm việc.
    /// </summary>
    public class WorkShift : BaseEntity
    {
        /// <summary>
        /// Mã ca.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên ca làm việc (VD: Ca sáng, Ca chiều, Ca đêm).
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Giờ bắt đầu.
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// Giờ kết thúc.
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// Thời gian nghỉ trưa (phút).
        /// </summary>
        public int BreakMinutes { get; set; } = 60;

        /// <summary>
        /// Số giờ làm việc thực tế (trừ break).
        /// </summary>
        public decimal WorkHours { get; set; } = 8;

        /// <summary>
        /// Cho phép trễ tối đa (phút).
        /// </summary>
        public int LateToleranceMinutes { get; set; } = 15;

        /// <summary>
        /// Cho phép về sớm tối đa (phút).
        /// </summary>
        public int EarlyLeaveToleranceMinutes { get; set; } = 15;

        /// <summary>
        /// Ca áp dụng cho thứ nào trong tuần (JSON: [1,2,3,4,5] = Thứ 2 đến Thứ 6).
        /// </summary>
        [MaxLength(50)]
        public string? ApplicableDays { get; set; }

        /// <summary>
        /// Trạng thái hoạt động.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Mô tả.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        // Navigation
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
}
