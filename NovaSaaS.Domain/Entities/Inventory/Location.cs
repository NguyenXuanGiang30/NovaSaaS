using NovaSaaS.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Inventory
{
    /// <summary>
    /// Location - Vị trí trong kho hàng (kệ, ngăn, dãy...).
    /// </summary>
    public class Location : BaseEntity
    {
        /// <summary>
        /// ID kho hàng.
        /// </summary>
        public Guid WarehouseId { get; set; }
        public virtual Warehouse Warehouse { get; set; } = null!;

        /// <summary>
        /// Mã vị trí (ví dụ: A-01-02).
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên vị trí.
        /// </summary>
        [MaxLength(100)]
        public string? Name { get; set; }

        /// <summary>
        /// Dãy.
        /// </summary>
        [MaxLength(20)]
        public string? Aisle { get; set; }

        /// <summary>
        /// Kệ.
        /// </summary>
        [MaxLength(20)]
        public string? Rack { get; set; }

        /// <summary>
        /// Ngăn.
        /// </summary>
        [MaxLength(20)]
        public string? Shelf { get; set; }

        /// <summary>
        /// Trạng thái hoạt động.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Sức chứa tối đa (số lượng đơn vị).
        /// </summary>
        public int? MaxCapacity { get; set; }
    }
}
