using NovaSaaS.Domain.Entities.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.HRM
{
    /// <summary>
    /// LeaveBalance - Số dư ngày phép của nhân viên theo loại và năm.
    /// </summary>
    public class LeaveBalance : BaseEntity
    {
        /// <summary>
        /// ID nhân viên.
        /// </summary>
        public Guid EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;

        /// <summary>
        /// ID loại nghỉ phép.
        /// </summary>
        public Guid LeaveTypeId { get; set; }
        public virtual LeaveType LeaveType { get; set; } = null!;

        /// <summary>
        /// Năm áp dụng.
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Tổng ngày phép được cấp.
        /// </summary>
        [Range(0, 365)]
        public decimal TotalDays { get; set; }

        /// <summary>
        /// Ngày phép đã sử dụng.
        /// </summary>
        [Range(0, 365)]
        public decimal UsedDays { get; set; } = 0;

        /// <summary>
        /// Ngày phép chuyển từ năm trước.
        /// </summary>
        [Range(0, 365)]
        public decimal CarriedForwardDays { get; set; } = 0;

        /// <summary>
        /// Ngày phép còn lại.
        /// </summary>
        public decimal RemainingDays => TotalDays + CarriedForwardDays - UsedDays;
    }
}
