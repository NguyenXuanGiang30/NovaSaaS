using NovaSaaS.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.HRM
{
    /// <summary>
    /// Department - Phòng ban trong tổ chức.
    /// Hỗ trợ cấu trúc cây phân cấp phòng ban.
    /// </summary>
    public class Department : BaseEntity
    {
        /// <summary>
        /// Mã phòng ban (duy nhất).
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên phòng ban.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// ID phòng ban cha (hỗ trợ cây phân cấp).
        /// </summary>
        public Guid? ParentDepartmentId { get; set; }
        public virtual Department? ParentDepartment { get; set; }

        /// <summary>
        /// ID trưởng phòng.
        /// </summary>
        public Guid? ManagerEmployeeId { get; set; }
        public virtual Employee? ManagerEmployee { get; set; }

        /// <summary>
        /// Số điện thoại phòng ban.
        /// </summary>
        [MaxLength(20)]
        public string? Phone { get; set; }

        /// <summary>
        /// Email phòng ban.
        /// </summary>
        [MaxLength(200)]
        public string? Email { get; set; }

        /// <summary>
        /// Vị trí / Tầng / Tòa nhà.
        /// </summary>
        [MaxLength(200)]
        public string? Location { get; set; }

        /// <summary>
        /// Thứ tự hiển thị.
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// Trạng thái hoạt động.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual ICollection<Department> ChildDepartments { get; set; } = new List<Department>();
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public virtual ICollection<Position> Positions { get; set; } = new List<Position>();
    }
}
