using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.HRM
{
    /// <summary>
    /// PerformanceReview - Đánh giá hiệu suất nhân viên.
    /// </summary>
    public class PerformanceReview : BaseEntity
    {
        /// <summary>
        /// ID nhân viên được đánh giá.
        /// </summary>
        public Guid EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;

        /// <summary>
        /// ID người đánh giá (quản lý).
        /// </summary>
        public Guid? ReviewerUserId { get; set; }

        /// <summary>
        /// Loại kỳ đánh giá.
        /// </summary>
        public ReviewPeriodType PeriodType { get; set; } = ReviewPeriodType.Quarterly;

        /// <summary>
        /// Năm đánh giá.
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Quý / Tháng / Kỳ đánh giá.
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// Trạng thái đánh giá.
        /// </summary>
        public ReviewStatus Status { get; set; } = ReviewStatus.Draft;

        /// <summary>
        /// Điểm tự đánh giá (1-100).
        /// </summary>
        [Range(0, 100)]
        public decimal? SelfScore { get; set; }

        /// <summary>
        /// Nhận xét tự đánh giá.
        /// </summary>
        [MaxLength(2000)]
        public string? SelfComment { get; set; }

        /// <summary>
        /// Điểm đánh giá từ quản lý (1-100).
        /// </summary>
        [Range(0, 100)]
        public decimal? ManagerScore { get; set; }

        /// <summary>
        /// Nhận xét từ quản lý.
        /// </summary>
        [MaxLength(2000)]
        public string? ManagerComment { get; set; }

        /// <summary>
        /// Điểm tổng kết cuối cùng.
        /// </summary>
        [Range(0, 100)]
        public decimal? FinalScore { get; set; }

        /// <summary>
        /// Xếp loại (A, B, C, D...).
        /// </summary>
        [MaxLength(10)]
        public string? Rating { get; set; }

        /// <summary>
        /// Mục tiêu cho kỳ tiếp theo.
        /// </summary>
        [MaxLength(2000)]
        public string? GoalsForNextPeriod { get; set; }

        /// <summary>
        /// Ngày hoàn thành đánh giá.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        // Navigation
        public virtual ICollection<KPI> KPIs { get; set; } = new List<KPI>();
    }
}
