using NovaSaaS.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.HRM
{
    /// <summary>
    /// LeaveType - Loại nghỉ phép (Phép năm, Ốm, Thai sản...).
    /// </summary>
    public class LeaveType : BaseEntity
    {
        /// <summary>
        /// Mã loại nghỉ.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên loại nghỉ phép.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Số ngày mặc định/năm.
        /// </summary>
        public int DefaultDaysPerYear { get; set; } = 12;

        /// <summary>
        /// Có trả lương hay không.
        /// </summary>
        public bool IsPaid { get; set; } = true;

        /// <summary>
        /// Có yêu cầu duyệt hay không.
        /// </summary>
        public bool RequiresApproval { get; set; } = true;

        /// <summary>
        /// Cho phép chuyển ngày phép sang năm sau.
        /// </summary>
        public bool AllowCarryForward { get; set; } = false;

        /// <summary>
        /// Số ngày tối đa được chuyển.
        /// </summary>
        public int MaxCarryForwardDays { get; set; } = 0;

        /// <summary>
        /// Trạng thái hoạt động.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
        public virtual ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
    }
}
