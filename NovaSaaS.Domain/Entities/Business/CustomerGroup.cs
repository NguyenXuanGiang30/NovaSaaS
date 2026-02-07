using NovaSaaS.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Business
{
    /// <summary>
    /// Nhóm khách hàng - Phân loại khách hàng theo nhóm/phân khúc.
    /// </summary>
    public class CustomerGroup : BaseEntity
    {
        /// <summary>
        /// Tên nhóm khách hàng.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả nhóm.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Phần trăm giảm giá mặc định cho nhóm.
        /// </summary>
        [Range(0, 100)]
        public decimal DiscountPercent { get; set; } = 0;

        /// <summary>
        /// Trạng thái hoạt động.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
    }
}
