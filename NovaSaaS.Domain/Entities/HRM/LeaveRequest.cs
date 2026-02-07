using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.HRM
{
    /// <summary>
    /// LeaveRequest - Đơn xin nghỉ phép.
    /// </summary>
    public class LeaveRequest : BaseEntity
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
        /// Ngày bắt đầu nghỉ.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc nghỉ.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Tổng số ngày nghỉ.
        /// </summary>
        [Range(0.5, 365)]
        public decimal TotalDays { get; set; }

        /// <summary>
        /// Lý do xin nghỉ.
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Trạng thái đơn.
        /// </summary>
        public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;

        /// <summary>
        /// Người duyệt (quản lý trực tiếp).
        /// </summary>
        public Guid? ApprovedByUserId { get; set; }

        /// <summary>
        /// Ngày duyệt.
        /// </summary>
        public DateTime? ApprovedAt { get; set; }

        /// <summary>
        /// Lý do từ chối (nếu bị reject).
        /// </summary>
        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// File đính kèm (giấy khám bệnh, ...).
        /// </summary>
        [MaxLength(500)]
        public string? AttachmentUrl { get; set; }
    }
}
