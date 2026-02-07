using NovaSaaS.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.HRM
{
    /// <summary>
    /// Position - Chức vụ / Vị trí công việc.
    /// </summary>
    public class Position : BaseEntity
    {
        /// <summary>
        /// Mã chức vụ.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên chức vụ.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả chức vụ.
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// ID phòng ban (chức vụ thuộc phòng nào).
        /// </summary>
        public Guid DepartmentId { get; set; }
        public virtual Department Department { get; set; } = null!;

        /// <summary>
        /// Cấp bậc (level) để phân cấp quản lý.
        /// </summary>
        public int Level { get; set; } = 0;

        /// <summary>
        /// Mức lương cơ bản tối thiểu.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal? MinSalary { get; set; }

        /// <summary>
        /// Mức lương cơ bản tối đa.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal? MaxSalary { get; set; }

        /// <summary>
        /// Số lượng nhân sự tối đa cho vị trí này.
        /// </summary>
        public int? MaxHeadcount { get; set; }

        /// <summary>
        /// Trạng thái hoạt động.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
