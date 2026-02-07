using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.HRM
{
    /// <summary>
    /// Payroll - Phiếu lương nhân viên trong kỳ.
    /// </summary>
    public class Payroll : BaseEntity
    {
        /// <summary>
        /// ID kỳ lương.
        /// </summary>
        public Guid PayrollPeriodId { get; set; }
        public virtual PayrollPeriod PayrollPeriod { get; set; } = null!;

        /// <summary>
        /// ID nhân viên.
        /// </summary>
        public Guid EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;

        /// <summary>
        /// Lương cơ bản.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal BaseSalary { get; set; }

        /// <summary>
        /// Số ngày công thực tế.
        /// </summary>
        [Range(0, 31)]
        public decimal WorkDays { get; set; }

        /// <summary>
        /// Số ngày công chuẩn.
        /// </summary>
        [Range(0, 31)]
        public decimal StandardWorkDays { get; set; }

        /// <summary>
        /// Lương theo ngày công.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal ProRataSalary { get; set; }

        /// <summary>
        /// Tiền tăng ca.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal OvertimePay { get; set; } = 0;

        /// <summary>
        /// Tổng phụ cấp.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal TotalAllowance { get; set; } = 0;

        /// <summary>
        /// Tổng thưởng.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal TotalBonus { get; set; } = 0;

        /// <summary>
        /// Tổng lương GROSS.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal GrossSalary { get; set; }

        /// <summary>
        /// BHXH (nhân viên đóng).
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal SocialInsurance { get; set; } = 0;

        /// <summary>
        /// BHYT (nhân viên đóng).
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal HealthInsurance { get; set; } = 0;

        /// <summary>
        /// BHTN (nhân viên đóng).
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal UnemploymentInsurance { get; set; } = 0;

        /// <summary>
        /// Thuế TNCN.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal IncomeTax { get; set; } = 0;

        /// <summary>
        /// Tổng khấu trừ khác.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal TotalDeduction { get; set; } = 0;

        /// <summary>
        /// Lương NET (thực nhận).
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal NetSalary { get; set; }

        /// <summary>
        /// Trạng thái phiếu lương.
        /// </summary>
        public PayrollStatus Status { get; set; } = PayrollStatus.Draft;

        /// <summary>
        /// Ngày thanh toán.
        /// </summary>
        public DateTime? PaidDate { get; set; }

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        // Navigation
        public virtual ICollection<PayrollDetail> Details { get; set; } = new List<PayrollDetail>();
    }

    /// <summary>
    /// PayrollDetail - Chi tiết các khoản thưởng/khấu trừ trong phiếu lương.
    /// </summary>
    public class PayrollDetail : BaseEntity
    {
        /// <summary>
        /// ID phiếu lương.
        /// </summary>
        public Guid PayrollId { get; set; }
        public virtual Payroll Payroll { get; set; } = null!;

        /// <summary>
        /// Tên khoản (VD: "Thưởng KPI", "Khấu trừ đi trễ").
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string ItemName { get; set; } = string.Empty;

        /// <summary>
        /// Loại: Bonus hoặc Deduction.
        /// </summary>
        public PayrollItemType ItemType { get; set; }

        /// <summary>
        /// Số tiền.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Loại khoản trong phiếu lương.
    /// </summary>
    public enum PayrollItemType
    {
        Bonus = 0,
        Deduction = 1,
        Allowance = 2
    }
}
