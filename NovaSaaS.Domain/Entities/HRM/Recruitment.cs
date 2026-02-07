using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.HRM
{
    /// <summary>
    /// Recruitment - Tin tuyển dụng / Yêu cầu tuyển dụng.
    /// </summary>
    public class Recruitment : BaseEntity
    {
        /// <summary>
        /// Mã tin tuyển dụng.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tiêu đề tin tuyển dụng.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// ID phòng ban cần tuyển.
        /// </summary>
        public Guid DepartmentId { get; set; }
        public virtual Department Department { get; set; } = null!;

        /// <summary>
        /// ID vị trí cần tuyển.
        /// </summary>
        public Guid PositionId { get; set; }
        public virtual Position Position { get; set; } = null!;

        /// <summary>
        /// Số lượng cần tuyển.
        /// </summary>
        [Range(1, 100)]
        public int Headcount { get; set; } = 1;

        /// <summary>
        /// Mô tả công việc.
        /// </summary>
        [MaxLength(5000)]
        public string? JobDescription { get; set; }

        /// <summary>
        /// Yêu cầu ứng viên.
        /// </summary>
        [MaxLength(5000)]
        public string? Requirements { get; set; }

        /// <summary>
        /// Quyền lợi.
        /// </summary>
        [MaxLength(5000)]
        public string? Benefits { get; set; }

        /// <summary>
        /// Mức lương tối thiểu.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal? MinSalary { get; set; }

        /// <summary>
        /// Mức lương tối đa.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal? MaxSalary { get; set; }

        /// <summary>
        /// Địa điểm làm việc.
        /// </summary>
        [MaxLength(200)]
        public string? WorkLocation { get; set; }

        /// <summary>
        /// Trạng thái tin tuyển dụng.
        /// </summary>
        public RecruitmentStatus Status { get; set; } = RecruitmentStatus.Open;

        /// <summary>
        /// Ngày mở.
        /// </summary>
        public DateTime OpenDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ngày đóng (deadline).
        /// </summary>
        public DateTime? CloseDate { get; set; }

        /// <summary>
        /// Người yêu cầu tuyển dụng.
        /// </summary>
        public Guid? RequestedByUserId { get; set; }

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        // Navigation
        public virtual ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();
    }
}
