using NovaSaaS.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Inventory
{
    /// <summary>
    /// Danh mục sản phẩm - Phân loại sản phẩm theo nhóm.
    /// </summary>
    public class Category : BaseEntity
    {
        /// <summary>
        /// Tên danh mục.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả danh mục.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// ID danh mục cha (hỗ trợ cây danh mục).
        /// </summary>
        public Guid? ParentId { get; set; }
        public virtual Category? Parent { get; set; }

        /// <summary>
        /// Thứ tự hiển thị.
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// Trạng thái hoạt động.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual ICollection<Category> Children { get; set; } = new List<Category>();
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
