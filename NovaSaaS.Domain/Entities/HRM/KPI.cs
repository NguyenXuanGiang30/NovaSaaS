using NovaSaaS.Domain.Entities.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.HRM
{
    /// <summary>
    /// KPI - Chỉ số hiệu suất (Key Performance Indicator).
    /// </summary>
    public class KPI : BaseEntity
    {
        /// <summary>
        /// ID bài đánh giá.
        /// </summary>
        public Guid PerformanceReviewId { get; set; }
        public virtual PerformanceReview PerformanceReview { get; set; } = null!;

        /// <summary>
        /// Tên chỉ số KPI.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả chỉ số.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Mục tiêu (target).
        /// </summary>
        [MaxLength(200)]
        public string? Target { get; set; }

        /// <summary>
        /// Kết quả thực tế.
        /// </summary>
        [MaxLength(200)]
        public string? ActualResult { get; set; }

        /// <summary>
        /// Trọng số (%) - tổng trọng số các KPI = 100%.
        /// </summary>
        [Range(0, 100)]
        public decimal Weight { get; set; } = 0;

        /// <summary>
        /// Điểm đạt được (0-100).
        /// </summary>
        [Range(0, 100)]
        public decimal? Score { get; set; }

        /// <summary>
        /// Điểm có trọng số = Score * Weight / 100.
        /// </summary>
        public decimal? WeightedScore => Score.HasValue ? Score.Value * Weight / 100 : null;

        /// <summary>
        /// Nhận xét.
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
