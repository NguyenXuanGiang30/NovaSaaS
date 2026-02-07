using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.HRM
{
    /// <summary>
    /// PayrollPeriod - Kỳ lương (tháng).
    /// </summary>
    public class PayrollPeriod : BaseEntity
    {
        /// <summary>
        /// Tên kỳ lương (VD: "Tháng 01/2026").
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tháng.
        /// </summary>
        [Range(1, 12)]
        public int Month { get; set; }

        /// <summary>
        /// Năm.
        /// </summary>
        [Range(2020, 2100)]
        public int Year { get; set; }

        /// <summary>
        /// Ngày bắt đầu kỳ.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc kỳ.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Trạng thái kỳ lương.
        /// </summary>
        public PayrollStatus Status { get; set; } = PayrollStatus.Draft;

        /// <summary>
        /// Tổng chi phí lương trong kỳ.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal TotalGrossSalary { get; set; } = 0;

        /// <summary>
        /// Tổng lương NET trong kỳ.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal TotalNetSalary { get; set; } = 0;

        /// <summary>
        /// Tổng số nhân viên trong kỳ.
        /// </summary>
        public int TotalEmployees { get; set; } = 0;

        /// <summary>
        /// Người duyệt.
        /// </summary>
        public Guid? ApprovedByUserId { get; set; }

        /// <summary>
        /// Ngày duyệt.
        /// </summary>
        public DateTime? ApprovedAt { get; set; }

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        // Navigation
        public virtual ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
    }
}
