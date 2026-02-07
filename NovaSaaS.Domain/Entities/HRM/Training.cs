using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.HRM
{
    /// <summary>
    /// Training - Khóa đào tạo.
    /// </summary>
    public class Training : BaseEntity
    {
        /// <summary>
        /// Mã khóa đào tạo.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên khóa đào tạo.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả nội dung.
        /// </summary>
        [MaxLength(2000)]
        public string? Description { get; set; }

        /// <summary>
        /// Giảng viên / Đơn vị đào tạo.
        /// </summary>
        [MaxLength(200)]
        public string? Trainer { get; set; }

        /// <summary>
        /// Địa điểm đào tạo.
        /// </summary>
        [MaxLength(200)]
        public string? Location { get; set; }

        /// <summary>
        /// Ngày bắt đầu.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Trạng thái khóa đào tạo.
        /// </summary>
        public TrainingStatus Status { get; set; } = TrainingStatus.Planned;

        /// <summary>
        /// Số lượng tối đa.
        /// </summary>
        public int? MaxParticipants { get; set; }

        /// <summary>
        /// Chi phí đào tạo.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal Cost { get; set; } = 0;

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        // Navigation
        public virtual ICollection<TrainingParticipant> Participants { get; set; } = new List<TrainingParticipant>();
    }

    /// <summary>
    /// TrainingParticipant - Học viên tham gia khóa đào tạo.
    /// </summary>
    public class TrainingParticipant : BaseEntity
    {
        /// <summary>
        /// ID khóa đào tạo.
        /// </summary>
        public Guid TrainingId { get; set; }
        public virtual Training Training { get; set; } = null!;

        /// <summary>
        /// ID nhân viên.
        /// </summary>
        public Guid EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;

        /// <summary>
        /// Trạng thái tham gia.
        /// </summary>
        public TrainingParticipantStatus Status { get; set; } = TrainingParticipantStatus.Registered;

        /// <summary>
        /// Tiến độ hoàn thành (%).
        /// </summary>
        [Range(0, 100)]
        public decimal Progress { get; set; } = 0;

        /// <summary>
        /// Điểm đánh giá (nếu có bài kiểm tra).
        /// </summary>
        [Range(0, 100)]
        public decimal? Score { get; set; }

        /// <summary>
        /// Ngày hoàn thành.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Nhận xét.
        /// </summary>
        [MaxLength(500)]
        public string? Feedback { get; set; }
    }

    /// <summary>
    /// Trạng thái tham gia đào tạo.
    /// </summary>
    public enum TrainingParticipantStatus
    {
        Registered = 0,
        InProgress = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 4
    }
}
