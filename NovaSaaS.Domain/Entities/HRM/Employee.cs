using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.HRM
{
    /// <summary>
    /// Employee - Nhân viên trong tổ chức.
    /// Liên kết với User account (Identity).
    /// </summary>
    public class Employee : BaseEntity
    {
        /// <summary>
        /// Mã nhân viên (duy nhất trong tenant).
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string EmployeeCode { get; set; } = string.Empty;

        /// <summary>
        /// Họ và tên.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Giới tính.
        /// </summary>
        public Gender Gender { get; set; } = Gender.Male;

        /// <summary>
        /// Ngày sinh.
        /// </summary>
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// Số CMND / CCCD.
        /// </summary>
        [MaxLength(20)]
        public string? IdentityNumber { get; set; }

        /// <summary>
        /// Ngày cấp CMND.
        /// </summary>
        public DateTime? IdentityIssuedDate { get; set; }

        /// <summary>
        /// Nơi cấp CMND.
        /// </summary>
        [MaxLength(200)]
        public string? IdentityIssuedPlace { get; set; }

        /// <summary>
        /// Tình trạng hôn nhân.
        /// </summary>
        public MaritalStatus MaritalStatus { get; set; } = MaritalStatus.Single;

        /// <summary>
        /// Email cá nhân.
        /// </summary>
        [MaxLength(200)]
        [EmailAddress]
        public string? PersonalEmail { get; set; }

        /// <summary>
        /// Email công ty.
        /// </summary>
        [MaxLength(200)]
        [EmailAddress]
        public string? WorkEmail { get; set; }

        /// <summary>
        /// Số điện thoại.
        /// </summary>
        [MaxLength(20)]
        [Phone]
        public string? Phone { get; set; }

        /// <summary>
        /// Địa chỉ thường trú.
        /// </summary>
        [MaxLength(500)]
        public string? PermanentAddress { get; set; }

        /// <summary>
        /// Địa chỉ hiện tại.
        /// </summary>
        [MaxLength(500)]
        public string? CurrentAddress { get; set; }

        /// <summary>
        /// Ảnh đại diện.
        /// </summary>
        [MaxLength(500)]
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// ID phòng ban.
        /// </summary>
        public Guid DepartmentId { get; set; }
        public virtual Department Department { get; set; } = null!;

        /// <summary>
        /// ID chức vụ.
        /// </summary>
        public Guid PositionId { get; set; }
        public virtual Position Position { get; set; } = null!;

        /// <summary>
        /// ID quản lý trực tiếp.
        /// </summary>
        public Guid? ManagerId { get; set; }
        public virtual Employee? Manager { get; set; }

        /// <summary>
        /// Trạng thái nhân sự.
        /// </summary>
        public EmploymentStatus Status { get; set; } = EmploymentStatus.Active;

        /// <summary>
        /// Ngày vào công ty.
        /// </summary>
        public DateTime JoinDate { get; set; }

        /// <summary>
        /// Ngày nghỉ việc (nếu có).
        /// </summary>
        public DateTime? TerminationDate { get; set; }

        /// <summary>
        /// Loại hợp đồng hiện tại.
        /// </summary>
        public ContractType ContractType { get; set; } = ContractType.Permanent;

        /// <summary>
        /// Lương cơ bản hiện tại.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal BaseSalary { get; set; } = 0;

        /// <summary>
        /// Số tài khoản ngân hàng.
        /// </summary>
        [MaxLength(30)]
        public string? BankAccountNumber { get; set; }

        /// <summary>
        /// Tên ngân hàng.
        /// </summary>
        [MaxLength(100)]
        public string? BankName { get; set; }

        /// <summary>
        /// Chi nhánh ngân hàng.
        /// </summary>
        [MaxLength(100)]
        public string? BankBranch { get; set; }

        /// <summary>
        /// Mã số thuế cá nhân.
        /// </summary>
        [MaxLength(20)]
        public string? TaxCode { get; set; }

        /// <summary>
        /// Số sổ BHXH.
        /// </summary>
        [MaxLength(20)]
        public string? SocialInsuranceNumber { get; set; }

        /// <summary>
        /// ID User account (liên kết Identity module).
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(2000)]
        public string? Notes { get; set; }

        // Navigation
        public virtual ICollection<Employee> DirectReports { get; set; } = new List<Employee>();
        public virtual ICollection<EmployeeContract> Contracts { get; set; } = new List<EmployeeContract>();
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
        public virtual ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
        public virtual ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
        public virtual ICollection<PerformanceReview> PerformanceReviews { get; set; } = new List<PerformanceReview>();
    }
}
