using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.HRM
{
    /// <summary>
    /// Candidate - Ứng viên tuyển dụng.
    /// </summary>
    public class Candidate : BaseEntity
    {
        /// <summary>
        /// ID tin tuyển dụng.
        /// </summary>
        public Guid RecruitmentId { get; set; }
        public virtual Recruitment Recruitment { get; set; } = null!;

        /// <summary>
        /// Họ và tên ứng viên.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Email.
        /// </summary>
        [Required]
        [MaxLength(200)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Số điện thoại.
        /// </summary>
        [MaxLength(20)]
        [Phone]
        public string? Phone { get; set; }

        /// <summary>
        /// Trạng thái ứng viên.
        /// </summary>
        public CandidateStatus Status { get; set; } = CandidateStatus.New;

        /// <summary>
        /// Nguồn ứng viên.
        /// </summary>
        [MaxLength(100)]
        public string? Source { get; set; }

        /// <summary>
        /// Kinh nghiệm (năm).
        /// </summary>
        public int? YearsOfExperience { get; set; }

        /// <summary>
        /// Mức lương mong muốn.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal? ExpectedSalary { get; set; }

        /// <summary>
        /// Ngày có thể bắt đầu.
        /// </summary>
        public DateTime? AvailableDate { get; set; }

        /// <summary>
        /// Link CV (URL).
        /// </summary>
        [MaxLength(500)]
        public string? ResumeUrl { get; set; }

        /// <summary>
        /// Ghi chú đánh giá.
        /// </summary>
        [MaxLength(2000)]
        public string? EvaluationNotes { get; set; }

        /// <summary>
        /// Điểm đánh giá (1-100).
        /// </summary>
        [Range(0, 100)]
        public decimal? Rating { get; set; }

        /// <summary>
        /// Ngày phỏng vấn.
        /// </summary>
        public DateTime? InterviewDate { get; set; }

        /// <summary>
        /// Người phỏng vấn.
        /// </summary>
        public Guid? InterviewerUserId { get; set; }

        /// <summary>
        /// Lý do từ chối (nếu rejected).
        /// </summary>
        [MaxLength(500)]
        public string? RejectionReason { get; set; }
    }
}
