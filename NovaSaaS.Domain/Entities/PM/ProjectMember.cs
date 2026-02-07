using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.PM
{
    /// <summary>
    /// ProjectMember - Thành viên tham gia dự án.
    /// </summary>
    public class ProjectMember : BaseEntity
    {
        /// <summary>
        /// ID dự án.
        /// </summary>
        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;

        /// <summary>
        /// ID User.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Vai trò trong dự án.
        /// </summary>
        public ProjectRole Role { get; set; } = ProjectRole.Member;

        /// <summary>
        /// Ngày tham gia.
        /// </summary>
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ngày rời (null = vẫn tham gia).
        /// </summary>
        public DateTime? LeaveDate { get; set; }

        /// <summary>
        /// Tỷ lệ allocation (%) - phân bổ thời gian cho dự án.
        /// </summary>
        [Range(0, 100)]
        public decimal AllocationPercent { get; set; } = 100;

        /// <summary>
        /// Chi phí/giờ (billing rate).
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal? HourlyRate { get; set; }

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
