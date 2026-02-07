using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.HRM
{
    /// <summary>
    /// Attendance - Chấm công nhân viên.
    /// </summary>
    public class Attendance : BaseEntity
    {
        /// <summary>
        /// ID nhân viên.
        /// </summary>
        public Guid EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;

        /// <summary>
        /// Ngày chấm công.
        /// </summary>
        public DateTime WorkDate { get; set; }

        /// <summary>
        /// ID ca làm việc.
        /// </summary>
        public Guid? WorkShiftId { get; set; }
        public virtual WorkShift? WorkShift { get; set; }

        /// <summary>
        /// Giờ check-in.
        /// </summary>
        public DateTime? CheckInTime { get; set; }

        /// <summary>
        /// Giờ check-out.
        /// </summary>
        public DateTime? CheckOutTime { get; set; }

        /// <summary>
        /// Trạng thái chấm công.
        /// </summary>
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

        /// <summary>
        /// Số giờ làm việc thực tế.
        /// </summary>
        [Range(0, 24)]
        public decimal ActualWorkHours { get; set; } = 0;

        /// <summary>
        /// Số giờ tăng ca.
        /// </summary>
        [Range(0, 24)]
        public decimal OvertimeHours { get; set; } = 0;

        /// <summary>
        /// Số phút đi trễ.
        /// </summary>
        public int LateMinutes { get; set; } = 0;

        /// <summary>
        /// Số phút về sớm.
        /// </summary>
        public int EarlyLeaveMinutes { get; set; } = 0;

        /// <summary>
        /// IP check-in (nếu check qua web/app).
        /// </summary>
        [MaxLength(50)]
        public string? CheckInIp { get; set; }

        /// <summary>
        /// Tọa độ check-in (GPS).
        /// </summary>
        [MaxLength(50)]
        public string? CheckInLocation { get; set; }

        /// <summary>
        /// Đã duyệt chấm công hay chưa.
        /// </summary>
        public bool IsApproved { get; set; } = false;

        /// <summary>
        /// Người duyệt.
        /// </summary>
        public Guid? ApprovedByUserId { get; set; }

        /// <summary>
        /// Ghi chú / Lý do điều chỉnh.
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
