using NovaSaaS.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Inventory
{
    /// <summary>
    /// Unit - Đơn vị tính sản phẩm (cái, kg, hộp, ...).
    /// </summary>
    public class Unit : BaseEntity
    {
        /// <summary>
        /// Tên đơn vị tính.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tên viết tắt.
        /// </summary>
        [MaxLength(10)]
        public string ShortName { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả.
        /// </summary>
        [MaxLength(200)]
        public string? Description { get; set; }

        /// <summary>
        /// Trạng thái hoạt động.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
